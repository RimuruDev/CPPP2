using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Internal
{
    public static class Constants
    {
        public const string ROOT_FOLDER_NAME = "Database";
        public const string USER_PROGRESS_FILE = "user_progress";
        public const string AUDIO_SETTINGS_FILE = "audio_settings";
    }
    
    public class MobileProgressService : IProgressService
    {
        private readonly string directoryPath;
        private readonly IDataStorage dataStorage;
        private readonly IProgressValidator validator;
        private readonly IEncryptionService encryptionService;
        private readonly IFileFormatConfiguration fileFormatConfig;
        private readonly IProgressMigrationService migrationService;

        private readonly Dictionary<string, Action> idToLoadAction;
        private readonly Dictionary<string, Action> idToSaveAction;
        private readonly Dictionary<string, Action> idToDeleteAction;

        public MobileProgressService(
            IFileFormatConfiguration fileFormatConfig,
            IDataStorage dataStorage,
            IEncryptionService encryptionService,
            IProgressValidator validator,
            IProgressMigrationService migrationService)
        {
            this.fileFormatConfig = fileFormatConfig;
            this.dataStorage = dataStorage;
            this.encryptionService = encryptionService;
            this.validator = validator;
            this.migrationService = migrationService;

            directoryPath = Path.Combine(Application.persistentDataPath, Constants.ROOT_FOLDER_NAME);

            if(!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            const string userProgressFile = Constants.USER_PROGRESS_FILE;
            const string audioSettingsFile = Constants.AUDIO_SETTINGS_FILE;
            
            // Маппинг ID на действия //
            idToLoadAction = new Dictionary<string, Action>
            {
                { userProgressFile, () => UserProgress = new UserProgressProxy(LoadProgress(userProgressFile, DefaultProgressFactory.CreateDefaultProgress)) },
                { audioSettingsFile, () => AudioSettings = new AudioSettingsProxy(LoadProgress(audioSettingsFile, DefaultProgressFactory.CreateDefaultAudioSettings)) }
            };

            // Маппинг ID на действия сохранения //
            idToSaveAction = new Dictionary<string, Action>
            {
                { userProgressFile, () => SaveProgress(userProgressFile, UserProgress.Origin) },
                { audioSettingsFile, () => SaveProgress(audioSettingsFile, AudioSettings.Origin) }
            };

            // Маппинг ID на действия удаления //
            idToDeleteAction = new Dictionary<string, Action>
            {
                { userProgressFile, () => DeleteProgress(userProgressFile) },
                { audioSettingsFile, () => DeleteProgress(audioSettingsFile) }
            };
        }

        #region API
        
        public IUserProgressProxy UserProgress { get; private set; }
        public IAudioSettingsProxy AudioSettings { get; private set; }

        public void SaveAllProgress()
        {
            foreach (var action in idToSaveAction.Values)
            {
                action?.Invoke();
            }

            Debug.Log("All progress saved successfully.");
        }

        public void LoadProgress()
        {
            foreach (var action in idToLoadAction.Values)
            {
                action?.Invoke();
            }

            Debug.Log("All progress loaded successfully.");
        }

        public void DeleteAllProgress()
        {
            foreach (var action in idToDeleteAction.Values)
            {
                action?.Invoke();
            }

            Debug.Log("All progress deleted successfully.");
        }

        public void SaveProgressById(string id)
        {
            if (idToSaveAction.TryGetValue(id, out var action))
            {
                action?.Invoke();
                Debug.Log($"Successfully saved progress for ID: {id}");
            }
            else
            {
                Debug.LogWarning($"Unknown progress ID: {id}");
            }
        }

        public void LoadProgressById(string id)
        {
            if (idToLoadAction.TryGetValue(id, out var action))
            {
                action?.Invoke();
                Debug.Log($"Successfully loaded progress for ID: {id}");
            }
            else
            {
                Debug.LogWarning($"Unknown progress ID: {id}");
            }
        }

        public void DeleteProgressById(string id)
        {
            if (idToDeleteAction.TryGetValue(id, out var action))
            {
                action?.Invoke();
                Debug.Log($"Successfully deleted progress for ID: {id}");
            }
            else
            {
                Debug.LogWarning($"Unknown progress ID: {id}");
            }
        }

        public void Dispose()
        {
            UserProgress?.Dispose();
            AudioSettings?.Dispose();
            
            idToLoadAction?.Clear();
            idToSaveAction?.Clear();
            idToDeleteAction?.Clear();
        }

        #endregion

        #region Abstraction

        private void SaveProgress<T>(string fileName, T data)
        {
            var savePath = Path.Combine(directoryPath, fileName + fileFormatConfig.CurrentFormatHandler.GetFileExtension());
            var rawData = fileFormatConfig.CurrentFormatHandler.Serialize(data);
            var encryptedData = encryptionService.Encrypt(rawData);

            // Сохранение основного файла //
            // NOTE: Покрыть тестами на время выполнения.
            dataStorage.Save(savePath, encryptedData);

#if UNITY_EDITOR
            var editorPath = Path.Combine(directoryPath, fileName + ".editor" + fileFormatConfig.CurrentFormatHandler.GetFileExtension());

            // Удаление устаревших файлов редактора //
            foreach (var handler in fileFormatConfig.SupportedFormatHandlers)
            {
                var legacyEditorPath = Path.Combine(directoryPath, fileName + ".editor" + handler.GetFileExtension());
                if (legacyEditorPath != editorPath && dataStorage.Exists(legacyEditorPath))
                {
                    dataStorage.Delete(legacyEditorPath);
                    Debug.Log($"Deleted legacy editor file: {legacyEditorPath}");
                }
            }

            // NOTE: Сохранение редакторского файла //
            // Это блин важно! Потом вынеси для API с ключами для сейва.
            try
            {
                // Редакторский файл сохраняется в оригинальном виде
                // Иначе куча ошибок, да и тяжко это постоянно делать пусть и для редактора.
                var editorData = rawData;
                dataStorage.Save(editorPath, editorData);
                Debug.Log($"Editor-readable file saved to: {editorPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save editor file: {ex.Message}");
            }
#endif

            Debug.Log($"Progress saved to: {savePath}");
        }

        private TData LoadProgress<TData>(string fileName, Func<TData> createDefault) where TData : class
        {
            var filePath = Path.Combine(directoryPath, fileName + fileFormatConfig.CurrentFormatHandler.GetFileExtension());

            // === Выполняем миграцию, если необходимо! *Позже так же нужно будет упростить это место. 
            if (!dataStorage.Exists(filePath))
            {
                Debug.Log($"File for '{fileName}' not found. Attempting migration.");

                Type modelType = typeof(TData);

                var migrated = migrationService.TryMigrate(
                    directoryPath,
                    fileName,
                    fileFormatConfig.CurrentFormatHandler,
                    fileFormatConfig.SupportedFormatHandlers,
                    modelType);

                if (!migrated)
                {
                    Debug.LogWarning($"No valid save file found for '{fileName}' after migration. Initializing default data.");
                    return createDefault();
                }
            }

            var rawData = dataStorage.Load(filePath);
            var decryptedData = encryptionService.IsEncrypted(rawData) ? encryptionService.Decrypt(rawData) : rawData;

            try
            {
                var progress = fileFormatConfig.CurrentFormatHandler.Deserialize<TData>(decryptedData);

                // === Валидация данных! | Я точно однажды забуду про это место, черкануть в доку нужно 100%
                if (typeof(TData) == typeof(UserProgress) && !validator.IsValid((UserProgress)(object)progress))
                    throw new InvalidDataException("Invalid UserProgress data.");

                if (typeof(TData) == typeof(AudioSettings) && !validator.IsValid((AudioSettings)(object)progress))
                    throw new InvalidDataException("Invalid AudioSettings data.");

                return progress;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load progress: {e.Message}. Initializing default data.");
                return createDefault();
            }
        }

        private void DeleteProgress(string fileName)
        {
            Debug.Log($"<color=red>Deleting file '{fileName}'.</color>");
            var filePath = Path.Combine(directoryPath, fileName + fileFormatConfig.CurrentFormatHandler.GetFileExtension());

            if (dataStorage.Exists(filePath))
            {
                dataStorage.Delete(filePath);
                Debug.Log($"Progress deleted: {filePath}");
            }

#if UNITY_EDITOR
            foreach (var handler in fileFormatConfig.SupportedFormatHandlers)
            {
                var editorPath = Path.Combine(directoryPath, fileName + ".editor" + handler.GetFileExtension());
                if (dataStorage.Exists(editorPath))
                {
                    dataStorage.Delete(editorPath);
                    Debug.Log($"Deleted editor file: {editorPath}");
                }
            }
#endif
        }

        #endregion
    }
}
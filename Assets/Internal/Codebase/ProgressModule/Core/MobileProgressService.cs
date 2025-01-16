using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Internal
{
    public class MobileProgressService : IProgressService
    {
        private const string rootFolderPath = "Database";
        private const string userProgressFile = "user_progress";
        private const string audioSettingsFile = "audio_settings";

        private readonly string directoryPath;
        private readonly IFileFormatConfiguration fileFormatConfig;
        private readonly IDataStorage dataStorage;
        private readonly IEncryptionService encryptionService;
        private readonly IProgressValidator validator;
        private readonly IProgressMigrationService migrationService;

        private readonly Dictionary<string, Func<object>> progressMappings;

        public IUserProgressProxy UserProgress { get; private set; }
        public IAudioSettingsProxy AudioSettings { get; private set; }

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

            directoryPath = Path.Combine(Application.persistentDataPath, rootFolderPath);

            progressMappings = new Dictionary<string, Func<object>>
            {
                {
                    userProgressFile, () =>
                    {
                        if (UserProgress != null)
                            return UserProgress.Origin;

                        return DefaultProgressFactory.CreateDefaultProgress();
                    }
                },
                {
                    audioSettingsFile, () =>
                    {
                        if (AudioSettings != null) return AudioSettings.Origin;
                        return DefaultProgressFactory.CreateDefaultAudioSettings();
                    }
                }
            };

            Directory.CreateDirectory(directoryPath);
        }

        public void SaveAllProgress()
        {
            SaveProgress(userProgressFile, UserProgress.Origin);
            SaveProgress(audioSettingsFile, AudioSettings.Origin);
        }

        public void LoadProgress()
        {
            UserProgress =
                new UserProgressProxy(LoadProgress(userProgressFile, DefaultProgressFactory.CreateDefaultProgress));
            AudioSettings =
                new AudioSettingsProxy(LoadProgress(audioSettingsFile,
                    DefaultProgressFactory.CreateDefaultAudioSettings));
        }

        public void DeleteAllProgress()
        {
            DeleteProgress(userProgressFile);
            DeleteProgress(audioSettingsFile);
        }

        public void Dispose()
        {
            UserProgress?.Dispose();
            AudioSettings?.Dispose();
        }

        private void SaveProgress<T>(string fileName, T data)
        {
            var savePath = Path.Combine(directoryPath,
                fileName + fileFormatConfig.CurrentFormatHandler.GetFileExtension());
            var rawData = fileFormatConfig.CurrentFormatHandler.Serialize(data);
            var encryptedData = encryptionService.Encrypt(rawData);

            // Сохранение основного файла //
            // NOTE: Покрыть тестами на время выполнения.
            dataStorage.Save(savePath, encryptedData);

#if UNITY_EDITOR
            var editorPath = Path.Combine(directoryPath,
                fileName + ".editor" + fileFormatConfig.CurrentFormatHandler.GetFileExtension());

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

        private T LoadProgress<T>(string fileName, Func<T> createDefault) where T : class
        {
            var filePath = Path.Combine(directoryPath,
                fileName + fileFormatConfig.CurrentFormatHandler.GetFileExtension());

            // === Выполняем миграцию, если необходимо! *Позже так же нужно будет упростить это место. 
            if (!dataStorage.Exists(filePath))
            {
                Debug.Log($"File for '{fileName}' not found. Attempting migration.");

                Type modelType = typeof(T);

                var migrated = migrationService.TryMigrate(
                    directoryPath,
                    fileName,
                    fileFormatConfig.CurrentFormatHandler,
                    fileFormatConfig.SupportedFormatHandlers,
                    modelType);

                if (!migrated)
                {
                    Debug.LogWarning(
                        $"No valid save file found for '{fileName}' after migration. Initializing default data.");
                    return createDefault();
                }
            }

            var rawData = dataStorage.Load(filePath);
            var decryptedData = encryptionService.IsEncrypted(rawData) ? encryptionService.Decrypt(rawData) : rawData;

            try
            {
                var progress = fileFormatConfig.CurrentFormatHandler.Deserialize<T>(decryptedData);

                // === Валидация данных! | Я точно однажды забуду про это место, черкануть в доку нужно 100%
                if (typeof(T) == typeof(UserProgress) && !validator.IsValid((UserProgress)(object)progress))
                    throw new InvalidDataException("Invalid UserProgress data.");

                if (typeof(T) == typeof(AudioSettings) && !validator.IsValid((AudioSettings)(object)progress))
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
            var filePath = Path.Combine(directoryPath,
                fileName + fileFormatConfig.CurrentFormatHandler.GetFileExtension());

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

        #region IProgressService implementation

        public void SaveProgressById(string id)
        {
            if (!progressMappings.ContainsKey(id))
            {
                Debug.LogError($"Progress ID '{id}' not found.");
                return;
            }

            var savePath = Path.Combine(directoryPath, id + fileFormatConfig.CurrentFormatHandler.GetFileExtension());
            var rawData = fileFormatConfig.CurrentFormatHandler.Serialize(progressMappings[id]());
            var encryptedData = encryptionService.Encrypt(rawData);

            dataStorage.Save(savePath, encryptedData);

#if UNITY_EDITOR
            var editorPath = Path.Combine(directoryPath,
                id + ".editor" + fileFormatConfig.CurrentFormatHandler.GetFileExtension());
            var editorData = fileFormatConfig.CurrentFormatHandler.GetFileExtension() == ".rimuru"
                ? encryptionService.Decrypt(rawData)
                : rawData;
            dataStorage.Save(editorPath, editorData);
#endif

            Debug.Log($"Progress '{id}' saved to: {savePath}");
        }

        public void LoadProgressById(string id)
        {
            if (!progressMappings.ContainsKey(id))
            {
                Debug.LogError($"Progress ID '{id}' not found.");
                return;
            }

            var filePath = Path.Combine(directoryPath, id + fileFormatConfig.CurrentFormatHandler.GetFileExtension());
            if (!dataStorage.Exists(filePath))
            {
                Debug.LogWarning($"Progress file for '{id}' not found. Using default data.");
                return;
            }

            var rawData = dataStorage.Load(filePath);
            var decryptedData = encryptionService.IsEncrypted(rawData) ? encryptionService.Decrypt(rawData) : rawData;

            try
            {
                var modelType = progressMappings[id]().GetType();
                var progress = fileFormatConfig.CurrentFormatHandler.Deserialize(decryptedData, modelType);

                if (progress is UserProgress userProgress)
                {
                    //
                    // Потом как нибудь по красивее обернуть нужно эту темку. Так как забуду 100% о том что тут чищу.
                    //
                    if (UserProgress is { Origin: not null })
                        UserProgress?.Dispose();

                    UserProgress = new UserProgressProxy(userProgress);
                }

                if (progress is AudioSettings audioSettings)
                {
                    //
                    // Потом как нибудь по красивее обернуть нужно эту темку. Так как забуду 100% о том что тут чищу.
                    //
                    if (AudioSettings is { Origin: not null })
                        AudioSettings?.Dispose();

                    AudioSettings = new AudioSettingsProxy(audioSettings);
                }

                Debug.Log($"Progress '{id}' loaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load progress '{id}': {e.Message}. Using default data.");
            }
        }

        public void DeleteProgressById(string id)
        {
            if (!progressMappings.ContainsKey(id))
            {
                Debug.LogError($"Progress ID '{id}' not found.");
                return;
            }

            var filePath = Path.Combine(directoryPath, id + fileFormatConfig.CurrentFormatHandler.GetFileExtension());
            if (dataStorage.Exists(filePath))
            {
                dataStorage.Delete(filePath);
                Debug.Log($"Progress '{id}' deleted.");
            }

#if UNITY_EDITOR
            foreach (var handler in fileFormatConfig.SupportedFormatHandlers)
            {
                var editorPath = Path.Combine(directoryPath, id + ".editor" + handler.GetFileExtension());
                if (dataStorage.Exists(editorPath))
                {
                    dataStorage.Delete(editorPath);
                    Debug.Log($"Editor file for '{id}' deleted.");
                }
            }
#endif
        }

        #endregion
    }
}
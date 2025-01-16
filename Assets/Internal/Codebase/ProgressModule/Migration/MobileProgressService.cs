using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using AudioSettings = Internal.AudioSettings;

namespace Internal
{
    public interface IDataStorage
    {
        public void Save(string path, string data);
        public string Load(string path);
        public void Delete(string path);
        public bool Exists(string path);
    }

    public class FileDataStorage : IDataStorage
    {
        public void Save(string path, string data)
        {
            File.WriteAllText(path, data);
        }

        public string Load(string path)
        {
            return File.ReadAllText(path);
        }

        public void Delete(string path)
        {
            if (Exists(path))
                File.Delete(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }

    public interface IFileFormatConfiguration
    {
        public IFileFormatHandler CurrentFormatHandler { get; }
        public List<IFileFormatHandler> SupportedFormatHandlers { get; }
    }

    public class FileFormatConfiguration : IFileFormatConfiguration
    {
        public IFileFormatHandler CurrentFormatHandler { get; }
        public List<IFileFormatHandler> SupportedFormatHandlers { get; }

        public FileFormatConfiguration(IFileFormatHandler currentHandler, List<IFileFormatHandler> supportedHandlers)
        {
            CurrentFormatHandler = currentHandler;
            SupportedFormatHandlers = supportedHandlers;
        }
    }

    public class MobileProgressService : IProgressService
    {
#if UNITY_EDITOR
        private readonly string fullPathEditor;
#endif

        private readonly string directoryPath;
        private readonly string userProgressFile;
        private readonly string audioSettingsFile;
        private readonly IFileFormatConfiguration fileFormatConfig;
        private readonly IDataStorage dataStorage;
        private readonly IEncryptionService encryptionService;
        private readonly IProgressValidator validator;
        private readonly IProgressMigrationService migrationService;

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

            directoryPath = Path.Combine(Application.persistentDataPath, "Database");
            userProgressFile = "user_progress";
            audioSettingsFile = "audio_settings";

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
                new UserProgressProxy(LoadProgress<UserProgress>(userProgressFile,
                    DefaultProgressFactory.CreateDefaultProgress));
            AudioSettings = new AudioSettingsProxy(LoadProgress<AudioSettings>(audioSettingsFile,
                DefaultProgressFactory.CreateDefaultAudioSettings));
        }

        public void DeleteAllProgress()
        {
            DeleteProgress(userProgressFile);
            DeleteProgress(audioSettingsFile);
        }

        public void Dispose()
        {
            // NOTE: Не забыть в конце избавиться от этого метода если в итоге модели не нужно будет освобождать.
            // Очистка ресурсов, если требуется //
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
    }

    public static class DefaultProgressFactory
    {
        public static UserProgress CreateDefaultProgress()
        {
            return new UserProgress
            {
                UserName = "Rimuru",
                Level = 1,
                HardCurrency = 0,
                SoftCurrency = 0,
            };
        }

        public static AudioSettings CreateDefaultAudioSettings()
        {
            return new AudioSettings
            {
                BackgroundMusicVolume = 1,
                SfxVolume = 1
            };
        }
    }
}
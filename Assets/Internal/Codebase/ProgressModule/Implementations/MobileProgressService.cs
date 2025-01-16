using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Internal.Codebase.ProgressModule.Core;
using Internal.Codebase.ProgressModule.Models;
using UnityEngine;
using Newtonsoft.Json;
using AudioSettings = Internal.Codebase.ProgressModule.Core.AudioSettings;

namespace Internal.Codebase.ProgressModule.Implementations
{
    public interface IEncryptionService
    {
        public string Encrypt(string plainText);
        public string Decrypt(string cipherText);

        public bool IsEncrypted(string data);
    }

    public class SimpleEncryptionService : IEncryptionService
    {
        // TODO: Замените на безопасный ключ.
        private const string Key = "my_secret_key";

        public string Encrypt(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);

            // TODO: Простая "заглушка" для шифрования 
            return Convert.ToBase64String(bytes);
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                var bytes = Convert.FromBase64String(cipherText);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                Debug.LogError("Decryption failed: invalid Base64 string.");
                throw new InvalidDataException("Failed to decrypt the data. The data might be corrupted.");
            }
        }

        public bool IsEncrypted(string data)
        {
            try
            {
                var decoded = Convert.FromBase64String(data);

                return decoded.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }

    public interface IProgressValidator
    {
        bool IsValid(UserProgress progress);
        bool IsValid(AudioSettings progress);

        public UserProgress ValidateAndFix(UserProgress progress);
        public AudioSettings ValidateAndFix(AudioSettings progress);
    }

    public class ProgressValidationService : IProgressValidator
    {
        public bool IsValid(UserProgress progress)
        {
            if (progress == null)
                return false;

            if (string.IsNullOrWhiteSpace(progress.UserName))
                return false;

            if (progress.Level <= 0)
                return false;

            return true;
        }

        public bool IsValid(AudioSettings progress)
        {
            if (progress.BackgroundMusicVolume < 0) //|| progress.BackgroundMusicVolume > 1)
                return false;

            if (progress.SfxVolume < 0) //|| progress.SfxVolume > 1)
                return false;

            return true;
        }

        public UserProgress ValidateAndFix(UserProgress progress)
        {
            if (progress == null)
            {
                Debug.LogWarning("Progress is null, initializing default progress.");
                return DefaultProgressFactory.CreateDefaultProgress();
            }

            if (progress.Level < 1)
            {
                Debug.LogWarning("Invalid level, resetting to 1.");
                progress.Level = 1;
            }

            if (progress.HardCurrency < 0)
                progress.HardCurrency = 0;

            if (progress.SoftCurrency < 0)
                progress.SoftCurrency = 0;

            return progress;
        }

        public AudioSettings ValidateAndFix(AudioSettings progress)
        {
            if (progress == null)
            {
                Debug.LogWarning("Progress is null, initializing default progress.");
                return DefaultProgressFactory.CreateDefaultAudioSettings();
            }

            // NOTE: Отрубил для тестов валидацию на > 1
            if (progress.BackgroundMusicVolume < 0) //|| progress.BackgroundMusicVolume > 1)
                progress.BackgroundMusicVolume = 1;

            if (progress.SfxVolume < 0) //|| progress.SfxVolume > 1)
                progress.SfxVolume = 1;

            return progress;
        }
    }

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
    
    public interface IFileFormatHandler
    {
        public string Serialize<T>(T data);
        public T Deserialize<T>(string serializedData);
        public object Deserialize(string serializedData, Type type);
        public string GetFileExtension();
    }

    public class JsonFileFormatHandler : IFileFormatHandler
    {
        public string Serialize<T>(T data) => JsonConvert.SerializeObject(data, Formatting.Indented);

        public T Deserialize<T>(string serializedData) => JsonConvert.DeserializeObject<T>(serializedData);

        public object Deserialize(string serializedData, Type type) =>
            JsonConvert.DeserializeObject(serializedData, type);


        public string GetFileExtension() => ".json";
    }

    public class BinaryFileFormatHandler : IFileFormatHandler
    {
        public string Serialize<T>(T data)
        {
            var json = JsonConvert.SerializeObject(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes); //TODO: Заглушка
        }

        public T Deserialize<T>(string serializedData)
        {
            var bytes = Convert.FromBase64String(serializedData); //TODO: Заглушка
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public object Deserialize(string serializedData, Type type)
        {
            var bytes = Convert.FromBase64String(serializedData); // TODO: Заглушка...
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type);
        }

        public string GetFileExtension() => ".rimuru";
    }

    public interface IProgressMigrationService
    {
        public bool TryMigrate(
            string directoryPath,
            string baseFileName,
            IFileFormatHandler targetFormat,
            IEnumerable<IFileFormatHandler> supportedFormats,
            Type modelType);
    }
    
    public class ProgressMigrationService : IProgressMigrationService
    {
        private readonly IDataStorage dataStorage;
        private readonly IEncryptionService encryptionService;

        public ProgressMigrationService(IDataStorage dataStorage, IEncryptionService encryptionService)
        {
            this.dataStorage = dataStorage;
            this.encryptionService = encryptionService;
        }

        public bool TryMigrate(
            string directoryPath,
            string baseFileName,
            IFileFormatHandler targetFormat,
            IEnumerable<IFileFormatHandler> supportedFormats,
            Type modelType)
        {
            foreach (var formatHandler in supportedFormats)
            {
                var legacyPath = Path.Combine(directoryPath, baseFileName + formatHandler.GetFileExtension());

                if (!dataStorage.Exists(legacyPath))
                    continue;

                try
                {
                    // === Загрузка данных из устаревшего формата.
                    var rawData = dataStorage.Load(legacyPath);
                    var decryptedData = encryptionService.IsEncrypted(rawData)
                        ? encryptionService.Decrypt(rawData)
                        : rawData;

                    // === Десериализация их.
                    var progress = formatHandler.Deserialize(decryptedData, modelType);

                    // === Сериализация в целевой формат.
                    var newFilePath = Path.Combine(directoryPath, baseFileName + targetFormat.GetFileExtension());
                    var newData = targetFormat.Serialize(progress);
                    var encryptedNewData = encryptionService.Encrypt(newData);

                    dataStorage.Save(newFilePath, encryptedNewData);

                    // === Удаление старого файла. !Editor тоже кстати не забыть бы отправить в вальхалу.
                    dataStorage.Delete(legacyPath);

                    Debug.Log($"Migration successful from {legacyPath} to {newFilePath}");
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Migration failed for {legacyPath}: {e.Message}");
                }
            }

            return false;
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

            dataStorage.Save(savePath, encryptedData);

#if UNITY_EDITOR
            var editorPath = Path.Combine(directoryPath,
                fileName + ".editor" + fileFormatConfig.CurrentFormatHandler.GetFileExtension());

            foreach (var handler in fileFormatConfig.SupportedFormatHandlers)
            {
                var legacyEditorPath = Path.Combine(directoryPath, fileName + ".editor" + handler.GetFileExtension());
                if (legacyEditorPath != editorPath && dataStorage.Exists(legacyEditorPath))
                {
                    dataStorage.Delete(legacyEditorPath);
                    Debug.Log($"Deleted legacy editor file: {legacyEditorPath}");
                }
            }

            var editorData = fileFormatConfig.CurrentFormatHandler.GetFileExtension() == ".rimuru"
                ? encryptionService.Decrypt(rawData)
                : rawData;
            dataStorage.Save(editorPath, editorData);
            Debug.Log($"Editor-readable file saved to: {editorPath}");
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
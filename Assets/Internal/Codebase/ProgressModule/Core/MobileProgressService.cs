using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Internal.Codebase.ProgressModule.Models.Gameplay;
using UnityEngine;

namespace Internal
{
    public class ProgressOperation
    {
        public bool IsDone { get; private set; }
        public float Progress { get; private set; }
        public string Status { get; private set; }

        public void Complete(string status = "Done")
        {
            IsDone = true;
            Progress = 1f;
            Status = status;
        }

        public void UpdateProgress(float progress, string status = "")
        {
            Progress = progress;
            Status = status;
        }
    }

    public static class Constants
    {
        public const string ROOT_FOLDER_NAME = "Database";

        // public const string USER_PROGRESS_FILE = "user_progress";
        public static string USER_PROGRESS_FILE = FileUtility.GetEncryptedFileName("user_progress");

        // public const string AUDIO_SETTINGS_FILE = "audio_settings";
        public static string AUDIO_SETTINGS_FILE = FileUtility.GetEncryptedFileName("audio_settings");


        // public const string WORLD_PROGRESS_FILE = "world_progress";
        public static string WORLD_PROGRESS_FILE = FileUtility.GetEncryptedFileName("world_progress");
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
            IProgressMigrationService migrationService,
            FakeKeyGenerator fakeKeyGenerator)
        {
            this.fileFormatConfig = fileFormatConfig;
            this.dataStorage = dataStorage;
            this.encryptionService = encryptionService;
            this.validator = validator;
            this.migrationService = migrationService;

            directoryPath = Path.Combine(Application.persistentDataPath, Constants.ROOT_FOLDER_NAME);

            if (IsFirstLaunch())
            {
                Directory.CreateDirectory(directoryPath);

                fakeKeyGenerator.StartDelayedFakeKeyGeneration();
            }

            var userProgressFile = Constants.USER_PROGRESS_FILE;
            var audioSettingsFile = Constants.AUDIO_SETTINGS_FILE;
            var worldProgressFile = Constants.WORLD_PROGRESS_FILE;

            // Маппинг ID на действия //
            idToLoadAction = new Dictionary<string, Action>
            {
                {
                    userProgressFile, () =>
                    {
                        if (UserProgress is { Origin: not null })
                        {
                            Debug.Log($"<color=yellow>User Progress Disposed.</color>: {UserProgress}>");
                            UserProgress?.Dispose();
                        }

                        UserProgress = new UserProgressProxy(LoadAllProgress(userProgressFile,
                            DefaultProgressFactory.CreateDefaultProgress));
                    }
                },
                {
                    audioSettingsFile, () =>
                    {
                        if (AudioSettings is { Origin: not null })
                        {
                            Debug.Log($"<color=yellow>Audio Settings Disposed.</color>: {AudioSettings}>");
                            AudioSettings?.Dispose();
                        }

                        AudioSettings = new AudioSettingsProxy(LoadAllProgress(audioSettingsFile,
                            DefaultProgressFactory.CreateDefaultAudioSettings));
                    }
                },
                {
                    worldProgressFile, () =>
                    {
                        if (WorldProgress is { Origin: not null })
                        {
                            Debug.Log($"<color=yellow>World Progress Disposed.</color>: {WorldProgress}>");
                            WorldProgress?.Dispose();
                        }

                        WorldProgress = new WorldProgressProxy(LoadAllProgress(worldProgressFile,
                            DefaultProgressFactory.CreateDefaultWorldProgress));
                    }
                }
            };

            // Маппинг ID на действия сохранения //
            idToSaveAction = new Dictionary<string, Action>
            {
                { userProgressFile, () => { SaveProgress(userProgressFile, UserProgress?.Origin); } },
                { audioSettingsFile, () => { SaveProgress(audioSettingsFile, AudioSettings?.Origin); } },
                { worldProgressFile, () => { SaveProgress(worldProgressFile, WorldProgress?.Origin); } }
            };

            // Маппинг ID на действия удаления //
            idToDeleteAction = new Dictionary<string, Action>
            {
                { userProgressFile, () => DeleteProgress(userProgressFile) },
                { audioSettingsFile, () => DeleteProgress(audioSettingsFile) },
                { worldProgressFile, () => DeleteProgress(worldProgressFile) }
            };
        }

        #region API

        public IUserProgressProxy UserProgress { get; private set; }
        public IAudioSettingsProxy AudioSettings { get; private set; }
        public IWorldProgressProxy WorldProgress { get; private set; }

        public void SaveAllProgress()
        {
            foreach (var action in idToSaveAction.Values)
            {
                Debug.Log($"[Save Progress] {action}]");
                action?.Invoke();
            }

            Debug.Log("All progress saved successfully.");
        }

        public void LoadAllProgress()
        {
            if (IsFirstLaunch())
            {
                Debug.Log("First launch detected. Initializing default progress.");

                UserProgress = new UserProgressProxy(DefaultProgressFactory.CreateDefaultProgress());
                AudioSettings = new AudioSettingsProxy(DefaultProgressFactory.CreateDefaultAudioSettings());
                WorldProgress = new WorldProgressProxy(DefaultProgressFactory.CreateDefaultWorldProgress());

                return;
            }

            foreach (var action in idToLoadAction.Values)
            {
                Debug.Log($"[Load Progress] {action}");
                action?.Invoke();
            }

            Debug.Log("All progress loaded successfully.");
        }

        public void DeleteAllProgress()
        {
            foreach (var action in idToDeleteAction.Values)
            {
                Debug.Log($"[Delete Progress] {action}]");
                action?.Invoke();
            }

            Debug.Log("All progress deleted successfully.");
        }

        public void SaveProgressById(string id)
        {
            if (idToSaveAction.TryGetValue(id, out var action))
            {
                action?.Invoke();
                Debug.Log($"[Successfully saved progress for ID: {id}]");
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
                Debug.Log($"[Successfully loaded progress for ID: {id}]");
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
                Debug.Log($"[Successfully deleted progress for ID: {id}]");
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
            WorldProgress?.Dispose();

            idToLoadAction?.Clear();
            idToSaveAction?.Clear();
            idToDeleteAction?.Clear();
        }

        #endregion

        #region Abstraction

        private void SaveProgress<T>(string fileName, T data)
        {
            if (data == null)
            {
                Debug.Log("You attempted to save a null object.");
                return;
            }

            var savePath = Path.Combine(directoryPath,
                fileName + fileFormatConfig.CurrentFormatHandler.GetFileExtension());
            var rawData = fileFormatConfig.CurrentFormatHandler.Serialize(data);
            var encryptedData = encryptionService.Encrypt(rawData);

            // Сохранение основного файла //
            // NOTE: Покрыть тестами на время выполнения.
            dataStorage.Save(savePath, encryptedData);

            Debug.Log($"Encrypt:Save | {fileName}: {encryptedData}");

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

        private TData LoadAllProgress<TData>(string fileName, Func<TData> createDefault) where TData : class
        {
            var fileFormat = fileFormatConfig.CurrentFormatHandler.GetFileExtension();
            var filePath = Path.Combine(directoryPath, fileName + fileFormat);

            // === Выполняем миграцию, если необходимо! *Позже так же нужно будет упростить это место. 
            if (!dataStorage.Exists(filePath))
            {
                Debug.Log($"File for '{fileName}' not found. Attempting migration.");

                var modelType = typeof(TData);

                var migrated = migrationService.TryMigrate(
                    directoryPath,
                    fileName,
                    fileFormatConfig.CurrentFormatHandler,
                    fileFormatConfig.SupportedFormatHandlers,
                    modelType);

                if (!migrated)
                {
                    Debug.LogWarning($"No valid save file found for '{fileName}' after migration. " +
                                     $"Initializing default data.");

                    return createDefault();
                }
            }

            var rawData = dataStorage.Load(filePath);
            try
            {
                var decryptedData = encryptionService.IsEncrypted(rawData)
                    ? encryptionService.Decrypt(rawData)
                    : rawData;

                Debug.Log($"Decrypt | {fileName}: {decryptedData}");

                // Validation Layer ===
                try
                {
                    var progress = fileFormatConfig.CurrentFormatHandler.Deserialize<TData>(decryptedData);

                    // === Валидация данных! | Я точно однажды забуду про это место, черкануть в доку нужно 100%
                    if (typeof(TData) == typeof(UserProgress) && !validator.IsValid((UserProgress)(object)progress))
                        throw new InvalidDataException("[Validation Layer] Invalid UserProgress data.");

                    if (typeof(TData) == typeof(AudioSettings) && !validator.IsValid((AudioSettings)(object)progress))
                        throw new InvalidDataException("[Validation Layer] Invalid AudioSettings data.");

                    if (typeof(TData) == typeof(WorldProgress) && !validator.IsValid((WorldProgress)(object)progress))
                        throw new InvalidDataException("[Validation Layer] Invalid WorldProgress data.");


                    return progress;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"[Catch Validation Layer] Failed to load progress: {e.Message}. Initializing default data.");
                    return createDefault();
                }
            }
            catch (InvalidDataException invalidDataException)
            {
                Debug.LogWarning(
                    $"[Decrypt -> [Pre Validation Layer]] -> Failed to load progress: {invalidDataException.Message}");
                return createDefault();
            }
        }

        private void DeleteProgress(string fileName)
        {
            Debug.Log($"<color=red>Deleting file '{fileName}'.</color>");

            var fileFormat = fileFormatConfig.CurrentFormatHandler.GetFileExtension();
            var filePath = Path.Combine(directoryPath, fileName + fileFormat);

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

        #region Internal

        /// <summary>
        /// Проверяет, является ли это первый вход.
        /// </summary>
        /// <returns>
        /// true - Это первый запуск игры
        /// false - Игрок как минимум 1 раз сохранялся.
        /// </returns>
        public bool IsFirstLaunch()
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.Log($"<color=yellow>[FirstLaunch] Directory doesn't exist. Creating it.]</color>");
                return true;
            }

            //
            // Вообще по одному файлу достаточно, так как если нет основного, можно все офнуть, ибо нефиг лезть в файлы :3
            //
            // var requiredFiles = new[]
            // {
            //     Path.Combine(directoryPath, Constants.USER_PROGRESS_FILE + fileFormatConfig.CurrentFormatHandler.GetFileExtension()),
            //     Path.Combine(directoryPath, Constants.AUDIO_SETTINGS_FILE + fileFormatConfig.CurrentFormatHandler.GetFileExtension()),
            // };
            //
            // // Проверяем, все ли необходимые файлы существуют //
            // foreach (var file in requiredFiles)
            // {
            //     Debug.Log($"file: {file}");
            //     
            //     if (!File.Exists(file))
            //     {
            //         Debug.Log($"<color=yellow>[FirstLaunch] Missing required file: {file}.</color>");
            //         return true;
            //     }
            // }

            Debug.Log("<color=yellow>[NO FirstLaunch] All required files found, not the first launch.</color>");
            return false;
        }

        #endregion

        #region Coroutines

        public IEnumerator LoadAllProgressCoroutine(ProgressOperation operation)
        {
            operation.UpdateProgress(0f, status: "Initializing...");

            if (IsFirstLaunch())
            {
                Debug.Log("[First launch detected. Initializing default progress.]");

                UserProgress = new UserProgressProxy(DefaultProgressFactory.CreateDefaultProgress());
                AudioSettings = new AudioSettingsProxy(DefaultProgressFactory.CreateDefaultAudioSettings());
                WorldProgress = new WorldProgressProxy(DefaultProgressFactory.CreateDefaultWorldProgress());

                operation.Complete("[Default data initialized.]");

                yield break;
            }

            var ids = idToLoadAction.Keys.ToList();
            var totalSteps = ids.Count;

            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];

                if (idToLoadAction.TryGetValue(id, out var action))
                {
                    action?.Invoke();

                    var targetProgress = (float)(i + 1) / totalSteps;

                    while (operation.Progress < targetProgress)
                    {
                        operation.UpdateProgress(Mathf.MoveTowards(
                                operation.Progress,
                                targetProgress,
                                Time.deltaTime * 0.5f),
                            $"Loading {id}");

                        yield return null;
                    }

                    yield return null;
                }
            }

            operation.Complete("All progress loaded successfully.");
        }

        public IEnumerator SaveAllProgressCoroutine(ProgressOperation operation)
        {
            operation.UpdateProgress(0f, "Initializing...");

            var ids = idToSaveAction.Keys.ToList();
            var totalSteps = ids.Count;

            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];

                if (idToSaveAction.TryGetValue(id, out var action))
                {
                    action?.Invoke();

                    var targetProgress = (float)(i + 1) / totalSteps;

                    while (operation.Progress < targetProgress)
                    {
                        operation.UpdateProgress(Mathf.MoveTowards(
                                operation.Progress,
                                targetProgress,
                                Time.deltaTime * 0.5f),
                            $"Saving {id}");

                        yield return null;
                    }

                    yield return null;
                }
            }

            operation.Complete("All progress saved successfully.");
        }

        public IEnumerator DeleteAllProgressCoroutine(ProgressOperation operation)
        {
            operation.UpdateProgress(0f, "Initializing...");

            var ids = idToDeleteAction.Keys.ToList();
            var totalSteps = ids.Count;

            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];

                if (idToDeleteAction.TryGetValue(id, out var action))
                {
                    action?.Invoke();

                    var targetProgress = (float)(i + 1) / totalSteps;

                    while (operation.Progress < targetProgress)
                    {
                        operation.UpdateProgress(Mathf.MoveTowards(
                                operation.Progress,
                                targetProgress,
                                Time.deltaTime * 0.5f),
                            $"Deleting {id}");

                        yield return null;
                    }

                    yield return null;
                }
            }

            operation.Complete("All progress deleted successfully.");
        }

        public IEnumerator SaveProgressByIdCoroutine(string id, ProgressOperation operation)
        {
            operation.UpdateProgress(0f, $"Initializing save for ID: {id}");

            if (idToSaveAction.TryGetValue(id, out var action))
            {
                action?.Invoke();

                // NOTE: Для одного ID прогресс всегда равен 100%
                // Пока не буду в константы выносить, пусть будет так пока что для простоты.
                var targetProgress = 1f;

                while (operation.Progress < targetProgress)
                {
                    operation.UpdateProgress(Mathf.MoveTowards(
                            operation.Progress,
                            targetProgress,
                            Time.deltaTime * 0.5f),
                        $"Saving {id}");

                    yield return null;
                }

                operation.Complete($"Save for ID {id} completed successfully.");
            }
            else
            {
                operation.Complete($"Unknown ID: {id}. Save skipped.");
                Debug.LogWarning($"Unknown progress ID: {id}");
            }
        }

        public IEnumerator LoadProgressByIdCoroutine(string id, ProgressOperation operation)
        {
            operation.UpdateProgress(0f, $"Initializing load for ID: {id}");

            if (idToLoadAction.TryGetValue(id, out var action))
            {
                action?.Invoke();

                // NOTE: Для одного ID прогресс всегда равен 100%
                // Так же я продублировал в сейве и удалении файла, при рефакторе надо бы учесть это.
                var targetProgress = 1f;

                while (operation.Progress < targetProgress)
                {
                    operation.UpdateProgress(Mathf.MoveTowards(
                            operation.Progress,
                            targetProgress,
                            Time.deltaTime * 0.5f),
                        $"Loading {id}");

                    yield return null;
                }

                operation.Complete($"Load for ID {id} completed successfully.");
            }
            else
            {
                operation.Complete($"Unknown ID: {id}. Load skipped.");
                Debug.LogWarning($"Unknown progress ID: {id}");
            }
        }

        public IEnumerator DeleteProgressByIdCoroutine(string id, ProgressOperation operation)
        {
            operation.UpdateProgress(0f, $"Initializing delete for ID: {id}");

            if (idToDeleteAction.TryGetValue(id, out var action))
            {
                action?.Invoke();

                var targetProgress = 1f;

                while (operation.Progress < targetProgress)
                {
                    operation.UpdateProgress(Mathf.MoveTowards(
                            operation.Progress,
                            targetProgress,
                            Time.deltaTime * 0.5f),
                        $"Deleting {id}"); // TODO: Передавать enum что бы можно было локализировать.

                    yield return null;
                }

                operation.Complete($"Delete for ID {id} completed successfully.");
            }
            else
            {
                operation.Complete($"Unknown ID: {id}. Delete skipped.");
                Debug.LogWarning($"Unknown progress ID: {id}");
            }
        }

        #endregion
    }
}
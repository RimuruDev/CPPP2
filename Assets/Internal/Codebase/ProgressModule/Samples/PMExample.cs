using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = System.Random;

namespace Internal
{
    // NOTE: Онли для теста. Снеси потом бро
    public enum FileFormatType
    {
        Json = 0,
        Binary = 1
    }

    public class PMExample : MonoBehaviour
    {
        public FileFormatType fileFormat;
        private MobileProgressService progressService;

        private void Awake()
        {
            var dataStorage = new FileDataStorage();
            var encryptionService = new SimpleEncryptionService();
            var validator = new ProgressValidationService();
            var jsonHandler = new JsonFileFormatHandler();
            var binaryHandler = new BinaryFileFormatHandler();
            FileFormatConfiguration fileFormatConfig;

            if (fileFormat == FileFormatType.Json)
            {
                fileFormatConfig = new FileFormatConfiguration(
                    currentHandler: jsonHandler,
                    supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler }
                );
            }
            else if (fileFormat == FileFormatType.Binary)
            {
                fileFormatConfig = new FileFormatConfiguration(
                    currentHandler: binaryHandler,
                    supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler }
                );
            }
            else // === Пусть будет Fallback на случай если редактор шалить будет.
            {
                fileFormatConfig = new FileFormatConfiguration(
                    currentHandler: jsonHandler,
                    supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler }
                );
            }

            var migrationService = new ProgressMigrationService(dataStorage, encryptionService);

            progressService = new MobileProgressService(
                fileFormatConfig,
                dataStorage,
                encryptionService,
                validator,
                migrationService
            );

            // TODO: Обновить надо так что бы он сразу все дергал, но пока просто 2 вызова кули
            //progressService.LoadProgress();
            progressService.LoadProgressById("audio_settings");
            progressService.LoadProgressById("user_progress");
            // progressService.SaveAllProgress();
            progressService.SaveProgressById("user_progress");
            progressService.SaveProgressById("audio_settings");
        }


        [ContextMenu(nameof(TestSave))]
        public void TestSave()
        {
            progressService.UserProgress.SoftCurrency.Value += 100;
            progressService.AudioSettings.BackgroundMusicVolume.Value -= 0.22f;
            progressService.AudioSettings.SfxVolume.Value += GetRandom01();

            //progressService.SaveAllProgress();
            progressService.SaveProgressById("user_progress");
            progressService.SaveProgressById("audio_settings");
        }

        [ContextMenu("_" + nameof(ChangeUserProgress))]
        private void ChangeUserProgress()
        {
            progressService.UserProgress.SoftCurrency.Value += 300;
            progressService.UserProgress.HardCurrency.Value += 150;
            progressService.UserProgress.UserName.Value = "Mewow" + UnityEngine.Random.Range(1, 50);
            progressService.UserProgress.Level.Value = UnityEngine.Random.Range(1, 50);
        }

        [ContextMenu("_" + nameof(ChangeAudioProgress))]
        private void ChangeAudioProgress()
        {
            progressService.AudioSettings.BackgroundMusicVolume.Value -= 0.132f;
            progressService.AudioSettings.SfxVolume.Value -= 0.123f;
        }

        [ContextMenu(nameof(SaveUserProgress))]
        public void SaveUserProgress()
        {
            progressService.UserProgress.SoftCurrency.Value += 50;
            progressService.SaveProgressById("user_progress");
        }

        [ContextMenu(nameof(SaveAudioSettings))]
        public void SaveAudioSettings()
        {
            progressService.AudioSettings.BackgroundMusicVolume.Value += GetRandom01();
            progressService.SaveProgressById("audio_settings");
        }

        [ContextMenu(nameof(LoadUserProgress))]
        public void LoadUserProgress()
        {
            progressService.LoadProgressById("user_progress");
        }

        [ContextMenu(nameof(LoadAudioSettings))]
        public void LoadAudioSettings()
        {
            progressService.LoadProgressById("audio_settings");
        }

        private static float GetRandom01() =>
            UnityEngine.Random.Range(0, 1f) < 0.5f
                ? -0.1f
                : 1.0f;
    }
}
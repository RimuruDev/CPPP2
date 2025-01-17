using UnityEngine;
using System.Collections.Generic;
using Internal.Codebase.ProgressModule.Models.Gameplay;

namespace Internal
{
    public class PMExample : MonoBehaviour
    {
        [SerializeField] private FileFormatType fileFormat;
        private MobileProgressService progressService;

        private void Awake()
        {
            var dataStorage = new FileDataStorage();
            var encryptionService = new SimpleEncryptionService();
            var validator = new ProgressValidationService();
            var jsonHandler = new JsonFileFormatHandler();
            var binaryHandler = new BinaryFileFormatHandler();

            var fileFormatConfig = fileFormat switch
            {
                FileFormatType.Json => new FileFormatConfiguration(currentHandler: jsonHandler, supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler }),
                FileFormatType.Binary => new FileFormatConfiguration(currentHandler: binaryHandler, supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler }),
                _ => new FileFormatConfiguration(currentHandler: jsonHandler, supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler })
            };

            var migrationService = new ProgressMigrationService(dataStorage, encryptionService);

            progressService = new MobileProgressService(
                fileFormatConfig,
                dataStorage,
                encryptionService,
                validator,
                migrationService
            );

            // progressService.LoadProgressById(Constants.AUDIO_SETTINGS_FILE);
            // progressService.LoadProgressById(Constants.USER_PROGRESS_FILE);
            //
            // progressService.SaveProgressById(Constants.USER_PROGRESS_FILE);
            // progressService.SaveProgressById(Constants.AUDIO_SETTINGS_FILE);
            
            progressService.LoadAllProgress();
            // progressService.SaveAllProgress();

            UserProgress = progressService.UserProgress.Origin;
            AudioSettings= progressService.AudioSettings.Origin;
            WorldProgress= progressService.WorldProgress.Origin;
        }

        public UserProgress  UserProgress;
        public AudioSettings AudioSettings;
        public WorldProgress WorldProgress;

        [ContextMenu(nameof(TestSave))]
        public void TestSave()
        {
            progressService.UserProgress.SoftCurrency.Value += 100;
            progressService.AudioSettings.BackgroundMusicVolume.Value -= 0.22f;
            progressService.AudioSettings.SfxVolume.Value += GetRandom01();
            progressService.WorldProgress.CurrentWorldPosition.Value = new Vector3Data(Random.value, Random.value, Random.value);
            progressService.WorldProgress.CurrentWorldRotation.Value = new Vector3Data(Random.value, Random.value, Random.value);
            progressService.WorldProgress.CurrentTime.Value = Random.value;

            progressService.SaveProgressById(Constants.USER_PROGRESS_FILE);
            progressService.SaveProgressById(Constants.AUDIO_SETTINGS_FILE);
            progressService.SaveProgressById(Constants.WORLD_PROGRESS_FILE);
        }

        [ContextMenu("_" + nameof(ChangeUserProgress))]
        private void ChangeUserProgress()
        {
            progressService.UserProgress.SoftCurrency.Value += 300;
            progressService.UserProgress.HardCurrency.Value += 150;
            progressService.UserProgress.UserName.Value = "Mewow" + Random.Range(1, 50);
            progressService.UserProgress.Level.Value = Random.Range(1, 50);
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
            progressService.SaveProgressById(Constants.USER_PROGRESS_FILE);
        }

        [ContextMenu(nameof(SaveAudioSettings))]
        public void SaveAudioSettings()
        {
            progressService.AudioSettings.BackgroundMusicVolume.Value += GetRandom01();
            progressService.SaveProgressById(Constants.AUDIO_SETTINGS_FILE);
        }

        [ContextMenu(nameof(LoadUserProgress))]
        public void LoadUserProgress() => 
            progressService.LoadProgressById(Constants.USER_PROGRESS_FILE);

        [ContextMenu(nameof(LoadAudioSettings))]
        public void LoadAudioSettings() => 
            progressService.LoadProgressById(Constants.AUDIO_SETTINGS_FILE);

        private static float GetRandom01() =>
            Random.Range(0, 1f) < 0.5f
                ? -0.1f
                : 1.0f;

        private enum FileFormatType
        {
            Json = 0,
            Binary = 1
        }
    }
}
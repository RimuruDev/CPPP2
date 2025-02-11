using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Internal.Codebase.ProgressModule.Models.Gameplay;
using TMPro;
using UnityEngine.UI;

namespace Internal
{
    public class PMExample : MonoBehaviour
    {
        // Debug only
        // [SerializeField] private UserProgress userProgress;
        // [SerializeField] private AudioSettings audioSettings;
        // [SerializeField] private WorldProgress worldProgress;
        [SerializeField] private FileFormatType fileFormat;

        // == load
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressText;

        // == save
        [SerializeField] private Slider saveProgressBar;
        [SerializeField] private TMP_Text saveProgressText;

        // == delete
        [SerializeField] private Slider deleteProgressBar; 
        [SerializeField] private TMP_Text deleteProgressText; 

        // == btns
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;

        [SerializeField]
        private FakeKeyGenerator fakeKeyGenerator;
        
        private IProgressService progressService;
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
                migrationService,
                fakeKeyGenerator
            );

            // progressService.LoadProgressById(Constants.AUDIO_SETTINGS_FILE);
            // progressService.LoadProgressById(Constants.USER_PROGRESS_FILE);
            //
            // progressService.SaveProgressById(Constants.USER_PROGRESS_FILE);
            // progressService.SaveProgressById(Constants.AUDIO_SETTINGS_FILE);

            // progressService.SaveAllProgress();

            // progressService.LoadAllProgress();
            // UserProgress = progressService.UserProgress.Origin;
            // AudioSettings= progressService.AudioSettings.Origin;
            // WorldProgress= progressService.WorldProgress.Origin;

            saveButton.onClick.AddListener(TestSaveWithUI);
            loadButton.onClick.AddListener(TestLoadWithUI);
            deleteButton.onClick.AddListener(TestDeleteWithUI);
        }

        // private IEnumerator Start()
        // {
        //     var loadOperation = new ProgressOperation();
        //     yield return StartCoroutine(progressService.LoadAllProgressCoroutine(loadOperation));
        //     
        //     UserProgress = progressService.UserProgress.Origin;
        //     AudioSettings= progressService.AudioSettings.Origin;
        //     WorldProgress= progressService.WorldProgress.Origin;
        //     
        //     Debug.Log($"Loading complete: {loadOperation.Status}");
        //     Debug.Log($"Progress: {loadOperation.Progress}");
        // }
       
        [ContextMenu("____" + nameof(TestLoadWithUI))]
        private void TestLoadWithUI()
        {
            var loadOperation = new ProgressOperation();
            StartCoroutine(UpdateProgressUI(loadOperation));
            StartCoroutine(progressService.LoadAllProgressCoroutine(loadOperation));

            // Debug.Log($"Loading complete: {loadOperation.Status}");
            // Debug.Log($"Progress: {loadOperation.Progress}");
        }
        
        [ContextMenu("____" + nameof(TestSaveWithUI))]
        public void TestSaveWithUI()
        {
            var saveOperation = new ProgressOperation();

            StartCoroutine(UpdateSaveProgressUI(saveOperation));
            StartCoroutine(progressService.SaveAllProgressCoroutine(saveOperation));
        }
        
        [ContextMenu("____" +nameof(TestDeleteWithUI))]
        public void TestDeleteWithUI()
        {
            var deleteOperation = new ProgressOperation();

            StartCoroutine(UpdateDeleteProgressUI(deleteOperation));
            StartCoroutine(progressService.DeleteAllProgressCoroutine(deleteOperation));
        }


        private IEnumerator UpdateProgressUI(ProgressOperation operation)
        {
            while (!operation.IsDone)
            {
                progressBar.value = operation.Progress;
                progressText.text = $"Status: {operation.Status}\nProgress: {operation.Progress * 100:F1}%";
                yield return null;
            }

            progressBar.value = 1f;
            progressText.text = $"Status: {operation.Status}\nProgress: 100%";
        }

        private IEnumerator UpdateSaveProgressUI(ProgressOperation operation)
        {
            while (!operation.IsDone)
            {
                saveProgressBar.value = operation.Progress;
                saveProgressText.text = $"Status: {operation.Status}\nProgress: {operation.Progress * 100:F1}%";
                yield return null;
            }

            saveProgressBar.value = 1f;
            saveProgressText.text = $"Status: {operation.Status}\nProgress: 100%";
        }
        
        private IEnumerator UpdateDeleteProgressUI(ProgressOperation operation)
        {
            while (!operation.IsDone)
            {
                deleteProgressBar.value = operation.Progress;
                deleteProgressText.text = $"Status: {operation.Status}\nProgress: {operation.Progress * 100:F1}%";
                yield return null;
            }

            deleteProgressBar.value = 1f;
            deleteProgressText.text = $"Status: {operation.Status}\nProgress: 100%";
        }

        [ContextMenu(nameof(TestSaveByIdWithUI))]
        public void TestSaveByIdWithUI()
        {
            var saveOperation = new ProgressOperation();

            StartCoroutine(UpdateSaveProgressUI(saveOperation)); 
            StartCoroutine(progressService.SaveProgressByIdCoroutine(Constants.USER_PROGRESS_FILE, saveOperation));
        }

        [ContextMenu(nameof(TestLoadByIdWithUI))]
        public void TestLoadByIdWithUI()
        {
            var loadOperation = new ProgressOperation();

            StartCoroutine(UpdateProgressUI(loadOperation));
            StartCoroutine(progressService.LoadProgressByIdCoroutine(Constants.USER_PROGRESS_FILE, loadOperation));
        }

        [ContextMenu(nameof(TestDeleteByIdWithUI))]
        public void TestDeleteByIdWithUI()
        {
            var deleteOperation = new ProgressOperation();

            StartCoroutine(UpdateDeleteProgressUI(deleteOperation));
            StartCoroutine(progressService.DeleteProgressByIdCoroutine(Constants.USER_PROGRESS_FILE, deleteOperation));
        }


        [ContextMenu(nameof(TestSave))]
        public void TestSave()
        {
            progressService.UserProgress.SoftCurrency.Value += 100;
            progressService.AudioSettings.BackgroundMusicVolume.Value -= 0.22f;
            progressService.AudioSettings.SfxVolume.Value += GetRandom01();
            progressService.WorldProgress.CurrentWorldPosition.Value =
                new Vector3Data(Random.value, Random.value, Random.value);
            progressService.WorldProgress.CurrentWorldRotation.Value =
                new Vector3Data(Random.value, Random.value, Random.value);
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
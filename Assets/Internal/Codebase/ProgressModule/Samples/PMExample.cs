using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
            FileFormatConfiguration fileFormatConfig ;
          
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
            progressService.LoadProgress();
            progressService.SaveAllProgress();
        }


        [ContextMenu(nameof(TestSave))]
        public void TestSave()
        {
            progressService.UserProgress.SoftCurrency.Value += 100;
            progressService.AudioSettings.BackgroundMusicVolume.Value += 0.1f;
            progressService.AudioSettings.SfxVolume.Value += 0.1f;
            
            progressService.SaveAllProgress();
        }

    }
}
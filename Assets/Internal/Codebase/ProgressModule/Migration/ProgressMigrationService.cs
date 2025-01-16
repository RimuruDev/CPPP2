using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Internal
{
    public class ProgressMigrationService : IProgressMigrationService
    {
        private readonly IDataStorage dataStorage;
        private readonly IEncryptionService encryptionService;

        public ProgressMigrationService(IDataStorage dataStorage, IEncryptionService encryptionService)
        {
            this.dataStorage = dataStorage;
            this.encryptionService = encryptionService;
        }

        // NOTE: Разбить на мелкие методы.
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
                    var rawData = dataStorage.Load(legacyPath);
                    var decryptedData = encryptionService.IsEncrypted(rawData)
                        ? encryptionService.Decrypt(rawData)
                        : rawData;

                    var progress = formatHandler.Deserialize(decryptedData, modelType);

                    var newFilePath = Path.Combine(directoryPath, baseFileName + targetFormat.GetFileExtension());
                    var newData = targetFormat.Serialize(progress);
                    var encryptedNewData = encryptionService.Encrypt(newData);

                    dataStorage.Save(newFilePath, encryptedNewData);

#if UNITY_EDITOR
                    var editorFilePath = Path.Combine(directoryPath, baseFileName + ".editor" + targetFormat.GetFileExtension());
                    dataStorage.Save(editorFilePath, newData);
                    Debug.Log($"Editor-readable file saved during migration: {editorFilePath}");
#endif

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
}
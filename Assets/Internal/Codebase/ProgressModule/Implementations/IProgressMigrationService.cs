using System;
using System.Collections.Generic;

namespace Internal
{
    public interface IProgressMigrationService
    {
        public bool TryMigrate(
            string directoryPath,
            string baseFileName,
            IFileFormatHandler targetFormat,
            IEnumerable<IFileFormatHandler> supportedFormats,
            Type modelType);
    }
}
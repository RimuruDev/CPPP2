using System.Collections.Generic;

namespace Internal
{
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
}
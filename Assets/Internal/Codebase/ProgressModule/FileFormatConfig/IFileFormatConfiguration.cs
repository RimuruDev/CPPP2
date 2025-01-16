using System.Collections.Generic;

namespace Internal
{
    public interface IFileFormatConfiguration
    {
        public IFileFormatHandler CurrentFormatHandler { get; }
        public List<IFileFormatHandler> SupportedFormatHandlers { get; }
    }
}
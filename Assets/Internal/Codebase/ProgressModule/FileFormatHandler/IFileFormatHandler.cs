using System;

namespace Internal
{
    public interface IFileFormatHandler
    {
        public string Serialize<TData>(TData data);
        public TData Deserialize<TData>(string serializedData);
        public object Deserialize(string serializedData, Type type);
        public string GetFileExtension();
    }
}
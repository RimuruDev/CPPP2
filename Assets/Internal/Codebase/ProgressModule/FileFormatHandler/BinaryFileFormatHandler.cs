using System;
using Newtonsoft.Json;

namespace Internal
{
    public sealed class BinaryFileFormatHandler : IFileFormatHandler
    {
        private const string CUSTOM_BINARY_FORMAT = ".rimuru";

        public string Serialize<TDate>(TDate data) =>
            JsonConvert.SerializeObject(data);

        public TDate Deserialize<TDate>(string serializedData) =>
            JsonConvert.DeserializeObject<TDate>(serializedData);

        public object Deserialize(string serializedData, Type type) =>
            JsonConvert.DeserializeObject(serializedData, type);

        public string GetFileExtension() =>
            CUSTOM_BINARY_FORMAT;
    }
}
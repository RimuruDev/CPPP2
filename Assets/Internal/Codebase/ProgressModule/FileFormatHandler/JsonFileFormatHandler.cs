using System;
using Newtonsoft.Json;

namespace Internal
{
    public sealed class JsonFileFormatHandler : IFileFormatHandler
    {
        private const string JSON_FORMAT = "json";

        public string Serialize<TData>(TData data) =>
            JsonConvert.SerializeObject(data, Formatting.Indented);

        public TData Deserialize<TData>(string serializedData) =>
            JsonConvert.DeserializeObject<TData>(serializedData);

        public object Deserialize(string serializedData, Type type) =>
            JsonConvert.DeserializeObject(serializedData, type);

        public string GetFileExtension() =>
            JSON_FORMAT;
    }
}
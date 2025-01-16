// ********************************************************************
//
//   Copyright (c) RimuruDev
//   Contact information:
//       Email:    rimuru.dev@gmail.com
//       GitHub:   https://github.com/RimuruDev
//       LinkedIn: https://www.linkedin.com/in/rimuru/
//
// ********************************************************************

// ReSharper disable CheckNamespace

using System;
using RimuruDev;
using UnityEngine;
using Newtonsoft.Json;

namespace AbyssMoth
{
    /// <summary>
    /// Dependency:
    /// <code>
    ///{
    ///  "dependencies": {
    ///    "com.unity.nuget.newtonsoft-json": "3.2.1",
    /// }
    ///}
    /// </code>
    /// <example>
    /// <code>
    /// [Serializable]
    /// public class HalloweenUserProgress
    /// {
    ///     [JsonProperty("Currency")] 
    ///     [JsonConverter(typeof(ReactivePropertyConverter))]
    ///     public ReactiveProperty-int> Currency = new();
    ///
    ///     [JsonProperty("LevelProgress")] 
    ///     [JsonConverter(typeof(ReactivePropertyConverter))]
    ///     public ReactiveProperty-List-LevelProgress>> LevelProgress = new();
    /// }
    /// </code>
    /// </example>
    /// </summary>
    [HelpURL("https://github.com/RimuruDev/Unity-ReactivePropertyConverter.git")]
    public sealed class ReactivePropertyConverter : JsonConverter
    {
        private const string ReactivePropertyName = "Value";
        private const int FirstGenericArgument = 0;

        public override bool CanConvert(Type objectType) =>
            objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ReactiveProperty<>);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var valueProperty = value.GetType().GetProperty(ReactivePropertyName);

                if (valueProperty != null)
                {
                    var underlyingValue = valueProperty.GetValue(value);

                    serializer.Serialize(writer, underlyingValue);
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var valueType = objectType.GetGenericArguments()[FirstGenericArgument];

            var value = serializer.Deserialize(reader, valueType);

            var reactivePropertyType = typeof(ReactiveProperty<>).MakeGenericType(valueType);
            var reactiveProperty = Activator.CreateInstance(reactivePropertyType, value);

            return reactiveProperty;
        }
    }
}
using EFT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreBotsAPI
{
    internal class WildSpawnTypeFromIntConverter<T> : JsonConverter where T : struct, Enum
    {
        public bool isCaseSensitive;
        public Dictionary<int, T> intToEnumMap;
        public Dictionary<T, int> enumToIntMap;

        public GClass1866<WildSpawnType> oldConverter;

        public WildSpawnTypeFromIntConverter() : this(true)
        {
        }

        public WildSpawnTypeFromIntConverter(bool caseSensitive)
        {
            oldConverter = new GClass1866<WildSpawnType>(false);

            isCaseSensitive = caseSensitive;
            int count = GClass866<T>.Count;
            intToEnumMap = new Dictionary<int, T>(count);
            enumToIntMap = new Dictionary<T, int>(count);
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                int intValue = Convert.ToInt32(item);
                intToEnumMap[intValue] = item;
                enumToIntMap[item] = intValue;
            }
        }

        public override bool CanWrite => true;

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return oldConverter.CanConvert(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string text = serializer.Deserialize<string>(reader);
            int number = -1;

            if (int.TryParse(text, out number))
            {
                if (intToEnumMap.TryGetValue(number, out T value))
                {
                    return value;
                }
            }

            return oldConverter.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            oldConverter.WriteJson(writer, value, serializer);
        }
    }
}

﻿using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Yolol.Execution;
using Type = System.Type;

namespace BlazorYololEmulator.Client
{
    public class SerializedState
    {
        private static readonly JsonSerializerSettings JsonConfig = new()
        {
            Converters = new JsonConverter[] {
                new YololValueConverter()
            },
            FloatFormatHandling = FloatFormatHandling.DefaultValue,
            FloatParseHandling = FloatParseHandling.Decimal
        };

        public string Code = "";
        public Dictionary<string, Value> Values = new();
        public int ProgramCounter = 0;

        public override string ToString()
        {
            return $"PC:{ProgramCounter} Vars:{Values.Count} Code:{Code.Length}";
        }

        public static SerializedState FromBase64(string base64)
        {
            var bytes = Decompress(Convert.FromBase64String(base64));
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<SerializedState>(json, JsonConfig) ?? new SerializedState();
        }

        public string ToBase64()
        {
            var json = JsonConvert.SerializeObject(this, JsonConfig);
            var bytes = Compress(Encoding.UTF8.GetBytes(json));
            return Convert.ToBase64String(bytes);
        }

        public static byte[] Compress(byte[] data)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            var input = new MemoryStream(data);
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        private class YololValueConverter
            : JsonConverter<Value>
        {
            public override void WriteJson(JsonWriter writer, Value value, JsonSerializer serializer)
            {
                Console.WriteLine(value);
                if (value.Type == Yolol.Execution.Type.String)
                    writer.WriteValue(value.String.ToString());
                else
                    writer.WriteValue((decimal)value.Number);
            }

            public override Value ReadJson(JsonReader reader, Type objectType, Value existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return reader.TokenType switch
                {
                    JsonToken.String => new Value((string)reader.Value!),
                    JsonToken.Integer => new Value((Number)(int)(long)reader.Value!),
                    JsonToken.Float => new Value((Number)(decimal)reader.Value!),
                    _ => throw new InvalidOperationException($"Unexpected token type `{reader.TokenType}` for Yolol value")
                };
            }

            public override bool CanRead => true;

            public override bool CanWrite => true;
        }
    }
}

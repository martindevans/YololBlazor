using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Yolol.Execution;

namespace BlazorYololEmulator.Shared;

public class SerializedState
{
    public static SerializedState Default { get; } = new SerializedState("", new Dictionary<string, Value>(), 0);

    public string Code { get; }
    public IReadOnlyDictionary<string, Value> Values { get; }
    public int ProgramCounter { get; }

    public SerializedState(string code, IReadOnlyDictionary<string, Value> values, int programCounter)
    {
        Code = code;
        Values = values.ToDictionary(a => a.Key, a => a.Value);
        ProgramCounter = programCounter;
    }

    public override string ToString()
    {
        return $"PC:{ProgramCounter} NumVars:{Values.Count} CodeLength:{Code.Length}";
    }

    #region serialization
    public static SerializedState Deserialize(string base64)
    {
        if (base64 == "")
            return Default;

        var bytes = Decompress(Convert.FromBase64String(base64));
        var json = Encoding.UTF8.GetString(bytes);
        return JsonConvert.DeserializeObject<SerializedState>(json, JsonConfig) ?? Default;
    }

    public string Serialize()
    {
        if (string.IsNullOrWhiteSpace(Code) && Values.Count == 0 && ProgramCounter == 0)
            return "";

        var json = JsonConvert.SerializeObject(this, JsonConfig);
        var bytes = Compress(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(bytes);
    }

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var dstream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
        {
            dstream.CopyTo(output);
        }
        return output.ToArray();
    }

    private static readonly JsonSerializerSettings JsonConfig = new()
    {
        Converters = new JsonConverter[] {
            new YololValueConverter()
        },
        FloatFormatHandling = FloatFormatHandling.DefaultValue,
        FloatParseHandling = FloatParseHandling.Decimal,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
    };

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

        public override Value ReadJson(JsonReader reader, System.Type objectType, Value existingValue, bool hasExistingValue, JsonSerializer serializer)
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
    #endregion
}
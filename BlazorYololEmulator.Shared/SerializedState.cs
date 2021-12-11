using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Newtonsoft.Json;
using Yolol.Execution;

namespace BlazorYololEmulator.Shared;

public class SerializedState
{
    public static SerializedState Default { get; } = new SerializedState("", new Dictionary<string, Value>(), 0);

    public string Code { get; }
    public IReadOnlyDictionary<string, Value> Values { get; }

    /// <summary>
    /// Zero based program counter
    /// </summary>
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
    public static SerializedState Deserialize(string urlEncoded)
    {
        if (urlEncoded == "")
            return Default;

        var base64 = HttpUtility.UrlDecode(urlEncoded);
        var compressed = Convert.FromBase64String(base64);
        var bytes = Decompress(compressed);
        var json = Encoding.UTF8.GetString(bytes);

        return JsonConvert.DeserializeObject<SerializedState>(json, JsonConfig) ?? Default;
    }

    public string Serialize()
    {
        if (string.IsNullOrWhiteSpace(Code) && Values.Count == 0 && ProgramCounter == 0)
            return "";

        var json = JsonConvert.SerializeObject(this, JsonConfig);
        var bytes = Encoding.UTF8.GetBytes(json);
        var compressed = Compress(bytes);
        var base64 = Convert.ToBase64String(compressed);
        var urlEncoded = HttpUtility.UrlEncode(base64);

        return urlEncoded;
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
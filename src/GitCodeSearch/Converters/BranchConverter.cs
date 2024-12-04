using GitCodeSearch.Model;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitCodeSearch.Converters;

public class BranchConverter : JsonConverter<Branch>
{
    public override Branch Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var branchName = reader.GetString();
            return string.IsNullOrEmpty(branchName) ? Branch.Empty : new Branch(branchName);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Branch value, JsonSerializerOptions options)
    {
        if (value == Branch.Empty)
        {
            writer.WriteStringValue(string.Empty);
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}

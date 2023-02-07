using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunit;

namespace Tests;

public record User(string? Name, string? Surname)
{
    [JsonIgnore]
    public string? FullName { get => $"{Name} {Surname}"; }
}

public class Test
{
    [Fact]
    public void SerializesUser()
    {
        User value = new User(Name: "John", Surname: "Doe");
        string json = JsonSerializer.Serialize(value);

        Assert.Equal("""{"Name":"John","Surname":"Doe"}""", json);
    }

    [Fact]
    public void SerializeWithOptions()
    {
        User value = new User(Name: "John", Surname: "Doe");

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        string json = JsonSerializer.Serialize(value, options);

        Assert.Equal("""{"name":"John","surname":"Doe"}""", json);
    }
    [Fact]
    public void DeserializesUser()
    {
        string json = """{"name":"John","surname":"Doe"}""";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        User user = JsonSerializer.Deserialize<User>(json, options)!;

        Assert.Equal("John Doe", user.FullName);
    }

    public enum PersonType { HUMAN, DROID };
    public record Person(PersonType type, string name);

    [Fact]
    public void EnumsAreInts()
    {
        var person = new Person(PersonType.HUMAN, "Allan");

        string json = JsonSerializer.Serialize(person);

        Assert.Equal("""{"type":0,"name":"Allan"}""", json);
    }

    [Fact]
    public void EnumsAsStrings()
    {
        var person = new Person(PersonType.HUMAN, "Allan");

        var serializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };
        string json = JsonSerializer.Serialize(person, serializerOptions);

        Assert.Equal("""{"type":"HUMAN","name":"Allan"}""", json);
    }

    [Fact]
    public void EnumsAsLowerCaseStrings()
    {
        var person = new Person(PersonType.HUMAN, "Allan");

        var serializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        string json = JsonSerializer.Serialize(person, serializerOptions);

        Assert.Equal("""{"type":"human","name":"Allan"}""", json);
    }

    [Fact]
    public void SerializesUserToFile()
    {
        User value = new User(Name: "John", Surname: "Doe");

        using (var stream = File.OpenWrite("sample.json"))
        {
            JsonSerializer.Serialize(stream, value);
        }
        var json = File.ReadAllText("sample.json");
        Assert.Equal("""{"Name":"John","Surname":"Doe"}""", json);
    }


    [Fact]
    public void CreateJson()
    {
        var node = new JsonObject
        {
            ["name"] = "John",
            ["surname"] = "Doe"
        };
        string json = node.ToJsonString();

        Assert.Equal("""{"name":"John","surname":"Doe"}""", json);
    }

    [Fact]
    public void CreateJsonArray()
    {
        var node = new JsonArray(
            new JsonObject
            {
                ["name"] = "John",
                ["surname"] = "Doe",
            },
            new JsonObject
            {
                ["name"] = "Mark",
                ["surname"] = "Twain",
            }
        );
        string json = node.ToJsonString();

        Assert.Equal("""[{"name":"John","surname":"Doe"},{"name":"Mark","surname":"Twain"}]""", json);
    }

    [Fact]
    public void ReadingJson()
    {
        string json = """[{"id":1,"name":"John","surname":"Doe"},{"name":"Mark","surname":"Twain"}]""";

        var jsonArray = JsonArray.Parse(json);

        var firstElement = jsonArray![0];
        Assert.Equal("John", firstElement!["name"]!.GetValue<string>());
        Assert.Equal(1, firstElement!["id"]!.GetValue<int>());
    }

    [Fact]
    public void ReadingInmutableJson()
    {
        string json = """[{"id":1,"name":"John","surname":"Doe"},{"name":"Mark","surname":"Twain"}]""";

        using (var jsonDocument = JsonDocument.Parse(json))
        {
            Assert.Equal(2, jsonDocument.RootElement.GetArrayLength());
            var firstElement = jsonDocument.RootElement.EnumerateArray().First();
            Assert.Equal("John", firstElement.GetProperty("name").GetString());
            Assert.Equal(1, firstElement.GetProperty("id").GetInt32());
            Assert.True(firstElement.GetProperty("name").ValueEquals("John"));
        }
    }

    [Fact]
    public void ReturningAJsonElement()
    {
        string json = """[{"id":1,"name":"John","surname":"Doe"},{"name":"Mark","surname":"Twain"}]""";

        JsonElement firstElement = default;
        JsonElement secondElement = default;
        using (var jsonDocument = JsonDocument.Parse(json))
        {
            firstElement = jsonDocument.RootElement.EnumerateArray().First().Clone();
            secondElement = jsonDocument.RootElement.EnumerateArray().Last();
        }
        Assert.Equal("John", firstElement.GetProperty("name").GetString());
        Assert.Throws<ObjectDisposedException>(() => secondElement.GetProperty("name"));
    }
}

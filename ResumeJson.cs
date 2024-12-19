// Ignore Spelling: Github Linkedin Hashtags Stackoverflow Json

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResumeGenerator
{
    public static class ResumeJson
    {
        public static Root Parse(string inPath)
        {
            string jsonString = File.ReadAllText(inPath);

            Root root = JsonSerializer.Deserialize(jsonString, MyJsonContext.Default.Root)
                ?? throw new JsonException($"Could not parse {inPath}");

            return root;
        }
    }

    public class Root
    {
        public Links? Links { get; set; }
        public About? About { get; set; }
        public List<Education>? Educations { get; set; }
        public List<Experience>? Experiences { get; set; }
        public List<Work>? Works { get; set; }
        public List<HobbyProject>? HobbyProjects { get; set; }
    }

    public class EducationalInstitute
    {
        public string? Name { get; set; }
        public string? Link { get; set; }
    }

    public class Education
    {
        public string? Degree { get; set; }
        public string? Major { get; set; }
        public EducationalInstitute? Institute { get; set; }
        public string? Duration { get; set; }
        public string? Grade { get; set; }
        public object? Courses { get; set; }
    }

    public class Links
    {
        public string? Email { get; set; }
        public string? Github { get; set; }
        public string? Linkedin { get; set; }
        public string? Medium { get; set; }
        public string? Stackoverflow { get; set; }
        public string? PortfolioWebsite { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class About
    {
        public string? Name { get; set; }
        public List<string>? Autobiography { get; set; }
        public List<string>? TechnicalSkills { get; set; }
        public List<string>? ProfessionalSummaryBullets { get; set; }
        public List<string>? TechStack { get; set; }
    }

    public class Experience
    {
        public string? Position { get; set; }
        public Company? Company { get; set; }
        public string? Duration { get; set; }
        public List<string>? BulletPoints { get; set; }
        public List<string>? Hashtags { get; set; }
    }

    public class Company
    {
        public string? Name { get; set; }
        public string? Link { get; set; }
    }

    public class Work
    {
        public string? ProjectName { get; set; }
        public string? YearCompleted { get; set; }

        [JsonConverter(typeof(DescriptionConverter))]
        public List<string>? Description { get; set; }

        public string? TechStack { get; set; }
        public List<Link>? Links { get; set; }
        public List<string>? Images { get; set; }
        public bool? AlignLeft { get; set; }
    }

    public class Link
    {
        public string? Label { get; set; }
        public string? Type { get; set; }
        public string? Url { get; set; }
    }

    public class HobbyProject
    {
        public string? ProjectName { get; set; }
        public string? YearCompleted { get; set; }
        public string? Description { get; set; }
        public string? TechStack { get; set; }
        public List<Link>? Links { get; set; }
        public List<string>? Images { get; set; }
        public bool? AlignLeft { get; set; }
    }

    public class DescriptionConverter : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new List<string>();

            if (reader.TokenType == JsonTokenType.String)
            {
                // Single string value
                result.Add(reader.GetString()!);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Begin reading the array
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        result.Add(reader.GetString()!);
                    }
                    else if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    else
                    {
                        throw new JsonException($"Unexpected token {reader.TokenType} in Description array.");
                    }
                }
            }
            else
            {
                throw new JsonException($"Unexpected token {reader.TokenType} for Description property.");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
            }
            else if (value.Count == 1)
            {
                writer.WriteStringValue(value[0]);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var item in value)
                {
                    writer.WriteStringValue(item);
                }
                writer.WriteEndArray();
            }
        }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
    [JsonSerializable(typeof(Root))]
    [JsonSerializable(typeof(Links))]
    [JsonSerializable(typeof(About))]
    [JsonSerializable(typeof(Experience))]
    [JsonSerializable(typeof(Company))]
    [JsonSerializable(typeof(Work))]
    [JsonSerializable(typeof(Link))]
    [JsonSerializable(typeof(HobbyProject))]
    [JsonSerializable(typeof(List<string>))]
    internal partial class MyJsonContext : JsonSerializerContext
    {
    }
}

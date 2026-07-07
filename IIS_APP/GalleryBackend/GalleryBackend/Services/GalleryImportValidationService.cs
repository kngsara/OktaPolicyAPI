using GalleryBackend.Models;
using Json.Schema;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Schema;

namespace GalleryBackend.Services
{
    public interface IGalleryImportValidationService
    {
        Task<(bool IsValid, List<string> Errors, GalleryItem? Item)> ValidateJsonAsync(string json);
        Task<(bool IsValid, List<string> Errors, GalleryItem? Item)> ValidateXmlAsync(string xml);
    }

    public class GalleryImportValidationService : IGalleryImportValidationService
    {
        private readonly IWebHostEnvironment _environment;

        public GalleryImportValidationService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(bool IsValid, List<string> Errors, GalleryItem? Item)> ValidateJsonAsync(string json)
        {
            var errors = new List<string>();

            try
            {
                var schemaPath = System.IO.Path.Combine(_environment.ContentRootPath, "Schemas", "gallery-item.schema.json");
                var schemaText = await File.ReadAllTextAsync(schemaPath);

                var schema = JsonSchema.FromText(schemaText);
                using var document = JsonDocument.Parse(json);

                var result = schema.Evaluate(
                    document.RootElement,
                    new EvaluationOptions
                    {
                        OutputFormat = OutputFormat.List
                    });

                if (!result.IsValid)
                {
                    CollectJsonErrors(result, errors);
                    return (false, errors, null);
                }

                var item = JsonSerializer.Deserialize<GalleryItem>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (item == null)
                {
                    errors.Add("JSON nije moguće pretvoriti u GalleryItem objekt.");
                    return (false, errors, null);
                }

                item.Id = 0;

                return (true, errors, item);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return (false, errors, null);
            }
        }

        public Task<(bool IsValid, List<string> Errors, GalleryItem? Item)> ValidateXmlAsync(string xml)
        {
            var errors = new List<string>();

            try
            {
                var xsdPath = System.IO.Path.Combine(_environment.ContentRootPath, "Schemas", "gallery-item.xsd");

                var schemas = new XmlSchemaSet();
                schemas.Add("", xsdPath);

                var document = XDocument.Parse(xml);

                document.Validate(schemas, (sender, args) =>
                {
                    errors.Add(args.Message);
                });

                if (errors.Any())
                {
                    return Task.FromResult((false, errors, (GalleryItem?)null));
                }

                var root = document.Root;

                if (root == null || root.Name.LocalName != "galleryItem")
                {
                    errors.Add("Root element mora biti <galleryItem>.");
                    return Task.FromResult((false, errors, (GalleryItem?)null));
                }

                var item = new GalleryItem
                {
                    Id = 0,
                    ImgurId = GetRequiredValue(root, "imgurId"),
                    Title = GetRequiredValue(root, "title"),
                    Link = GetRequiredValue(root, "link"),
                    Type = GetRequiredValue(root, "type"),
                    Views = int.Parse(GetRequiredValue(root, "views")),
                    Ups = int.Parse(GetRequiredValue(root, "ups")),
                    Downs = int.Parse(GetRequiredValue(root, "downs")),
                    Score = int.Parse(GetRequiredValue(root, "score")),
                    IsAlbum = bool.Parse(GetRequiredValue(root, "isAlbum")),
                    CreatedAt = DateTime.Parse(GetRequiredValue(root, "createdAt"))
                };

                return Task.FromResult((true, errors, item));
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return Task.FromResult((false, errors, (GalleryItem?)null));
            }
        }

        private static string GetRequiredValue(XElement root, string elementName)
        {
            return root.Element(elementName)?.Value
                   ?? throw new InvalidOperationException($"Nedostaje element <{elementName}>.");
        }

        private static void CollectJsonErrors(EvaluationResults result, List<string> errors)
        {
            if (result.Errors != null && result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    errors.Add($"{result.InstanceLocation}: {error.Value}");
                }
            }

            if (result.Details != null)
            {
                foreach (var detail in result.Details)
                {
                    CollectJsonErrors(detail, errors);
                }
            }

            if (!result.IsValid && !errors.Any())
            {
                errors.Add("JSON nije valjan prema zadanoj JSON shemi.");
            }
        }
    }
}
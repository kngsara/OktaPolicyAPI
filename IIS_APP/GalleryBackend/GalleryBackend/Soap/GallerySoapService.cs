using System.Xml.Linq;
using System.Xml.XPath;

namespace GalleryBackend.Soap
{
    public class GallerySoapService : IGallerySoapService
    {
        private readonly string _xmlFilePath = @"C:\IISProject\shared\gallery-items.xml";

        public string SearchGalleryItems(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return "<galleryItems></galleryItems>";
            }

            if (!File.Exists(_xmlFilePath))
            {
                return $"<error>XML datoteka nije pronađena: {_xmlFilePath}</error>";
            }

            try
            {
                var document = XDocument.Load(_xmlFilePath);

                string safeTerm = term.Trim().ToLower();

                var matchingItems = document
                    .XPathSelectElements("//galleryItem")
                    .Where(item =>
                        ContainsIgnoreCase(item.Element("title")?.Value, safeTerm) ||
                        ContainsIgnoreCase(item.Element("type")?.Value, safeTerm) ||
                        ContainsIgnoreCase(item.Element("link")?.Value, safeTerm) ||
                        ContainsIgnoreCase(item.Element("imgurId")?.Value, safeTerm)
                    )
                    .ToList();

                var resultXml = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("galleryItems", matchingItems)
                );

                return resultXml.ToString();
            }
            catch (Exception ex)
            {
                return $"<error>{System.Security.SecurityElement.Escape(ex.Message)}</error>";
            }
        }

        private static bool ContainsIgnoreCase(string? source, string term)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return source.ToLower().Contains(term);
        }
    }
}
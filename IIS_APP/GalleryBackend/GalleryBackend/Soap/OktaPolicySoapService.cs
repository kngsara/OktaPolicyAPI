using System.Xml.Linq;
using System.Xml.XPath;

namespace OktaBackend.Soap
{
    public class OktaPolicySoapService : IOktaPolicySoapService
    {
        private readonly string _xmlFilePath = @"C:\IISProject\shared\okta-policies.xml";
        private readonly string _xmlFilePathTwo = @"C:\IISProject\shared\okta-policies-soap.xml";

        public string SearchOktaPolicies(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return "<oktaPolicies></oktaPolicies>";
            }

            if (!File.Exists(_xmlFilePath))
            {
                return $"<error>XML datoteka nije pronađena: {_xmlFilePath}</error>";
            }

            try
            {
                var document = XDocument.Load(_xmlFilePath);

                var safeTerm = term.Trim().ToLower();

                var matchingPolicies = document
                    .XPathSelectElements("//oktaPolicy")
                    .Where(policy =>
                        ContainsIgnoreCase(policy.Element("oktaId")?.Value, safeTerm) ||
                        ContainsIgnoreCase(policy.Element("name")?.Value, safeTerm) ||
                        ContainsIgnoreCase(policy.Element("type")?.Value, safeTerm) ||
                        ContainsIgnoreCase(policy.Element("status")?.Value, safeTerm) ||
                        ContainsIgnoreCase(policy.Element("priority")?.Value, safeTerm) ||
                        ContainsIgnoreCase(policy.Element("link")?.Value, safeTerm)
                    )
                    .ToList();

                var resultXml = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("oktaPolicies", matchingPolicies)
                );

                return resultXml.ToString();
            }
            catch (Exception ex)
            {
                return $"<error>{System.Security.SecurityElement.Escape(ex.Message)}</error>";
            }
        }

        public string SaveOktaPolicySoap(string soap)
        {
            if (string.IsNullOrWhiteSpace(soap))
            {
                return "<error>SOAP sadržaj je prazan.</error>";
            }

            try
            {
                var directory = System.IO.Path.GetDirectoryName(_xmlFilePathTwo);

                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var document = XDocument.Parse(soap);

                document.Save(_xmlFilePathTwo);

                return "<success>SOAP XML je uspješno spremljen.</success>";
            }
            catch (System.Xml.XmlException ex)
            {
                return $"<error>Neispravan XML/SOAP format: {System.Security.SecurityElement.Escape(ex.Message)}</error>";
            }
            catch (Exception ex)
            {
                return $"<error>Greška prilikom spremanja SOAP XML-a: {System.Security.SecurityElement.Escape(ex.Message)}</error>";
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
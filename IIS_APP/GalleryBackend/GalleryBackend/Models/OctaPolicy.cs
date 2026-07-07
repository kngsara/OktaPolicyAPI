using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace OktaBackend.Models
{
    [XmlRoot("oktaPolicy")]
    public class OktaPolicy
    {
        [XmlIgnore]
        public int Id { get; set; }

        [Required]
        [XmlElement("oktaId")]
        public string OktaId { get; set; } = string.Empty;

        [Required]
        [XmlElement("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [XmlElement("type")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [XmlElement("status")]
        public string Status { get; set; } = string.Empty;

        [XmlElement("priority")]
        public int Priority { get; set; }

        [XmlElement("link")]
        public string Link { get; set; } = string.Empty;

        [XmlElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [XmlElement("lastUpdatedAt")]
        public DateTime? LastUpdatedAt { get; set; }
    }
}
namespace GalleryFrontend.Models
{
    public class SoapOktaPolicy
    {
        public string OktaId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int Priority { get; set; }

        public string Link { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }
    }
}
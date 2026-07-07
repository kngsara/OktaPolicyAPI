namespace GalleryBackend.Models
{
    public class GalleryItem
    {
        public int Id { get; set; }

        public string ImgurId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int Views { get; set; }

        public int Ups { get; set; }

        public int Downs { get; set; }

        public int Score { get; set; }

        public bool IsAlbum { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
namespace GalleryFrontend.Models
{
    public class GraphQlGalleryResponse
    {
        public GraphQlGalleryData? Data { get; set; }
    }

    public class GraphQlGalleryData
    {
        public List<GalleryItem> GalleryItems { get; set; } = new();
    }
}
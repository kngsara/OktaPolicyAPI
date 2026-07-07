namespace GalleryFrontend.Models
{
    public class GraphQlOktaPoliciesResponse
    {
        public GraphQlOktaPoliciesData? Data { get; set; }
    }

    public class GraphQlOktaPoliciesData
    {
        public List<OktaPolicy> OktaPolicies { get; set; } = new();
    }
}
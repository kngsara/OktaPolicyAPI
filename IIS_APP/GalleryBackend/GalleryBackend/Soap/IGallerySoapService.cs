using System.ServiceModel;

namespace GalleryBackend.Soap
{
    [ServiceContract]
    public interface IGallerySoapService
    {
        [OperationContract]
        string SearchGalleryItems(string term);
    }
}
using System.ServiceModel;

namespace OktaBackend.Soap
{
    [ServiceContract]
    public interface IOktaPolicySoapService
    {
        [OperationContract]
        string SearchOktaPolicies(string term);

    }
}
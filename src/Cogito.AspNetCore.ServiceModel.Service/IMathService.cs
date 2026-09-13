using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel.Service
{

    /// <summary>
    /// Interface for all Efm web services
    /// </summary>
    [ServiceContract(Namespace = "http://math/")]
    public interface IMathService
    {

        [OperationContract]
        int Add(int a, int b);

    }

}

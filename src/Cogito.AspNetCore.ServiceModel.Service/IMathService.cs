using System.ServiceModel;

namespace Cogito.ServiceModel.AspNetCore.Service
{

    [ServiceContract]
    public interface IMathService
    {

        [OperationContract]
        int Add(int x, int y);

    }

}

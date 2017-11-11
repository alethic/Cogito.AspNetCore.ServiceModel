using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel.Service
{

    [ServiceContract]
    public interface IMathService
    {

        [OperationContract]
        int Add(int x, int y);

    }

}

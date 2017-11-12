using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel.Service
{

    [ServiceContract(Namespace = "http://tempuri.org/", Name = "Math")]
    public interface IMathService
    {

        [OperationContract(Action = "Add")]
        int Add(int x, int y);

    }

}

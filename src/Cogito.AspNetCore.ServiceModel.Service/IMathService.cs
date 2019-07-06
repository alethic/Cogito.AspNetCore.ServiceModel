using System.ServiceModel;
using System.Threading.Tasks;

namespace Cogito.AspNetCore.ServiceModel.Service
{

    [ServiceContract(Namespace = "http://tempuri.org/", Name = "Math")]
    public interface IMathService
    {

        [OperationContract(Action = "Add")]
        Task<int> Add(int x, int y);

    }

}

using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel.Service.More
{

    [ServiceContract]
    public interface IOtherContract
    {

        [OperationContract]
        void Do();

    }

    public class FooBar
    {



    }

}

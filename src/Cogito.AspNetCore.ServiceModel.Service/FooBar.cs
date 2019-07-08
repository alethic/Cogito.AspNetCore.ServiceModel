using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel.Service.More
{

    [ServiceContract(Namespace = "http://tempuri.org/", Name = "Other")]
    public interface IOtherContract
    {

        [OperationContract]
        void Do();

    }

    public class FooBar
    {



    }

}

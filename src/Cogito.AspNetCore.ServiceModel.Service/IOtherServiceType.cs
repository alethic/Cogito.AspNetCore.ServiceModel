using System.ServiceModel;

using LexisNexis.EFM;

namespace Cogito.AspNetCore.ServiceModel.Service
{

    [ServiceContract]
    public interface IOtherServiceType
    {

        [OperationContract]
        void Do2();

    }

}

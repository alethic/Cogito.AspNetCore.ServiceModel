using System.ServiceModel;

namespace LexisNexis.EFM
{

    /// <summary>
    /// Interface for all Efm web services
    /// </summary>
    [ServiceContract(Namespace = "http://www.lexisnexis.com/efm/EfmAdaptorWebServices")]
    public interface IEfmWebService
    {

        /// <summary>
        /// Creates a new web service transaction.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [OperationContract(IsInitiating =false)]
        object InitiateTransaction(object data);

    }

}

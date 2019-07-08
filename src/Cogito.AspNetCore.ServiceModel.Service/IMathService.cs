using System.ServiceModel;

namespace LexisNexis.EFM
{

    /// <summary>
    /// Interface for all Efm web services
    /// </summary>
    [ServiceContract(Namespace = "http://www.lexisnexis.com/efm/EfmAdaptorWebServices")]
    public interface ILoxnpWebService : IEfmWebService
    {

        /// <summary>
        /// Takes a LOXNP mesage and sends it to the adaptor.
        /// </summary>
        /// <param name="xml"></param>
        [OperationContract]
        void Enqueue(string xml);

        /// <summary>
        /// Creates a new web service transaction.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="documents"></param>
        /// <returns></returns>
        [OperationContract]
        object ReceiveMTOM(object data, object documents);

    }

}

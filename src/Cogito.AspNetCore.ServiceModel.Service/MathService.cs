using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel.Service
{


    public class MathService :
        IMathService
    {

        public int Add(int x, int y)
        {
            var p =OperationContext.Current.IncomingMessageProperties;
            return x + y;

        }

    }

}

using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel.Service
{

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class MathService : IMathService
    {

        public int Add(int a, int b)
        {
            return a + b;
        }

    }

}

using System.Threading.Tasks;

using Cogito.AspNetCore.ServiceModel.Service.More;

namespace Cogito.AspNetCore.ServiceModel.Service
{


    public class MathService : IMathService
    {

        public async Task<int> Add(int x, int y, FooBar o)
        {
            return x + y;
        }

        public void Do()
        {

        }

    }

}

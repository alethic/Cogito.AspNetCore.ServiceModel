using System.ServiceModel;
using System.Threading.Tasks;

namespace Cogito.AspNetCore.ServiceModel.Service
{


    public class MathService :
        IMathService
    {

        public async Task<int> Add(int x, int y)
        {
            return x + y;
        }

    }

}

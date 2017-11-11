namespace Cogito.ServiceModel.AspNetCore.Service
{


    public class MathService :
        IMathService
    {

        public int Add(int x, int y)
        {
            return x + y;
        }

    }

}

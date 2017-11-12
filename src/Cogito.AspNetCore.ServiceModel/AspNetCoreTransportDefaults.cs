namespace Cogito.AspNetCore.ServiceModel
{

    static class AspNetCoreTransportDefaults
    {

        public const long MaxReceivedMessageSize = 65536;
        public const int MaxDrainSize = (int)MaxReceivedMessageSize;
        public const long MaxBufferPoolSize = 512 * 1024;
        public const int MaxBufferSize = (int)MaxReceivedMessageSize;
        public const bool RequireClientCertificate = false;
        public const int MaxFaultSize = MaxBufferSize;
        public const int MaxSecurityFaultSize = 16384;

    }

}

using Microsoft.Extensions.DependencyInjection;


namespace CAPI.Tests.NetCore.Helpers
{
    public static class Ioc
    {
        public static ServiceCollection GetServices()
        {
            var services = new ServiceCollection();

            return services;
        }
    }
}
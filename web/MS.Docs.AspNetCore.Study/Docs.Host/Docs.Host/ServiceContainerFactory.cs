using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docs.Host
{
    public class ServiceContainerFactory : IServiceProviderFactory<ServiceContainer>
    {
        public ServiceContainer CreateBuilder(IServiceCollection services)
        {
            return new ServiceContainer(); 
        }

        public IServiceProvider CreateServiceProvider(ServiceContainer containerBuilder)
        {
            throw new NotImplementedException();
        }
    }
}

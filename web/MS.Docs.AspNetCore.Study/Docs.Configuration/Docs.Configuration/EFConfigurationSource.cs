using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Docs.Configuration
{
    internal class EFConfigurationSource : IConfigurationSource
    {
        private Action<DbContextOptionsBuilder> optionsAction;

        public EFConfigurationSource(Action<DbContextOptionsBuilder> optionsAction)
        {
            this.optionsAction = optionsAction;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EFConfigurationProvider(optionsAction);
        }
    }
}
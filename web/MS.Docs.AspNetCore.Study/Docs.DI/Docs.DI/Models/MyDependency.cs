using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Docs.DI.Models
{
    public class MyDependency : IMyDependency
    {
        private readonly ILogger<MyDependency> _logger;

        public MyDependency(ILogger<MyDependency> logger)
        {
            _logger = logger;
        }

        public Task WriteMessage(string message)
        {
            _logger.LogInformation($"MyDependency类中的WriteMessage方法被调用。 Message：{message}");
            return Task.FromResult(10);
        }
    }
}
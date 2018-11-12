using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docs.DI.Models
{
    public interface IMyDependency
    {
        Task WriteMessage(string message);
    }
}

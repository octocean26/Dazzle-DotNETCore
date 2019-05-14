using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace My.ApplicationParts.Study
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false,Inherited =true)]
    public class WyControllerNameConvention : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
             if(controller.ControllerType.GetGenericTypeDefinition()!=typeof(WyController<>))
             {
                return;
             }

            var entityType = controller.ControllerType.GetGenericArguments()[0];
            controller.ControllerName = entityType.Name;
        }
    }
}

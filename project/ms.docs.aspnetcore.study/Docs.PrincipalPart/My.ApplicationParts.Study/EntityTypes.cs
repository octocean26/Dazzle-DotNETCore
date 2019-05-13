using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace My.ApplicationParts.Study
{
    public class EntityTypes
    {
        public static IReadOnlyList<TypeInfo> Types = new List<TypeInfo>(){
            typeof(Sproket).GetTypeInfo(),
            typeof(Widget).GetTypeInfo()
        };


        public class Sproket{ }
        public class Widget{ }
    }




}

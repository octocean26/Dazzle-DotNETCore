using System;

namespace Docs.DI.Models
{
    public class Operation : IOperationTransient, IOperationScoped, IOperationSingleton, IOperationSingletonInstance
    {
        //如果无参实例化，默认调用有参实例
        public Operation() : this(Guid.NewGuid())
        {
        }

        public Operation(Guid id)
        {
            OperationId = id;
        }

        //实现接口属性
        public Guid OperationId { get; private set; }
    }
}
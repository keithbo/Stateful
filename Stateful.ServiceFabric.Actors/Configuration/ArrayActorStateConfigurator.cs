namespace Stateful.ServiceFabric.Actors.Configuration
{
    using Stateful.Configuration;

    public class ArrayActorStateConfigurator : ActorStateConfiguratorBase<IArrayActorStateActivatorConfiguration>, IArrayStateConfigurator
    {
        public long Length { set => Configuration.Length = value; }

        public ArrayActorStateConfigurator(IArrayActorStateActivatorConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
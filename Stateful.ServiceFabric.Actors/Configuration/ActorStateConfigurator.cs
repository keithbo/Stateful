namespace Stateful.ServiceFabric.Actors.Configuration
{
    using Stateful.Configuration;

    public class ActorStateConfigurator : ActorStateConfiguratorBase<IActorStateActivatorConfiguration>
    {
        public ActorStateConfigurator(IActorStateActivatorConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
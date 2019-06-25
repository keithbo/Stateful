namespace Stateful.ServiceFabric.Actors.Configuration
{
    public class ActorStateConfigurator : ActorStateConfiguratorBase<IActorStateActivatorConfiguration>
    {
        public ActorStateConfigurator(IActorStateActivatorConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
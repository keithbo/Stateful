namespace Stateful.ServiceFabric.Actors.Configuration
{
    public class ObjectActivatorConfiguration<TValue> : IActorStateActivatorConfiguration
    {
        public IStateKey Key { set; get; }

        public IActorStateActivator Build()
        {
            return new ActorStateActivator(Key, (stateManager) => new ActorObjectState<TValue>(stateManager, Key));
        }
    }
}
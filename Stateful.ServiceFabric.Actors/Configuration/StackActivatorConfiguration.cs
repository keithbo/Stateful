namespace Stateful.ServiceFabric.Actors.Configuration
{
    public class StackActivatorConfiguration<TValue> : IActorStateActivatorConfiguration
    {
        public IStateKey Key { get; set; }

        public IActorStateActivator Build()
        {
            return new ActorStateActivator(Key, (stateManager) => new ActorStackState<TValue>(stateManager, Key));
        }
    }
}
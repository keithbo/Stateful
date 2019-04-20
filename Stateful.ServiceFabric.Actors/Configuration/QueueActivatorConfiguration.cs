namespace Stateful.ServiceFabric.Actors.Configuration
{
    public class QueueActivatorConfiguration<TValue> : IActorStateActivatorConfiguration
    {
        public IStateKey Key { get; set; }

        public IActorStateActivator Build()
        {
            return new ActorStateActivator(Key, (stateManager) => new ActorQueueState<TValue>(stateManager, Key));
        }
    }
}
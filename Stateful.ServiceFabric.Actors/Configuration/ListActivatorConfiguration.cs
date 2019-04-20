namespace Stateful.ServiceFabric.Actors.Configuration
{
    public class ListActivatorConfiguration<TValue> : IActorStateActivatorConfiguration
    {
        public IStateKey Key { get; set; }

        public IActorStateActivator Build()
        {
            return new ActorStateActivator(Key, (stateManager) => new ActorListState<TValue>(stateManager, Key));
        }
    }
}
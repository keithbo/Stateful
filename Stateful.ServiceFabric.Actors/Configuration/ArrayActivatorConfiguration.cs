namespace Stateful.ServiceFabric.Actors.Configuration
{
    public class ArrayActivatorConfiguration<TValue> : IArrayActorStateActivatorConfiguration
    {
        public IStateKey Key { get; set; }

        public long Length { get; set; }

        public IActorStateActivator Build()
        {
            return new ActorStateActivator(Key, (stateManager) => new ActorArrayState<TValue>(stateManager, Key, Length));
        }
    }
}
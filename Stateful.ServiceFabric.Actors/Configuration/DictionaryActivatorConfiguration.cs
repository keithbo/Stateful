namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;

    public class DictionaryActivatorConfiguration<TKey, TValue> : IActorStateActivatorConfiguration
        where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        public IStateKey Key { get; set; }

        public IActorStateActivator Build()
        {
            return new ActorStateActivator(Key, (stateManager) => new ActorDictionaryState<TKey, TValue>(stateManager, Key));
        }
    }
}
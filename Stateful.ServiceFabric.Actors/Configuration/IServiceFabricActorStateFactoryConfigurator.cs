namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public interface IServiceFabricActorStateFactoryConfigurator
    {
        IActorStateManager StateManager { set; }

        void AddObject<TValue>(IStateKey key);

        void AddList<TValue>(IStateKey key);

        void AddDictionary<TKey, TValue>(IStateKey key) where TKey : IEquatable<TKey>, IComparable<TKey>;

        void AddArray<TValue>(IStateKey key, long length);

        void AddQueue<TValue>(IStateKey key);

        void AddStack<TValue>(IStateKey key);
    }
}
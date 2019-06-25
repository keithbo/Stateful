namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.Configuration;

    public interface IActorStateFactoryConfigurator
    {
        IActorStateManager StateManager { get; }

        void AddObject<TValue>(Action<IStateConfigurator> configure);

        void AddList<TValue>(Action<IStateConfigurator> configure);

        void AddDictionary<TKey, TValue>(Action<IStateConfigurator> configure) where TKey : IEquatable<TKey>, IComparable<TKey>;

        void AddArray<TValue>(Action<IArrayStateConfigurator> configure);

        void AddQueue<TValue>(Action<IStateConfigurator> configure);

        void AddStack<TValue>(Action<IStateConfigurator> configure);
    }
}
namespace Stateful.ServiceFabric.Actors
{
    using System;
    using Stateful.ServiceFabric.Actors.Configuration;

    public static class StateFactoryConfiguratorExtensions
    {
        public static void AddObject<TValue>(this IServiceFabricActorStateFactoryConfigurator configurator, string name)
        {
            configurator.AddObject<TValue>(config =>
            {
                config.Key = new StateKey(name);
            });
        }

        public static void AddList<TValue>(this IServiceFabricActorStateFactoryConfigurator configurator, string name)
        {
            configurator.AddList<TValue>(config =>
            {
                config.Key = new StateKey(name);
            });
        }

        public static void AddDictionary<TKey, TValue>(this IServiceFabricActorStateFactoryConfigurator configurator, string name)
            where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            configurator.AddDictionary<TKey, TValue>(config =>
            {
                config.Key = new StateKey(name);
            });
        }

        public static void AddArray<TValue>(this IServiceFabricActorStateFactoryConfigurator configurator, string name, long length)
        {
            configurator.AddArray<TValue>(config =>
            {
                config.Key = new StateKey(name);
                config.Length = length;
            });
        }

        public static void AddQueue<TValue>(this IServiceFabricActorStateFactoryConfigurator configurator, string name)
        {
            configurator.AddQueue<TValue>(config =>
            {
                config.Key = new StateKey(name);
            });
        }

        public static void AddStack<TValue>(this IServiceFabricActorStateFactoryConfigurator configurator, string name)
        {
            configurator.AddStack<TValue>(config =>
            {
                config.Key = new StateKey(name);
            });
        }
    }
}
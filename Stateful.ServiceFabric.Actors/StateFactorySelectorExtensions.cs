namespace Stateful.ServiceFabric.Actors
{
    using System;
    using Stateful.Configuration;
    using Stateful.ServiceFabric.Actors.Configuration;

    public static class StateFactorySelectorExtensions
    {
        public static IStateFactory CreateUsingServiceFabricActors(this IStateFactorySelector selector, Action<IActorStateFactoryConfigurator> configure)
        {
            var configuration = new ActorStateFactoryConfiguration();
            var configurator = new ActorStateFactoryConfigurator(configuration);

            configure(configurator);

            return configuration.Build();
        }
    }
}
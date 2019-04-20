namespace Stateful.ServiceFabric.Actors
{
    using System;
    using Stateful.Configuration;
    using Stateful.ServiceFabric.Actors.Configuration;

    public static class StateFactorySelectorExtensions
    {
        public static IStateFactory CreateUsingServiceFabricActors(this IStateFactorySelector selector, Action<IServiceFabricActorStateFactoryConfigurator> configure)
        {
            var configuration = new ServiceFabricActorStateConfiguration();
            var configurator = new ServiceFabricActorStateFactoryConfigurator(configuration);

            configure(configurator);

            return configuration.Build();
        }
    }
}
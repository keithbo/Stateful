namespace Stateful.ServiceFabric.Actors
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.Configuration;
    using Stateful.ServiceFabric.Actors.Configuration;

    public static class StateFactorySelectorExtensions
    {
        public static IStateFactory CreateUsingServiceFabricActors(this IStateFactorySelector selector, IActorStateManager stateManager, Action<IActorStateFactoryConfigurator> configure)
        {
            if (stateManager is null)
            {
                throw new ArgumentNullException(nameof(stateManager));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var configuration = new ActorStateFactoryConfiguration
            {
                StateManager = stateManager
            };
            var configurator = new ActorStateFactoryConfigurator(configuration);

            configure(configurator);

            return configuration.Build();
        }
    }
}
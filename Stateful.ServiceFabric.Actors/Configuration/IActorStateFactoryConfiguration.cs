namespace Stateful.ServiceFabric.Actors.Configuration
{
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.Configuration;

    public interface IActorStateFactoryConfiguration : IStateFactoryConfiguration
    {
        IActorStateManager StateManager { get; set; }

        void AddStateActivator(IActorStateActivator activator);
    }
}
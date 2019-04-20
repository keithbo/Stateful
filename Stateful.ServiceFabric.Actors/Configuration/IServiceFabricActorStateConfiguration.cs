namespace Stateful.ServiceFabric.Actors.Configuration
{
    using Microsoft.ServiceFabric.Actors.Runtime;

    public interface IServiceFabricActorStateConfiguration : IStateConfiguration
    {
        IActorStateManager StateManager { get; set; }

        void AddStateActivator(IActorStateActivator activator);
    }
}
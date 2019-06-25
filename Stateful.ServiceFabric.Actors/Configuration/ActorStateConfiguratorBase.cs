namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using Stateful.Configuration;

    public abstract class ActorStateConfiguratorBase<TConfiguration> : IStateConfigurator
        where TConfiguration : class, IActorStateActivatorConfiguration
    {
        protected TConfiguration Configuration { get; }

        protected ActorStateConfiguratorBase(TConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IStateKey Key
        {
            set => Configuration.Key = value;
        }
    }
}
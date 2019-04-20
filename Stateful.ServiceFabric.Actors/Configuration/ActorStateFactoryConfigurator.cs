namespace Stateful.ServiceFabric.Actors.Configuration
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.Configuration;

    public class ActorStateFactoryConfigurator : IActorStateFactoryConfigurator
    {
        private readonly IActorStateFactoryConfiguration _configuration;

        public ActorStateFactoryConfigurator(IActorStateFactoryConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc />
        public IActorStateManager StateManager
        {
            set => _configuration.StateManager = value;
        }

        /// <inheritdoc />
        public void AddObject<TValue>(Action<IStateConfigurator> configure)
        {
            var configuration = new ObjectActivatorConfiguration<TValue>();
            var configurator = new ActorStateConfigurator(configuration);
            configure(configurator);

            _configuration.AddStateActivator(configuration.Build());
        }

        /// <inheritdoc />
        public void AddList<TValue>(Action<IStateConfigurator> configure)
        {
            var configuration = new ListActivatorConfiguration<TValue>();
            var configurator = new ActorStateConfigurator(configuration);
            configure(configurator);

            _configuration.AddStateActivator(configuration.Build());
        }

        /// <inheritdoc />
        public void AddDictionary<TKey, TValue>(Action<IStateConfigurator> configure) where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            var configuration = new DictionaryActivatorConfiguration<TKey, TValue>();
            var configurator = new ActorStateConfigurator(configuration);
            configure(configurator);

            _configuration.AddStateActivator(configuration.Build());
        }

        /// <inheritdoc />
        public void AddArray<TValue>(Action<IArrayStateConfigurator> configure)
        {
            var configuration = new ArrayActivatorConfiguration<TValue>();
            var configurator = new ArrayActorStateConfigurator(configuration);
            configure(configurator);

            _configuration.AddStateActivator(configuration.Build());
        }

        /// <inheritdoc />
        public void AddQueue<TValue>(Action<IStateConfigurator> configure)
        {
            var configuration = new QueueActivatorConfiguration<TValue>();
            var configurator = new ActorStateConfigurator(configuration);
            configure(configurator);

            _configuration.AddStateActivator(configuration.Build());
        }

        /// <inheritdoc />
        public void AddStack<TValue>(Action<IStateConfigurator> configure)
        {
            var configuration = new StackActivatorConfiguration<TValue>();
            var configurator = new ActorStateConfigurator(configuration);
            configure(configurator);

            _configuration.AddStateActivator(configuration.Build());
        }
    }
}
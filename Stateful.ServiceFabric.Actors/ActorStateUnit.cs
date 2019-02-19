namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;

    internal class ActorStateUnit : IUnit
    {
        private Lazy<IActorStateManager> _stateManager;

        private readonly Func<IActorStateManager, IStateKey, IState> _stateFactory;

        private readonly ConcurrentDictionary<IStateKey, IState> _stateCache = new ConcurrentDictionary<IStateKey, IState>();

        private bool _isDisposed;

        public ActorStateUnit(Func<IActorStateManager> stateManagerFactory, Func<IActorStateManager, IStateKey, IState> stateFactory)
        {
            _stateManager = new Lazy<IActorStateManager>(stateManagerFactory ?? throw new ArgumentNullException(nameof(stateManagerFactory)));
            _stateFactory = stateFactory ?? throw new ArgumentNullException(nameof(stateFactory));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _stateCache.Clear();
            _stateManager = null;
        }

        /// <inheritdoc />
        public TState Get<TState>(IStateKey key) where TState : IState
        {
            CheckDisposed();
            return (TState)_stateCache.GetOrAdd(key, k => _stateFactory(_stateManager.Value, k));
        }

        /// <inheritdoc />
        public void Abort()
        {
            CheckDisposed();
            Dispose();
        }

        /// <inheritdoc />
        public Task CommitAsync()
        {
            CheckDisposed();
            return Task.CompletedTask;
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException($"{nameof(ActorStateUnit)} is disposed");
            }
        }
    }
}
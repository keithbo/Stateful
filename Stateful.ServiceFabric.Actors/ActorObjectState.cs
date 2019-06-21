namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorObjectState<T> : IObjectState<T>
    {
        private readonly IActorStateManager _stateManager;

        protected string Name => Key.ToString();

        public IStateKey Key { get; }

        public ActorObjectState(IActorStateManager stateManager, IStateKey key)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public Task<bool> HasStateAsync(CancellationToken cancellationToken)
        {
            return _stateManager.ContainsStateAsync(Name, cancellationToken);
        }

        public Task DeleteStateAsync(CancellationToken cancellationToken)
        {
            return _stateManager.TryRemoveStateAsync(Name, cancellationToken);
        }

        public async Task<ConditionalValue<T>> TryGetAsync(CancellationToken cancellationToken)
        {
            var value = await _stateManager.TryGetStateAsync<T>(Name, cancellationToken);
            return new ConditionalValue<T>(value.HasValue, value.Value);
        }

        public async Task<T> SetAsync(T value, CancellationToken cancellationToken)
        {
            await _stateManager.SetStateAsync(Name, value, cancellationToken);
            return value;
        }
    }
}
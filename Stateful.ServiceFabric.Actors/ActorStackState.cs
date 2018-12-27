namespace Stateful.ServiceFabric
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Internals;

    public class ActorStackState<T> : LinkedCollectionStateBase<T>, IStackState<T>
    {
        public ActorStackState(IActorStateManager stateManager, string name)
            : base(stateManager, name)
        {
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryPeekAsync(CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            if (!manifest.First.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var key = IndexToKey(manifest.First.Value);
            var node = await StateManager.TryGetStateAsync<LinkedNode<T>>(key, cancellationToken);
            return node.HasValue ? new ConditionalValue<T>(node.Value.Value) : new ConditionalValue<T>();
        }

        /// <inheritdoc />
        public Task PushAsync(T value, CancellationToken cancellationToken)
        {
            return InsertFirstAsync(new [] { value }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryPopAsync(CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            if (!manifest.First.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var key = IndexToKey(manifest.First.Value);
            var nodeResult = await StateManager.TryGetStateAsync<LinkedNode<T>>(key, cancellationToken);
            if (!nodeResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            await RemoveCoreAsync(manifest, null, null, key, nodeResult.Value, cancellationToken);

            return new ConditionalValue<T>(nodeResult.Value.Value);
        }
    }
}
namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Actors.Internals;

    /// <summary>
    /// Indexed state collection. This state is a linked-list collection that can be accessed via absolute index.
    /// </summary>
    public class ActorListState<T> : LinkedCollectionStateBase<T>, IListState<T>
    {
        public ActorListState(IActorStateManager stateManager, IStateKey key)
            : base(stateManager, key)
        {
        }

        /// <inheritdoc />
        public Task AddAsync(T value, CancellationToken cancellationToken)
        {
            return InsertLastAsync(new [] { value }, cancellationToken);
        }

        /// <inheritdoc />
        public Task AddRangeAsync(IEnumerable<T> values, CancellationToken cancellationToken)
        {
            return InsertLastAsync(values, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<IList<T>>> TryGetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<T>();
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<IList<T>>();
            }

            var manifest = manifestResult.Value;
            LinkedNode<T> currentNode;
            for (var internalIndex = manifest.First; internalIndex.HasValue; internalIndex = currentNode.Next)
            {
                currentNode = await StateManager.GetStateAsync<LinkedNode<T>>(IndexToKey(internalIndex.Value), cancellationToken);
                result.Add(currentNode.Value);
            }

            return new ConditionalValue<IList<T>>(result);
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryGetAsync(long index, CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            if (index < 0 || index >= manifest.Count)
            {
                return new ConditionalValue<T>();
            }

            long i = 0;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => i++ == index, cancellationToken);

            return foundNode != null ? new ConditionalValue<T>(foundNode.Value) : new ConditionalValue<T>();
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryFindAsync(Predicate<T> match, CancellationToken cancellationToken)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => match(node.Value), cancellationToken);

            return foundNode != null ? new ConditionalValue<T>(foundNode.Value) : new ConditionalValue<T>();
        }

        /// <inheritdoc />
        public async Task InsertAsync(long index, T value, CancellationToken cancellationToken)
        {
            await InsertAtAsync(index, new [] { value }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task InsertRangeAsync(long index, IEnumerable<T> values, CancellationToken cancellationToken)
        {
            await InsertAtAsync(index, values, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAtAsync(long index, CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                throw new IndexOutOfRangeException("List is empty");
            }

            var manifest = manifestResult.Value;
            if (index < 0 || index >= manifest.Count)
            {
                throw new IndexOutOfRangeException($"Index {index} must be in range [0, {manifest.Count})");
            }

            long i = 0;
            await RemoveAsync(value => i++ == index, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAsync(Predicate<T> match, CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return;
            }

            var manifest = manifestResult.Value;
            string previousKey = null;
            LinkedNode<T> previousNode = null;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) =>
            {
                var success = match(node.Value);
                if (!success)
                {
                    previousKey = key;
                    previousNode = node;
                }

                return success;
            }, cancellationToken);

            if (foundNode != null)
            {
                await RemoveCoreAsync(manifest, previousKey, previousNode, foundKey, foundNode, cancellationToken);
            }
        }
    }
}
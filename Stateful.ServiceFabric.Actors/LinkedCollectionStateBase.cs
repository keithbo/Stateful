namespace Stateful.ServiceFabric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Actors.Internals;

    public abstract class LinkedCollectionStateBase<T> : ICollectionState<T>
    {
        protected const string IndexKeyFormat = "{0}:{1:X}";

        protected IActorStateManager StateManager { get; }

        protected string Name => Key.ToString();

        public IStateKey Key { get; }

        protected LinkedCollectionStateBase(IActorStateManager stateManager, IStateKey key)
        {
            StateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <inheritdoc />
        public Task<bool> HasStateAsync(CancellationToken cancellationToken)
        {
            return StateManager.ContainsStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteStateAsync(CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return;
            }

            var manifest = manifestResult.Value;
            LinkedNode<T> currentNode;
            for (var internalIndex = manifest.First; internalIndex.HasValue; internalIndex = currentNode?.Next)
            {
                var key = IndexToKey(internalIndex.Value);
                currentNode = await StateManager.GetStateAsync<LinkedNode<T>>(key, cancellationToken);
                await StateManager.RemoveStateAsync(key, cancellationToken);
            }

            await StateManager.RemoveStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            var manifest = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            return manifest.HasValue ? manifest.Value.Count : 0;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsAsync(Predicate<T> predicate, CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return false;
            }

            var manifest = manifestResult.Value;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => predicate(node.Value), cancellationToken);

            return foundNode != null;
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new AsyncEnumerator(this);
        }

        /// <summary>
        /// Insert a sequence of values before the current 0th node of the collection
        /// </summary>
        /// <param name="values">sequence of values to insert</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        protected async Task InsertFirstAsync(IEnumerable<T> values, CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new LinkedManifest();

            var oldFirstIndex = manifest.First;
            string oldFirstKey = null;
            LinkedNode<T> oldFirstNode = null;
            if (oldFirstIndex.HasValue)
            {
                oldFirstKey = IndexToKey(oldFirstIndex.Value);
                oldFirstNode = await StateManager.GetStateAsync<LinkedNode<T>>(oldFirstKey, cancellationToken);
            }

            long? previousIndex = null;
            string previousKey = null;
            LinkedNode<T> previousNode = null;

            foreach (var value in values)
            {
                var newIndex = NextIndex(manifest);
                var newKey = IndexToKey(newIndex);
                var newNode = new LinkedNode<T>
                {
                    Value = value,
                    Previous = previousIndex,
                    Next = oldFirstIndex
                };

                if (oldFirstNode != null)
                {
                    oldFirstNode.Previous = newIndex;
                }

                if (previousNode == null)
                {
                    manifest.First = newIndex;
                }
                else
                {
                    previousNode.Next = newIndex;
                    await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
                }

                manifest.Count++;

                previousIndex = newIndex;
                previousKey = newKey;
                previousNode = newNode;
            }

            if (previousNode != null)
            {
                await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }

            if (oldFirstNode != null)
            {
                await StateManager.SetStateAsync(oldFirstKey, oldFirstNode, cancellationToken);
            }

            manifest.Last = manifest.Last ?? previousIndex;

            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <summary>
        /// Insert a sequence of values after the current Nth node of the collection
        /// </summary>
        /// <param name="values">sequence of values to insert</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        protected async Task InsertLastAsync(IEnumerable<T> values, CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new LinkedManifest();

            string previousKey = null;
            LinkedNode<T> previousNode = null;
            if (manifest.Last.HasValue)
            {
                previousKey = IndexToKey(manifest.Last.Value);
                previousNode = await StateManager.GetStateAsync<LinkedNode<T>>(previousKey, cancellationToken);
            }

            foreach (var value in values)
            {
                var newIndex = NextIndex(manifest);
                var newKey = IndexToKey(newIndex);
                var newNode = new LinkedNode<T>
                {
                    Value = value,
                    Previous = previousNode?.Next
                };

                if (previousNode != null)
                {
                    previousNode.Next = newIndex;
                    await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
                }

                manifest.Count++;
                manifest.Last = newIndex;

                if (!manifest.First.HasValue)
                {
                    manifest.First = newIndex;
                }

                previousKey = newKey;
                previousNode = newNode;
            }

            if (previousNode != null)
            {
                await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }

            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <summary>
        /// Insert a sequence of values before the current <paramref name="position"/>th node of the collection
        /// </summary>
        /// <param name="position">index of the node to insert values before</param>
        /// <param name="values">sequence of values to insert</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        protected async Task InsertAtAsync(long position, IEnumerable<T> values, CancellationToken cancellationToken)
        {
            var manifestResult = await StateManager.TryGetStateAsync<LinkedManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new LinkedManifest();

            if (position < 0 || position > manifest.Count)
            {
                throw new IndexOutOfRangeException($"Index {position} must be in range [0, {manifest.Count}]");
            }

            long i = 0;
            string previousKey = null;
            LinkedNode<T> previousNode = null;
            var (insertBeforeKey, insertBeforeNode) = await FindNodeAsync(manifest.First, (key, node) =>
            {
                var success = i++ == position;
                if (!success)
                {
                    previousKey = key;
                    previousNode = node;
                }

                return success;
            }, cancellationToken);

            foreach (var value in values)
            {
                var newIndex = NextIndex(manifest);
                var newKey = IndexToKey(newIndex);
                var newNode = new LinkedNode<T>
                {
                    Value = value,
                    Next = previousNode?.Next ?? manifest.First,
                    Previous = insertBeforeNode?.Previous ?? manifest.Last
                };

                await StateManager.AddStateAsync(newKey, newNode, cancellationToken);

                manifest.Count++;

                if (previousNode == null)
                {
                    manifest.First = newIndex;
                }
                else
                {
                    previousNode.Next = newIndex;
                    await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
                }

                if (insertBeforeNode == null)
                {
                    manifest.Last = newIndex;
                }
                else
                {
                    insertBeforeNode.Previous = newIndex;
                }

                previousKey = newKey;
                previousNode = newNode;
            }

            if (insertBeforeNode != null)
            {
                await StateManager.SetStateAsync(insertBeforeKey, insertBeforeNode, cancellationToken);
            }

            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        protected async Task RemoveCoreAsync(LinkedManifest manifest, string previousKey, LinkedNode<T> previousNode, string removeKey, LinkedNode<T> removeNode, CancellationToken cancellationToken)
        {
            if (previousNode != null)
            {
                previousNode.Next = removeNode.Next;
                await StateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }
            else
            {
                manifest.First = removeNode.Next;
            }

            if (removeNode.Next.HasValue)
            {
                var nextKey = IndexToKey(removeNode.Next.Value);
                var nextNode = await StateManager.GetStateAsync<LinkedNode<T>>(nextKey, cancellationToken);
                nextNode.Previous = removeNode.Previous;
                await StateManager.SetStateAsync(nextKey, nextNode, cancellationToken);
            }
            else
            {
                manifest.Last = removeNode.Previous;
            }

            await StateManager.RemoveStateAsync(removeKey, cancellationToken);

            manifest.Count--;
            await StateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        protected static long NextIndex(LinkedManifest manifest)
        {
            return manifest.Next++;
        }

        protected string IndexToKey(long index)
        {
            return string.Format(IndexKeyFormat, Name, index);
        }

        protected async Task<(string Key, LinkedNode<T> Node)> FindNodeAsync(long? startIndex, Func<string, LinkedNode<T>, bool> predicate, CancellationToken cancellationToken)
        {
            string currentKey = null;
            LinkedNode<T> current = null;
            var found = false;
            for (var currentIndex = startIndex; !found && currentIndex.HasValue; currentIndex = current.Next)
            {
                currentKey = IndexToKey(currentIndex.Value);
                current = await StateManager.GetStateAsync<LinkedNode<T>>(currentKey, cancellationToken);
                found = predicate(currentKey, current);
            }

            return (Key: currentKey, Node: found ? current : null);
        }

        private class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly LinkedCollectionStateBase<T> _source;
            private LinkedNode<T> _currentNode;

            internal AsyncEnumerator(LinkedCollectionStateBase<T> source)
            {
                _source = source;
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public T Current => _currentNode != null ? _currentNode.Value : default(T);

            /// <inheritdoc />
            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                var stateManager = _source.StateManager;
                if (_currentNode == null)
                {
                    var manifestResult = await stateManager.TryGetStateAsync<LinkedManifest>(_source.Name, cancellationToken);
                    if (!manifestResult.HasValue)
                    {
                        return false;
                    }

                    var manifest = manifestResult.Value;
                    if (!manifest.First.HasValue)
                    {
                        return false;
                    }

                    _currentNode = await stateManager.GetStateAsync<LinkedNode<T>>(_source.IndexToKey(manifest.First.Value), cancellationToken);
                }
                else
                {
                    if (_currentNode.Next.HasValue)
                    {
                        _currentNode = await stateManager.GetStateAsync<LinkedNode<T>>(_source.IndexToKey(_currentNode.Next.Value), cancellationToken);
                    }
                    else
                    {
                        _currentNode = null;
                    }
                }

                return _currentNode != null;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _currentNode = null;
            }
        }
    }
}
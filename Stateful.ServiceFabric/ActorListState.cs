namespace Stateful.ServiceFabric
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Stateful.ServiceFabric.Internals;

    public class ActorListState<T> : IListState<T>
    {
        private const string IndexKeyFormat = "{0}:{1:X}";

        private readonly IActorStateManager _stateManager;

        public string Name { get; }

        public ActorListState(IActorStateManager stateManager, string name)
        {
            _stateManager = stateManager;
            Name = name;
        }

        /// <inheritdoc />
        public Task<bool> HasStateAsync(CancellationToken cancellationToken)
        {
            return _stateManager.ContainsStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteStateAsync(CancellationToken cancellationToken)
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return;
            }

            var manifest = manifestResult.Value;
            ListNode<T> currentNode;
            for (var internalIndex = manifest.First; internalIndex.HasValue; internalIndex = currentNode.Next)
            {
                var key = IndexToKey(internalIndex.Value);
                currentNode = await _stateManager.GetStateAsync<ListNode<T>>(key, cancellationToken);
                await _stateManager.RemoveStateAsync(key, cancellationToken);
            }

            await _stateManager.RemoveStateAsync(Name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            var manifest = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            return manifest.HasValue ? manifest.Value.Count : 0;
        }

        /// <inheritdoc />
        public async Task AddAsync(T value, CancellationToken cancellationToken)
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new ListManifest();
            manifest.Count++;

            var newIndex = NextIndex(manifest);
            var newKey = IndexToKey(newIndex);
            var newNode = new ListNode<T>
            {
                Value = value,
                Previous = manifest.Last
            };
            manifest.Last = newIndex;
            if (!manifest.First.HasValue)
            {
                manifest.First = newIndex;
            }

            if (newNode.Previous.HasValue)
            {
                var lastKey = IndexToKey(newNode.Previous.Value);
                var lastNode = await _stateManager.GetStateAsync<ListNode<T>>(lastKey, cancellationToken);
                lastNode.Next = newIndex;
                await _stateManager.SetStateAsync(lastKey, lastNode, cancellationToken);
            }

            await _stateManager.AddStateAsync(newKey, newNode, cancellationToken);
            await _stateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddRangeAsync(IEnumerable<T> values, CancellationToken cancellationToken)
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new ListManifest();

            ListNode<T> previousNode = null;
            string previousKey = null;

            foreach (var value in values)
            {
                if (previousNode == null && manifest.Last.HasValue)
                {
                    previousKey = IndexToKey(manifest.Last.Value);
                    previousNode = await _stateManager.GetStateAsync<ListNode<T>>(previousKey, cancellationToken);
                }

                var newIndex = NextIndex(manifest);
                var newKey = IndexToKey(newIndex);
                var newNode = new ListNode<T>
                {
                    Value = value,
                    Previous = manifest.Last
                };

                manifest.Count++;
                manifest.Last = newIndex;
                if (!manifest.First.HasValue)
                {
                    manifest.First = newIndex;
                }

                if (previousNode != null)
                {
                    previousNode.Next = newIndex;
                    await _stateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
                }

                previousKey = newKey;
                previousNode = newNode;
            }

            if (previousNode != null)
            {
                await _stateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }
            await _stateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<T>();
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return result;
            }

            var manifest = manifestResult.Value;
            ListNode<T> currentNode;
            for (var internalIndex = manifest.First; internalIndex.HasValue; internalIndex = currentNode.Next)
            {
                currentNode = await _stateManager.GetStateAsync<ListNode<T>>(IndexToKey(internalIndex.Value), cancellationToken);
                result.Add(currentNode.Value);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryGetAsync(long index, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            long i = 0;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => i++ == index, cancellationToken);

            return foundNode != null ? new ConditionalValue<T>(foundNode.Value) : new ConditionalValue<T>();
        }

        /// <inheritdoc />
        public async Task<bool> ContainsAsync(T value, IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return false;
            }

            var manifest = manifestResult.Value;
            equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => equalityComparer.Equals(value, node.Value), cancellationToken);

            return foundNode != null;
        }

        /// <inheritdoc />
        public async Task<ConditionalValue<T>> TryFindAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return new ConditionalValue<T>();
            }

            var manifest = manifestResult.Value;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) => match(node.Value), cancellationToken);

            return foundNode != null ? new ConditionalValue<T>(foundNode.Value) : new ConditionalValue<T>();
        }

        /// <inheritdoc />
        public async Task InsertAsync(long index, T value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            var manifest = manifestResult.HasValue ? manifestResult.Value : new ListManifest();

            long i = 0;
            string previousKey = null;
            ListNode<T> previousNode = null;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) =>
            {
                var success = i++ == index;
                if (!success)
                {
                    previousKey = key;
                    previousNode = node;
                }

                return success;
            }, cancellationToken);

            var newIndex = NextIndex(manifest);
            var newKey = IndexToKey(newIndex);
            var newNode = new ListNode<T>
            {
                Value = value,
                Next = previousNode?.Next ?? manifest.First,
                Previous = foundNode?.Previous ?? manifest.Last
            };

            await _stateManager.AddStateAsync(newKey, newNode, cancellationToken);

            if (previousNode == null)
            {
                manifest.First = newIndex;
            }
            else
            {
                previousNode.Next = newIndex;
                await _stateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }

            if (foundNode == null)
            {
                manifest.Last = newIndex;
            }
            else
            {
                foundNode.Previous = newIndex;
                await _stateManager.SetStateAsync(foundKey, foundNode, cancellationToken);
            }

            manifest.Count++;
            await _stateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAtAsync(long index, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return;
            }

            var manifest = manifestResult.Value;
            long i = 0;
            string previousKey = null;
            ListNode<T> previousNode = null;
            var (foundKey, foundNode) = await FindNodeAsync(manifest.First, (key, node) =>
            {
                var success = i++ == index;
                if (!success)
                {
                    previousKey = key;
                    previousNode = node;
                }

                return success;
            }, cancellationToken);

            if (foundNode != null)
            {
                await RemoveInternalAsync(manifest, previousKey, previousNode, foundKey, foundNode, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken))
        {
            var manifestResult = await _stateManager.TryGetStateAsync<ListManifest>(Name, cancellationToken);
            if (!manifestResult.HasValue)
            {
                return;
            }

            var manifest = manifestResult.Value;
            string previousKey = null;
            ListNode<T> previousNode = null;
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
                await RemoveInternalAsync(manifest, previousKey, previousNode, foundKey, foundNode, cancellationToken);
            }
        }

        private async Task RemoveInternalAsync(ListManifest manifest, string previousKey, ListNode<T> previousNode, string removeKey, ListNode<T> removeNode, CancellationToken cancellationToken)
        {
            if (previousNode != null)
            {
                previousNode.Next = removeNode.Next;
                await _stateManager.SetStateAsync(previousKey, previousNode, cancellationToken);
            }
            else
            {
                manifest.First = removeNode.Next;
            }

            if (removeNode.Next.HasValue)
            {
                var nextKey = IndexToKey(removeNode.Next.Value);
                var nextNode = await _stateManager.GetStateAsync<ListNode<T>>(nextKey, cancellationToken);
                nextNode.Previous = removeNode.Previous;
                await _stateManager.SetStateAsync(nextKey, nextNode, cancellationToken);
            }
            else
            {
                manifest.Last = removeNode.Previous;
            }

            await _stateManager.RemoveStateAsync(removeKey, cancellationToken);

            manifest.Count--;
            await _stateManager.SetStateAsync(Name, manifest, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new AsyncEnumerator(this);
        }

        private static long NextIndex(ListManifest manifest)
        {
            return manifest.Next++;
        }

        private string IndexToKey(long index)
        {
            return string.Format(IndexKeyFormat, Name, index);
        }

        private async Task<(string Key, ListNode<T> Node)> FindNodeAsync(long? startIndex, Func<string, ListNode<T>, bool> predicate, CancellationToken cancellationToken)
        {
            string currentKey = null;
            ListNode<T> current = null;
            var found = false;
            for (var currentIndex = startIndex; !found && currentIndex.HasValue; currentIndex = current.Next)
            {
                currentKey = IndexToKey(currentIndex.Value);
                current = await _stateManager.GetStateAsync<ListNode<T>>(currentKey, cancellationToken);
                found = predicate(currentKey, current);
            }

            return (Key: currentKey, Node: current);
        }

        public class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly ActorListState<T> _list;
            private ListNode<T> _currentNode;

            internal AsyncEnumerator(ActorListState<T> list)
            {
                _list = list;
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public T Current => _currentNode != null ? _currentNode.Value : default(T);

            /// <inheritdoc />
            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                var stateManager = _list._stateManager;
                if (_currentNode == null)
                {
                    var manifestResult = await stateManager.TryGetStateAsync<ListManifest>(_list.Name, cancellationToken);
                    if (!manifestResult.HasValue)
                    {
                        return false;
                    }

                    var manifest = manifestResult.Value;
                    if (!manifest.First.HasValue)
                    {
                        return false;
                    }

                    _currentNode = await stateManager.GetStateAsync<ListNode<T>>(string.Format(IndexKeyFormat, _list.Name, manifest.First.Value), cancellationToken);
                }
                else
                {
                    if (_currentNode.Next.HasValue)
                    {
                        _currentNode = await stateManager.GetStateAsync<ListNode<T>>(string.Format(IndexKeyFormat, _list.Name, _currentNode.Next.Value), cancellationToken);
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
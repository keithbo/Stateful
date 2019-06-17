namespace Stateful
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// IUnit is ultimately a Unit-Of-Work pattern to supply state-access instances derived from <see cref="IState"/>
    /// and abstraction to commit/abort state modifications.
    /// State is to be tied to the lifetime of the IUnit and once committed/aborted the state is considered invalid to use further.
    /// </summary>
    /// <remarks>
    /// Implementation of state is decided at configuration time via <see cref="Stateful.Configuration.IStateConfigurator"/>.
    /// Abort/Commit of state is decided by each implementation and may not actually be used.
    /// </remarks>
    public interface IUnit : IDisposable
    {
        /// <summary>
        /// Get a state of the target type derived from <see cref="IState"/>.
        /// </summary>
        /// <typeparam name="TState">Type of state</typeparam>
        /// <param name="key"><see cref="IStateKey"/> for configured state</param>
        /// <returns><typeparamref name="TState"/> instance</returns>
        TState Get<TState>(IStateKey key) where TState : IState;

        /// <summary>
        /// Cancel any active state operations.
        /// </summary>
        void Abort();

        /// <summary>
        /// Submit all active state operations.
        /// </summary>
        Task CommitAsync();
    }
}

namespace Stateful.Configuration
{
    /// <summary>
    /// Configure the base details of a specific state
    /// </summary>
    public interface IStateConfigurator
    {
        /// <summary>
        /// Set the <see cref="IStateKey"/> used to identify this state
        /// </summary>
        IStateKey Key { set; }
    }
}
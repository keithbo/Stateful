namespace Stateful.Configuration
{
    /// <summary>
    /// Base configuration interface for generating <see cref="IStateFactory"/> implementations
    /// </summary>
    public interface IStateFactoryConfiguration
    {
        /// <summary>
        /// Construct the configured State Factory
        /// </summary>
        /// <returns><see cref="IStateFactory"/></returns>
        IStateFactory Build();
    }
}
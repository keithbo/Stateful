namespace Stateful.Configuration
{
    /// <summary>
    /// Configure the details of an array typed state
    /// </summary>
    public interface IArrayStateConfigurator : IStateConfigurator
    {
        /// <summary>
        /// Set the length of this array state
        /// </summary>
        long Length { set; }
    }
}
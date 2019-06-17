namespace Stateful
{
    /// <summary>
    /// Interface to wrap construction of <see cref="IUnit"/> that supplies State instances
    /// </summary>
    public interface IStateFactory
    {
        /// <summary>
        /// Create an <see cref="IUnit"/>
        /// </summary>
        /// <returns><see cref="IUnit"/></returns>
        IUnit CreateTransaction();
    }
}
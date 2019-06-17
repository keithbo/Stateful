namespace Stateful
{
    using Stateful.Configuration;

    public static class State
    {
        /// <summary>
        /// Stateful selector, launch pad for library-specific implementations.
        /// </summary>
        public static IStateFactorySelector Factory { get; } = new StateFactorySelector();
    }
}
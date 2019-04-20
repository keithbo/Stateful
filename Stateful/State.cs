namespace Stateful
{
    using Stateful.Configuration;

    public static class State
    {
        public static IStateFactorySelector Factory { get; } = new StateFactorySelector();
    }
}
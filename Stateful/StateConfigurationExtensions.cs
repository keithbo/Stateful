namespace Stateful
{
    using System;

    public static class StateConfigurationExtensions
    {
        public static IStateConfiguration Object<TValue>(this IStateConfiguration configuration, string name)
        {
            return configuration.Object<TValue>(new StateKey(name));
        }

        public static IStateConfiguration List<TValue>(this IStateConfiguration configuration, string name)
        {
            return configuration.List<TValue>(new StateKey(name));
        }

        public static IStateConfiguration Dictionary<TKey, TValue>(this IStateConfiguration configuration, string name)
            where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            return configuration.Dictionary<TKey, TValue>(new StateKey(name));
        }

        public static IStateConfiguration Array<TValue>(this IStateConfiguration configuration, string name, long length)
        {
            return configuration.Array<TValue>(new StateKey(name), length);
        }

        public static IStateConfiguration Queue<TValue>(this IStateConfiguration configuration, string name)
        {
            return configuration.Queue<TValue>(new StateKey(name));
        }

        public static IStateConfiguration Stack<TValue>(this IStateConfiguration configuration, string name)
        {
            return configuration.Stack<TValue>(new StateKey(name));
        }
    }
}
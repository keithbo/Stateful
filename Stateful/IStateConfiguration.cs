namespace Stateful
{
    public interface IStateConfiguration
    {
        IStateFactory Build();
    }
}
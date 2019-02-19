namespace Stateful
{
    public interface IStateFactory
    {
        IUnit CreateTransaction();
    }
}
namespace Stateful.Configuration
{
    public interface IArrayStateConfigurator : IStateConfigurator
    {
        long Length { set; }
    }
}
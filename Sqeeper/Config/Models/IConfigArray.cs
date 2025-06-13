namespace Sqeeper.Config.Models
{
    public interface IConfigArray
    {
        public AppConfig Get(int index);
        public int Length { get; }
    }
}
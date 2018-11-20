namespace Core
{
    public interface IBaseResponstory
    {
        string responstory();
    }
    public interface IBaseResponstory<T> : IBaseResponstory
    {
    }
}
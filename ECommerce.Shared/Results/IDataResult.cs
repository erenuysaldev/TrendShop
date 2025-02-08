namespace ECommerce.Shared.Results
{
    public interface IDataResult<T> : IResult
    {
        T Data { get; }
    }
} 
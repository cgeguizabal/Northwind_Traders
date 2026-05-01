namespace NorthwindTraders.Domain.Common;

public class Result<T>
{
    // private constructor — C# language
    // forces callers to use Success() or Failure() methods below
    // nobody can do: new Result<T>() directly
    private Result() { }

    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }

    // static factory method — C# pattern
    // creates a successful result carrying a value
    public static Result<T> Success(T value) => new Result<T>
    {
        IsSuccess = true,
        Value = value
    };

    // creates a failed result carrying an error message
    public static Result<T> Failure(string error) => new Result<T>
    {
        IsSuccess = false,
        Error = error
    };
}
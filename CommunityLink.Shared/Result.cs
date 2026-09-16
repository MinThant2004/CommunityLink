using System;

namespace CommunityLink.Shared;

public enum ResultStatus
{
    Success,
    ValidationError,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    RateLimited,
    SystemError
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;
    public bool IsFailure => IsError;
    public string Message { get; }
    public string Error => Message;
    public ResultStatus Status { get; }

    public Result(bool isSuccess, string message, ResultStatus status)
    {
        if (!isSuccess && string.IsNullOrEmpty(message))
            throw new InvalidOperationException("Failure result must have an error message.");

        IsSuccess = isSuccess;
        Message = message;
        Status = status;
    }

    public static Result Success(string message = "Success") => new(true, message, ResultStatus.Success);
    public static Result Failure(string error, ResultStatus status = ResultStatus.ValidationError) => new(false, error, status);
}

public class Result<T> : Result
{
    public T? Data { get; }
    public T? Value => Data;

    public Result(bool isSuccess, T? data, string message, ResultStatus status) : base(isSuccess, message, status)
    {
        Data = data;
    }

    public static Result<T> Success(T value, string message = "Success") => new(true, value, message, ResultStatus.Success);
    public static new Result<T> Failure(string error, ResultStatus status = ResultStatus.ValidationError) => new(false, default, error, status);
}

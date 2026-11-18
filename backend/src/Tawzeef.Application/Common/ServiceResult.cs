namespace Tawzeef.Application.Common;

public enum ResultError
{
    None,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
}

public class ServiceResult
{
    public bool IsSuccess { get; init; }
    public ResultError Error { get; init; } = ResultError.None;
    public List<string> ErrorMessages { get; init; } = [];

    public static ServiceResult Success() => new() { IsSuccess = true };
    public static ServiceResult Invalid(params string[] m) => Fail(ResultError.Validation, m);
    public static ServiceResult Invalid(IEnumerable<string> m) => Fail(ResultError.Validation, m);
    public static ServiceResult NotFound(params string[] m) => Fail(ResultError.NotFound, m);
    public static ServiceResult Conflict(params string[] m) => Fail(ResultError.Conflict, m);
    public static ServiceResult Forbidden(params string[] m) => Fail(ResultError.Forbidden, m);

    protected static ServiceResult Fail(ResultError e, IEnumerable<string> m) =>
        new() { IsSuccess = false, Error = e, ErrorMessages = m.ToList() };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public new static ServiceResult<T> Invalid(params string[] m) => FailWithData(ResultError.Validation, m);
    public new static ServiceResult<T> Invalid(IEnumerable<string> m) => FailWithData(ResultError.Validation, m);
    public new static ServiceResult<T> NotFound(params string[] m) => FailWithData(ResultError.NotFound, m);
    public new static ServiceResult<T> Conflict(params string[] m) => FailWithData(ResultError.Conflict, m);
    public new static ServiceResult<T> Forbidden(params string[] m) => FailWithData(ResultError.Forbidden, m);

    private static ServiceResult<T> FailWithData(ResultError e, IEnumerable<string> m) =>
        new() { IsSuccess = false, Error = e, ErrorMessages = m.ToList() };
}

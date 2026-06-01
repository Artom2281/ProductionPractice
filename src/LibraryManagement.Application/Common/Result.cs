namespace LibraryManagement.Application.Common;

// Результат операции бизнес-слоя без использования исключений для ожидаемых ошибок
// (например, "логин не найден", "книга недоступна", "ошибка валидации").
// Технические сбои (потеря БД, неверная конфигурация) идут как обычные исключения.
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public IReadOnlyList<string> ValidationErrors { get; }

    protected Result(bool isSuccess, string? error, IReadOnlyList<string>? validationErrors)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? Array.Empty<string>();
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error) => new(false, error, null);
    public static Result ValidationFailure(IReadOnlyList<string> errors) => new(false, "Ошибка валидации.", errors);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool isSuccess, string? error, IReadOnlyList<string>? validationErrors)
        : base(isSuccess, error, validationErrors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null, null);
    public new static Result<T> Failure(string error) => new(default, false, error, null);
    public new static Result<T> ValidationFailure(IReadOnlyList<string> errors) => new(default, false, "Ошибка валидации.", errors);
}

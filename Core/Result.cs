﻿namespace Abd.Shared.Core;

public class Result:IResult
{
    public bool IsSuccess { get; }
    public object? Value { get; }
    public IEnumerable<IError> Errors { get; }  = Enumerable.Empty<IError>();
    public string StatusCode => IsSuccess ? "200" : Errors.FirstOrDefault()?.Code??"400";
    public string Message => Errors.FirstOrDefault()?.Message??"An error occured";

    protected Result(object? value)
    {
        IsSuccess = true;
        Value = value;
    }

    protected Result(IEnumerable<IError>? errors)
    {
        IsSuccess = false;
        var errorLst = errors?.ToList();
        if(errorLst is {} && errorLst.Any())
            Errors = errorLst;
    }

    public static IResult<T> Parse<T>(IOperationResult request) where T : class
    {
        return request
            .IsSuccessResult() ? 
            
            Create(request.Data?
                .GetType()
                .GetProperties()
                .FirstOrDefault()?
                .GetValue(request.Data)?
                .Adapt<T>()) : 
            
            Fail<T>(request.Errors.Adapt<IError>());
    }
    public static IResult Parse(IOperationResult result) 
        => result.IsSuccessResult() ? 
            Create() : 
            Fail(result.Errors.Adapt<IEnumerable<Error>>());

    public static IResult<T> Create<T>(T? data) where T : class 
        => new Result<T>(data);

    public static IObservable<IResult<T>> Create0<T>(T? data) where T : class 
        => Observable.Return<IResult<T>>(new Result<T>(data));

    // errors
    public static IResult<T> Fail<T>(IEnumerable<IError> errors) where T : class
    {
        return new Result<T>(errors);
    }
    // single error
    public static IResult<T> Fail<T>(IError? error) where T : class
    {
        return new Result<T>(error);
    }
    public static IResult<T> Fail<T>(string? message = null) where T : class
    {
        return new Result<T>(message);
    }
    public static IResult Create(object? value = null)
    {
        return new Result(value);
    }
    public static IResult Fail(IError error)
    {
        return new Result(error);
    }
    public static IResult Fail(string error)
    {
        return new Result(error);
    }

    public static IResult Fail()
    {
        return new Result(Array.Empty<IError>());
    }
    public static IResult Fail(IEnumerable<IError> errors)
    {
        return new Result(errors);
    }
    public static IResult Fail(IEnumerable<Error> errors)
    {
        return new Result(errors);
    }
    
}
public class Result<T>:Result,IResult<T> where T : class
{
    public new T? Value => (T?)base.Value;
    public Result(T? value):base(value){ }
    public Result(IEnumerable<IError> errors):base(errors){ }
    
    public Result(IError? error)
        : base(new []{ error ?? new Error("Something went wrong. Error is not specified") }){ }
    
    public Result(string? error = null)
        :base(new []{new Error(error??"Something went wrong. Error is not specified") }){ }
}
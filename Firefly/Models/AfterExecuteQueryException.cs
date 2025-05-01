using System;

namespace Firefly.Models;

public class AfterExecuteQueryException : Exception
{
    public AfterExecuteQueryException()
    { }

    public AfterExecuteQueryException(string? message) : base(message)
    { }

    public AfterExecuteQueryException(string? message, Exception? innerException) : base(message, innerException)
    { }
}

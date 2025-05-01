using System;

namespace Firefly.Models;

public class CccfScraperLog
{
    private static int _index = 0;

    public Exception? Error { get; set; }

    public bool HasError { get; }

    public int Index { get; }

    public string Message { get; }

    public int PageNumber { get; }

    public DateTime Timestamp { get; set; }

    public CccfScraperLog(int pageNumber, Exception error) : this(pageNumber, $"<{error.GetType()}> {error.Message}")
    {
        HasError = error is not null;
    }

    public CccfScraperLog(int pageNumber, string message)
    {
        Timestamp = DateTime.Now;
        Index = _index++;
        PageNumber = pageNumber;
        Message = message;
    }

    public static void ResetIndex()
    {
        _index = 0;
    }

    public override string ToString()
    {
        return $"#{Index} | [{Timestamp:MM/dd, hh:mm:ss.fff}] | {PageNumber} | {Message}";
    }
}

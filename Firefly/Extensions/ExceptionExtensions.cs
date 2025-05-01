using System;
using System.Net.Http;
using System.Text;

using Firefly.Models.Responses;

namespace Firefly.Extensions;

public static class ExceptionExtensions
{
    public static string GetHttpStatusDescription(this HttpRequestException exception)
    {
        string desc;

        if (exception.StatusCode is not null && Enum.IsDefined(typeof(FriendlyHttpStatusCode), (int)exception.StatusCode))
        {
            var status = (FriendlyHttpStatusCode)exception.StatusCode;
            desc = status.GetDescription();
        }
        else
        {
            desc = exception.Message;
        }

        return desc;
    }

    public static string Pretty(this Exception exception)
    {
        var sb = new StringBuilder($"{exception.Message}\n");
        var innerEx = exception.InnerException;

        int indent = 0;

        while (innerEx is not null)
        {
            sb.AppendLine($"{new string(' ', indent)}└── {innerEx.Message}");
            innerEx = innerEx.InnerException;
            indent += 4;
        }

        return sb.ToString();
    }
}

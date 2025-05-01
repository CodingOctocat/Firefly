using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Firefly.Models;

public class SerializableException
{
    public class ExceptionWrapper
    {
        public Dictionary<string, string?> Datas = [];

        public int Depth { get; } = 0;

        public string? HelpLink { get; }

        public int HResult { get; }

        public ExceptionWrapper? InnerException { get; }

        public string Message { get; }

        public string? Source { get; }

        public string? StackTrace { get; }

        public string? TargetSite { get; }

        public Type Type { get; }

        public ExceptionWrapper(Exception exception)
        {
            Type = exception.GetType();
            Source = exception.Source;
            Message = exception.Message;
            TargetSite = exception.TargetSite?.ToString();
            StackTrace = exception.StackTrace;
            HResult = exception.HResult;
            HelpLink = exception.HelpLink;

            foreach (DictionaryEntry data in exception.Data)
            {
                Datas.Add($"{data.Key}", data.Value?.ToString());
            }

            if (exception?.InnerException is not null)
            {
                InnerException = new(exception.InnerException, Depth++);
            }
        }

        private ExceptionWrapper(Exception exception, int depth) : this(exception)
        {
            Depth = depth;
        }

        public override string? ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(Type)}: {Type.ToString() ?? "null"}");
            sb.AppendLine($"{nameof(Source)}: {Source ?? "null"}");
            sb.AppendLine($"{nameof(Message)}: {Message ?? "null"}");
            sb.AppendLine($"{nameof(TargetSite)}: {TargetSite ?? "null"}");
            sb.AppendLine($"{nameof(StackTrace)}:{Environment.NewLine}{StackTrace ?? "null"}");
            sb.AppendLine($"{nameof(HResult)}: {HResult}");
            sb.AppendLine($"{nameof(HelpLink)}: {HelpLink ?? "null"}");

            if (InnerException is null)
            {
                sb.AppendLine($"{nameof(InnerException)}: null");
            }
            else
            {
                sb.AppendLine($"{Environment.NewLine}--- Inner Exception (Level {Depth}) ---");
                sb.AppendLine(InnerException.ToString());
            }

            return sb.ToString();
        }
    }

    public static string AppVersion => Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Unknow";

    public string ClrVersion { get; }

    public ExceptionWrapper Exception { get; }

    public string Handler { get; }

    public string OSVersion { get; }

    public int ThreadId { get; }

    public string ThreadName { get; }

    public DateTime Timestamp { get; }

    public SerializableException(Exception exception, string handler)
    {
        Timestamp = DateTime.Now;
        Exception = new ExceptionWrapper(exception);
        Handler = handler;

        OSVersion = Environment.OSVersion.VersionString;
        ClrVersion = Environment.Version.ToString();
        ThreadName = Thread.CurrentThread.Name ?? "N/A";
        ThreadId = Environment.CurrentManagedThreadId;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{nameof(Timestamp)}: {Timestamp.ToString()}");
        sb.AppendLine($"{nameof(OSVersion)}: {OSVersion}");
        sb.AppendLine($"{nameof(ClrVersion)}: {ClrVersion}");
        sb.AppendLine($"{nameof(ThreadName)}: {ThreadName}({ThreadId})");
        sb.AppendLine(new string('-', 40));
        sb.AppendLine($"{nameof(Handler)}: {Handler}");
        sb.AppendLine(new string('-', 40));
        sb.AppendLine(Exception.ToString());

        return sb.ToString();
    }
}

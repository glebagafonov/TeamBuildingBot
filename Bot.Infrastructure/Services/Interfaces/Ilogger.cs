using System;

namespace Bot.Infrastructure.Services.Interfaces
{
    public interface ILogger
    {
        bool IsDebugEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }

        void Debug(Exception exception, string format, params object[] args);
        void Error(Exception exception, string format, params object[] args);
        void Fatal(Exception exception, string format, params object[] args);
        void Info(Exception exception, string format, params object[] args);
        void Trace(Exception exception, string format, params object[] args);
        void Warn(Exception exception, string format, params object[] args);

        void Debug(Exception exception);
        void Error(Exception exception);
        void Fatal(Exception exception);
        void Info(Exception exception);
        void Trace(Exception exception);
        void Warn(Exception exception);

        void Debug(string format, params object[] args);
        void Error(string format, params object[] args);
        void Fatal(string format, params object[] args);
        void Info(string format, params object[] args);
        void Trace(string format, params object[] args);
        void Warn(string format, params object[] args);
    }
}
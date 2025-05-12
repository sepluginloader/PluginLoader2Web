using System.Reflection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace PluginLoader2Web
{
    public static class Log
    {
        private const string LogFormat = "{@t:HH:mm:ss} [{@l:u3}] [{ThreadID}]{CustomPrefix} {@m} {@x}\n";

        private static Logger log = null!;
        private static readonly List<Logger> customLoggers = new List<Logger>();

        public static void Init(string fileName)
        {
            ExpressionTemplate format = new ExpressionTemplate(LogFormat);
            LoggerConfiguration logConfig = new LoggerConfiguration()
                .Enrich.With(new ThreadIDEnricher())
#if DEBUG
                .WriteTo.Debug(format)
#endif
                .WriteTo.Console(new ExpressionTemplate(LogFormat, theme: TemplateTheme.Literate))
                .WriteTo.File(format, fileName, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);

            log = logConfig.CreateLogger();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            AssemblyName name = Assembly.GetExecutingAssembly().GetName();
            Info($"Starting {name.Name} - v{name.Version?.ToString(2)}");
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            try
            {
                if (log != null)
                    Info("Process exit");
            }
            catch { }
        }

        public static void Link(IHostBuilder hostBuilder, string name)
        {
            hostBuilder.UseSerilog(CreateCustomLogger(name), false);
        }

        public static void Link(ILoggingBuilder logBuilder, string name)
        {
            logBuilder.AddSerilog(CreateCustomLogger(name), false);
        }

        public static Logger CreateCustomLogger(string name)
        {
            LoggerConfiguration config = new LoggerConfiguration()
                .Enrich.With(new PrefixEnricher(" [" + name + "]"))
                .WriteTo.Logger(log);
            Logger customLog = config.CreateLogger();
            customLoggers.Add(customLog);
            return customLog;
        }

        public static void Info(string msg)
        {
            Write(LogEventLevel.Information, msg);
        }

        public static void Error(string msg)
        {
            Write(LogEventLevel.Error, msg);
        }

        public static void Warn(string msg)
        {
            Write(LogEventLevel.Warning, msg);
        }

        internal static void Warn(string msg, Exception exception)
        {
            log.Warning(exception, msg);
        }


        public static void Error(Exception ex)
        {
            log.Error(ex, "An exception was thrown:");
        }

        public static void Error(string msg, Exception ex)
        {
            log.Error(ex, msg);
        }

        private static void Write(LogEventLevel level, string msg)
        {
            log.Write(level, msg);
        }

        public static void Close()
        {
            log.Dispose();
            foreach (Logger log in customLoggers)
                log.Dispose();
        }

        private class PrefixEnricher : ILogEventEnricher
        {
            private readonly string prefix;

            public PrefixEnricher(string prefix)
            {
                this.prefix = prefix;
            }

            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                  "CustomPrefix", prefix));
            }
        }

        private class ThreadIDEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                  "ThreadID", Environment.CurrentManagedThreadId.ToString()));
            }
        }
    }
}

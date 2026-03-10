namespace EdiEngine.Tests;

internal sealed class NUnitLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new NUnitLogger(categoryName);

    public void Dispose()
    {
    }

    private sealed class NUnitLogger : ILogger
    {
        private readonly string _categoryName;

        public NUnitLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            var prefix = logLevel switch
            {
                LogLevel.Critical => "crit",
                LogLevel.Error => "err",
                LogLevel.Warning => "warn",
                _ => "info"
            };

            try
            {
                TestContext.Progress.WriteLine($"{prefix}: {_categoryName}[{eventId.Id}]");
                TestContext.Progress.WriteLine($"      {message}");
                if (exception != null)
                {
                    TestContext.Progress.WriteLine($"      {exception}");
                }
            }
            catch
            {
                // TestContext может быть недоступен вне контекста теста
            }
        }
    }
}

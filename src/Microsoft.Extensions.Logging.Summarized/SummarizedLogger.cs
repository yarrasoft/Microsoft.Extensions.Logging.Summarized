using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.Summarized
{
    public class SummarizedLogger
    {
        private ILogger logger;
        // Instant startTime;
        DateTime startTime;
        Dictionary<string, SummarizedLogger> eventNameMap = new Dictionary<string, SummarizedLogger>();
        public SummarizedLogger(ILogger logger, LogLevel level, string eventName)
        {
            this.logger = logger;
            this.LogLevel = level;
            this.EventName = eventName;
            id = Guid.NewGuid().ToString();
        }

        public bool UseLogFrequencyEvents { get; protected internal set; }

        public LogLevel LogLevel { get; protected internal set; }

        public string EventName { get; protected internal set; }

        private string id;

        public TimeSpan LogFrequencyTime { get; protected internal set; }

        public int LogFrequencyEvents { get; protected internal set; }

        public int LoggedEventCount { get; protected internal set; }

        DateTime nextLogTime;

        object mapAddLock = new object();

        /// <summary>
        /// Logs an custom event and adds it to the list of loggers with the same profile as this logger
        /// </summary>
        /// <param name="eventName"></param>
        public async Task LogEventAsync(string eventName)
        {

            await Task.Run(async () =>
            {
                SummarizedLogger eventLogger;
                if (!eventNameMap.TryGetValue(eventName, out eventLogger))
                {
                    lock (mapAddLock)
                    {
                        if (!eventNameMap.TryGetValue(eventName, out eventLogger))
                        {
                            eventNameMap.Add(eventName, eventLogger = new SummarizedLogger(this.logger, this.LogLevel, eventName));
                            if (UseLogFrequencyEvents)
                            {
                                eventLogger.SetFrequency(this.LogFrequencyEvents);
                            }
                            else
                            {
                                eventLogger.SetFrequency(this.LogFrequencyTime);
                            }
                        }
                    }
                }

                await eventLogger.LogEventAsync();
            });
        }

        private object writeLock = new object();

        public async Task LogEventAsync()
        {
            await Task.Run(() =>
            {
                if (++LoggedEventCount == 1)
                {
                    //startTime = Instant.FromDateTimeUtc(DateTime.UtcNow);
                    startTime = DateTime.Now;

                    if (!UseLogFrequencyEvents)
                    {
                        nextLogTime = startTime;
                    }
                }

                if (UseLogFrequencyEvents && LoggedEventCount % LogFrequencyEvents == 0)
                {
                    lock (writeLock)
                    {
                        if (LoggedEventCount % LogFrequencyEvents == 0)
                        {
                            WriteEventLog();
                        }
                    }
                }
                else if (UseLogFrequencyEvents && LoggedEventCount == 1)
                {
                    lock (writeLock)
                    {
                        if (LoggedEventCount == 1)
                        {
                            WriteEventLog();
                        }
                    }
                }
                else if (!UseLogFrequencyEvents && DateTime.Now >= nextLogTime)
                {
                    lock (writeLock)
                    {
                        var now = DateTime.Now;
                        if (now >= nextLogTime)
                        {
                            var nextTime = now.Add(LogFrequencyTime);

                            logger.LogTrace("Writing time based logs. now: {now}, currentNext: {currentNext}, nextNext: {nextNext}, id: {id}", now, nextLogTime, nextTime, id);

                            nextLogTime = nextTime;
                            WriteEventLog();
                        }
                    }
                }
            });
        }

        private void WriteEventLog()
        {
            if ((UseLogFrequencyEvents && LogFrequencyEvents == 0) || (!UseLogFrequencyEvents && LogFrequencyTime.TotalSeconds <= 0))
            {
                throw new ArgumentException("Log event frequency or time not set");
            }

            Action<string, object[]> logAction;
            switch (LogLevel)
            {
                case LogLevel.Trace:
                    logAction = logger.LogTrace;
                    break;

                case LogLevel.Debug:
                    logAction = logger.LogDebug;
                    break;

                case LogLevel.Information:
                    logAction = logger.LogInformation;
                    break;

                case LogLevel.Warning:
                    logAction = logger.LogWarning;
                    break;

                case LogLevel.Error:
                    logAction = logger.LogError;
                    break;

                case LogLevel.Critical:
                    logAction = logger.LogCritical;
                    break;

                case LogLevel.None:
                default:
                    logAction = null;
                    break;
            }
            if (logAction != null)
            {
                logAction("{EventName} occurred {EventCount} times in the last {EventSeconds} seconds. ({id}}", new object[] { EventName, LoggedEventCount, DateTime.Now.Subtract(startTime).TotalSeconds.ToString("0"), id });
            }

        }
    }
}

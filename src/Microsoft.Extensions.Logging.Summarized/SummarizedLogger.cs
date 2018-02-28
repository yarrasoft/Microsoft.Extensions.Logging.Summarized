using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

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
        }

        public bool UseLogFrequencyEvents { get; protected internal set; }

        public LogLevel LogLevel { get; protected internal set; }

        public string EventName { get; protected internal set; }

        public TimeSpan LogFrequencyTime { get; protected internal set; }

        public int LogFrequencyEvents { get; protected internal set; }

        public int LoggedEventCount { get; protected internal set; }

        DateTime nextLogTime;

        object mapAddLock = new object();

        /// <summary>
        /// Logs an custom event and adds it to the list of loggers with the same profile as this logger
        /// </summary>
        /// <param name="eventName"></param>
        public void LogEvent(string eventName)
        {
            SummarizedLogger eventLogger;
            if (!eventNameMap.TryGetValue(eventName, out eventLogger))
            {
                lock (mapAddLock)
                {
                    if(!eventNameMap.TryGetValue(eventName, out eventLogger))
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

            eventLogger.LogEvent();
        }


        public void LogEvent()
        {
            if(++LoggedEventCount == 1)
            {
                //startTime = Instant.FromDateTimeUtc(DateTime.UtcNow);
                startTime = DateTime.Now;

                if (!UseLogFrequencyEvents)
                {
                    nextLogTime = startTime;
                }
            }

            if( UseLogFrequencyEvents && LoggedEventCount % LogFrequencyEvents == 0)
            {
                WriteEventLog();
            }
            else if(UseLogFrequencyEvents && LoggedEventCount == 1)
            {
                WriteEventLog();
            }
            else if( ! UseLogFrequencyEvents && DateTime.Now >= nextLogTime)
            {
                WriteEventLog();
                nextLogTime = nextLogTime.Add(LogFrequencyTime);
            }
        }

        private void WriteEventLog()
        {
            if((UseLogFrequencyEvents && LogFrequencyEvents == 0) || (!UseLogFrequencyEvents && LogFrequencyTime.TotalSeconds <= 0))
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
                logAction("{EventName} occurred {EventCount} times in the last {EventSeconds} seconds", new object[] { EventName, LoggedEventCount, DateTime.Now.Subtract(startTime).TotalSeconds.ToString("0")});
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging.Summarized
{
    public static class LoggingPolicy
    {
        public static SummarizedLogger SetFrequency(this SummarizedLogger logger, int eventCount)
        {
            logger.LoggedEventCount = eventCount;
            return logger;
        }

        public static SummarizedLogger SetFrequency(this SummarizedLogger logger, TimeSpan eventFrequency)
        {
            logger.LogFrequencyTime = eventFrequency;
            return logger;
        }
    }
}

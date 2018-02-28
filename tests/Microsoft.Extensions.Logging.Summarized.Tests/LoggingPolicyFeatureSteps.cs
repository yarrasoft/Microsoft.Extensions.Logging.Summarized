using System;
using TechTalk.SpecFlow;

namespace Microsoft.Extensions.Logging.Summarized.Tests
{
    [Binding]
    public class LoggingPolicyFeatureSteps
    {
        [Given(@"I am setting up a Logging Summary")]
        public void GivenIAmSettingUpALoggingSummary()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"I specify to log every (.*) events")]
        public void GivenISpecifyToLogEveryEvents(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"I specify to log every (.*) minutes")]
        public void GivenISpecifyToLogEveryMinutes(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"The (.*)th event occurs")]
        public void WhenTheThEventOccurs(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"The first event occurs after (.*) minutes")]
        public void WhenTheFirstEventOccursAfterMinutes(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"The summary logger should log an event")]
        public void ThenTheSummaryLoggerShouldLogAnEvent()
        {
            ScenarioContext.Current.Pending();
        }
    }
}

Feature: LoggingPolicyFeature
	As a package consumer
	I should be able to specify a logging policy
	So I can summarize my logs as I see fit

@policy @nEvents @setup
Scenario: Log every 10 events
	Given I am setting up a Logging Summary
	And I specify to log every 10 events 
	When The 10th event occurs
	Then The summary logger should log an event

#TODO
#@policy @nEvents @setup
#Scenario: Log every 10 events
#	Given I am setting up a Logging Summary
#	And I specify to log every 10 events 
#	When The 10th event occurs
#	Then logged event should include the number of events summarized


@policy @nTime @setup
Scenario: Log every 2 minutes
	Given I am setting up a Logging Summary
	And I specify to log every 2 minutes
	When The first event occurs after 2 minutes
	Then The summary logger should log an event



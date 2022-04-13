﻿
using SeleniumSpecFlow.Utilities;
using TechTalk.SpecFlow;
using UI.Model;
using UISelenium.Pages;
using FluentAssertions;
using TestFramework;
using OpenQA.Selenium;
using System;

namespace UI.Steps
{
    [Binding]
    public class LogoffSteps: ObjectFactory
    {
        private readonly ScenarioContext _scenarioContext;
        private Hearing _hearing;
        private static string loginUrl = "login.microsoftonline.com";

        public LogoffSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Then(@"I log off")]
        public void ThenILogOff()
            {
            _scenarioContext.UpdatePageName("logout");
            Driver = (IWebDriver)_scenarioContext["driver"];
            if (ExtensionMethods.IsElementVisible(Driver, Header.LinkSignOut, null))
            {
                Driver.RetryClick(Header.LinkSignOut,_scenarioContext,TimeSpan.FromSeconds(Config.DefaultElementWait));
            }
            else
            {
                Driver.FindElement(Header.SignOut).Click();
            }
        }

        [Then(@"everyone signs out")]
        public void ThenEveryoneSignsOut()
        {
            _hearing = (Hearing)_scenarioContext["Hearing"];
            foreach (var participant in _hearing.Participant)
            {
                Driver = GetDriver(participant.Id, _scenarioContext);
                if (Driver != null)
                {
                    Driver.FindElement(Header.SignOut).Click();
                }
            }
        }
    }
}

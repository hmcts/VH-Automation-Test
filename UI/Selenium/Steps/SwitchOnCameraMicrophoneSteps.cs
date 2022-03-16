﻿using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumSpecFlow.Utilities;
using System;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using TestFramework;
using UISelenium.Pages;
using OpenQA.Selenium.Interactions;
using UI.Utilities;

namespace UI.Steps
{
    [Binding]
    public class SwitchOnCameraMicrophoneSteps : ObjectFactory
    {
        ScenarioContext _scenarioContext;

        SwitchOnCameraMicrophoneSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Then(@"I make sure camera and microphone switched on")]
        public void ThenIMakeSureCameraAndMicrophoneSwitchedOn()
        {
            ExtensionMethods.FindElementWithWait(Driver, SwitchOnCameraMicrophonePage.SwitchOnButton, _scenarioContext).Click();
        }
    }
}
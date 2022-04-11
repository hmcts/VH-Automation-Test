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
using UI.Model;
using TestLibrary.Utilities;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Collections.Generic;

namespace UI.Steps
{
    [Binding]
    public class CheckParticipantStatusToCommandCentreSteps : ObjectFactory
    {
        private readonly ScenarioContext _scenarioContext;
        private Hearing _hearing;

        CheckParticipantStatusToCommandCentreSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Then(@"participants have joined the hearing waiting room")]
        public void ThenParticipantsHaveJoinedTheHearingWaitingRoom()
        {
             int participantNum = 0;
            _hearing = (Hearing)_scenarioContext["Hearing"];
            foreach (var driver in (Dictionary<string, IWebDriver>)_scenarioContext["drivers"])
            {
                Driver = driver.Value;
                string participant = driver.Key.Split('#').FirstOrDefault();

                Driver = GetDriver(participant, _scenarioContext);
                _scenarioContext["driver"] = Driver;
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Config.DefaultElementWait);
                ExtensionMethods.FindElementWithWait(Driver, ParticipantHearingListPage.SelectButton(_hearing.Case.CaseNumber), _scenarioContext, TimeSpan.FromSeconds(Config.DefaultElementWait)).Click();
                if (!(participant.ToLower().Contains("judge") || participant.ToLower().Contains("panel")))
                {
                    if (participantNum == 1)
                    {
                        Driver = driver.Value;
                        ProceedToWaitingRoom(_hearing.Case.CaseNumber);
                        _hearing.HearingId = Driver.Url.Split('/').LastOrDefault();
                        _scenarioContext["Hearing"] = _hearing;
                    }
                    else if (participantNum == 3)
                    {
                        Driver = driver.Value;
                        ProceedToWaitingRoom(_hearing.Case.CaseNumber);
                        _hearing.HearingId = Driver.Url.Split('/').LastOrDefault();
                        _scenarioContext["Hearing"] = _hearing;
                        if (ExtensionMethods.IsElementVisible(Driver, Header.SignOut, null))
                        {
                            Driver.FindElement(Header.SignOut).Click();
                            ExtensionMethods.FindElementWithWait(Driver, Header.SignoutCompletely).Click();
                        }
                    }
                }
                participantNum++;
            }
        }

        public void ProceedToWaitingRoom(string caseNumber)
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Config.DefaultElementWait);
            ExtensionMethods.WaitForElementVisible(Driver, ParticipantHearingListPage.ButtonNext);
            Driver.FindElement(ParticipantHearingListPage.ButtonNext).Click();
            ExtensionMethods.WaitForElementVisible(Driver, ParticipantHearingListPage.ContinueButton);
            Driver.FindElement(ParticipantHearingListPage.ContinueButton).Click();
            ExtensionMethods.WaitForElementVisible(Driver, ParticipantHearingListPage.SwitchOnButton);
            Driver.FindElement(ParticipantHearingListPage.SwitchOnButton).Click();
            ExtensionMethods.WaitForElementVisible(Driver, ParticipantHearingListPage.WatchVideoButton);
            Driver.FindElement(ParticipantHearingListPage.WatchVideoButton).Click();
            // Assert video is playing
            Driver.RetryClick(ParticipantHearingListPage.ContinueButton, _scenarioContext, TimeSpan.FromSeconds(180));
            if (SkipPracticeVideoHearingDemo)
            {
                string cameraUrl = Driver.Url.Replace("practice-video-hearing", "camera-working");
                Driver.Navigate().GoToUrl(cameraUrl);
                Driver.SwitchTo().Alert().Accept();
            }
            else
            {
                ExtensionMethods.FindElementEnabledWithWait(Driver, ParticipantHearingListPage.ContinueButton, 180).Click();
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Config.DefaultElementWait);
            }
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Config.DefaultElementWait);
            Driver.FindElement(ParticipantHearingListPage.CameraWorkingYes)?.Click();
            Driver.FindElement(ParticipantHearingListPage.ContinueButton).Click();
            Driver.FindElement(ParticipantHearingListPage.MicrophoneWorkingYes).Click();
            Driver.FindElement(ParticipantHearingListPage.ContinueButton).Click();
            Driver.FindElement(ParticipantHearingListPage.VideoWorkingYes).Click();
            Driver.FindElement(ParticipantHearingListPage.ContinueButton).Click();
            Driver.FindElement(ParticipantHearingListPage.NextButton).Click();
            Driver.FindElement(ParticipantHearingListPage.DeclareCheckbox).Click();
            Driver.FindElement(ParticipantHearingListPage.NextButton).Click();
        }

        [Then(@"the the Video Hearings Officer should able to see the status")]
        public void ThenTheTheVideoHearingsOfficerShouldAbleToSeeTheStatus()
        {
            _hearing = (Hearing)_scenarioContext["Hearing"];
            foreach (var driver in (Dictionary<string, IWebDriver>)_scenarioContext["drivers"])
            {
                Driver = driver.Value;
                string participant = driver.Key.Split('#').FirstOrDefault();
                Driver = GetDriver(participant, _scenarioContext);
                if (participant.Equals("VHO")){
                    ExtensionMethods.WaitForElementVisible(Driver, VHOHearingListPage.ParticipantName);
                    var participantNames = Driver.FindElements(VHOHearingListPage.ParticipantName);
                    var participantStatus = Driver.FindElements(VHOHearingListPage.ParticipantStatus);

                    Assert.IsTrue(participantNames.Count > 0);

                    for (int i = 1; i < participantNames.Count; i++)
                    {
                        string name = participantNames.ElementAt(i).Text;
                        string status = participantStatus.ElementAt(i).Text;

                        if (name.Contains("Individual_05"))
                        {
                            Assert.IsTrue(status.Equals("Available"));
                        }
                        else if (name.Contains("Individual_06"))
                        {
                            Assert.IsTrue(status.Equals("Joining"));
                        }
                        else if (name.Contains("Individual_07"))
                        {
                            Assert.IsTrue(status.Equals("Disconnected"));
                        }
                        else if (name.Contains("Individual_08"))
                        {
                            Assert.IsTrue(status.Equals("Not signed in"));
                        }
                    }
                }
            }
            
        }
    }
}
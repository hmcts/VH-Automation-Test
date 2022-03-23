﻿using OpenQA.Selenium.Support.UI;
using SeleniumSpecFlow.Utilities;
using System;
using System.Linq;
using TechTalk.SpecFlow;
using TestFramework;
using UI.Model;
using UISelenium.Pages;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium;
using FluentAssertions;

namespace UI.Steps
{
    [Binding]
    public class ConsultationRoomSteps : ObjectFactory
    {
        private readonly ScenarioContext _scenarioContext;
        private Hearing _hearing;
        public ConsultationRoomSteps(ScenarioContext context)
            : base(context)
        {
            _scenarioContext = context;
            _hearing = _scenarioContext.Get<Hearing>("Hearing");
        }

        [When(@"judge invites participant into the consultation")]
        public void WhenJudgeInvitesParticipantIntoTheConsultation()
        {
            _scenarioContext.UpdatePageName("Consultation Room");

            Driver = GetDriver("Judge", _scenarioContext);
            _scenarioContext["driver"] = Driver;

            var participantName = _hearing.Participant[0].Name;

            var element = ExtensionMethods.FindElementWithWait(Driver, ConsultationRoomPage.InviteParticipant(participantName.FirstName), _scenarioContext);
            element.Click();
        }


        [When(@"participant accepts the consultation room invitation")]
        public void WhenParticipantAcceptsTheConsultationRoomInvitation()
        {
            var participant = _hearing.Participant.FirstOrDefault(p => !p.Id.ToLower().Contains("judge"));
            var participantKey = $"{participant.Id}#{participant.Party.Name}-{participant.Role.Name}";
            Driver = GetDriver(participantKey, _scenarioContext);
            _scenarioContext["driver"] = Driver;

            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(Config.DefaultElementWait));
            wait.Until(ExpectedConditions.ElementToBeClickable(ParticipantWaitingRoomPage.AcceptConsultationButton));

            var element = ExtensionMethods.FindElementWithWait(Driver, ParticipantWaitingRoomPage.AcceptConsultationButton, _scenarioContext);
            element.Click();
        }

        [Then(@"judge checks participant joined the consultation room")]
        public void ThenJudgeChecksParticipantJoinedTheConsultationRoom()
        {
            Driver = GetDriver("Judge", _scenarioContext);
            _scenarioContext["driver"] = Driver;

            var participant = _hearing.Participant.FirstOrDefault(p => !p.Id.ToLower().Contains("judge"));
            var participantName = participant.Name;
            ExtensionMethods.FindElementWithWait(Driver, ConsultationRoomPage.ParticipantTick(participantName.FirstName), _scenarioContext, TimeSpan.FromSeconds(Config.DefaultElementWait));
            var isTickVisible = ExtensionMethods.IsElementVisible(Driver, ConsultationRoomPage.ParticipantTick(participantName.FirstName), _scenarioContext);
            isTickVisible.Should().BeTrue("tick icon in judge panel not visible");
        }

        [Then(@"all participants leave consultation room")]
        public void ThenAllParticipantsLeaveConsultationRoom()
        {
            Driver = GetDriver("Judge", _scenarioContext);
            _scenarioContext["driver"] = Driver;

            var script = "document.getElementById('leaveButton-landscape').click();";
            var scriptExecutor = Driver as IJavaScriptExecutor;
            scriptExecutor.ExecuteScript(script);
            ExtensionMethods.FindElementWithWait(Driver, ConsultationRoomPage.ConfirmLeaveButton, _scenarioContext).Click();

            ExtensionMethods.FindElementWithWait(Driver, JudgeWaitingRoomPage.EnterPrivateConsultationButton, _scenarioContext);

            var isBtnVisible = ExtensionMethods.IsElementVisible(Driver, JudgeWaitingRoomPage.EnterPrivateConsultationButton, _scenarioContext);
            isBtnVisible.Should().BeTrue("Judge didn't leave consultation room");

            var participant = _hearing.Participant.FirstOrDefault(p => !p.Id.ToLower().Contains("judge"));
            var participantKey = $"{participant.Id}#{participant.Party.Name}-{participant.Role.Name}";
            Driver = GetDriver(participantKey, _scenarioContext);
            _scenarioContext["driver"] = Driver;

            scriptExecutor = Driver as IJavaScriptExecutor;
            scriptExecutor.ExecuteScript(script);
            ExtensionMethods.FindElementWithWait(Driver, ConsultationRoomPage.ConfirmLeaveButton, _scenarioContext).Click();

            Driver.FindElement(ParticipantWaitingRoomPage.ParticipantDetails($"{participant.Name.FirstName} {participant.Name.LastName}")).Displayed.Should().BeTrue();

            ExtensionMethods.FindElementWithWait(Driver, ParticipantWaitingRoomPage.ParticipantDetails(_hearing.Case.CaseNumber), _scenarioContext, TimeSpan.FromSeconds(Config.DefaultElementWait)).Displayed.Should().BeTrue();
            Driver.FindElement(ParticipantWaitingRoomPage.ChooseCameraAndMicButton).Displayed.Should().BeTrue();
            if (participant.Role.Name.Contains("Panel Member"))
            {
                Driver.FindElement(ParticipantWaitingRoomPage.ParticipantDetails(participant.Name.FirstName)).Displayed.Should().BeTrue();
                ExtensionMethods.FindElementWithWait(Driver, JudgeWaitingRoomPage.EnterPrivateConsultationButton, _scenarioContext);

                isBtnVisible = ExtensionMethods.IsElementVisible(Driver, JudgeWaitingRoomPage.EnterPrivateConsultationButton, _scenarioContext);
                isBtnVisible.Should().BeTrue("Panel member didn't leave consultation room");
            }
            else
            {
                Driver.FindElement(ParticipantWaitingRoomPage.ParticipantDetails(participant.Party.Name)).Displayed.Should().BeTrue();
                Driver.FindElement(ParticipantWaitingRoomPage.JoinPrivateMeetingButton).Displayed.Should().BeTrue();
            }
        }

        [Then(@"both participants click on the leave button to leave the room")]
        public void ThenBothParticipantsLeaveConsultationRoom()
        {
            var participants = _hearing.Participant.Where(p => p.Party.Name.ToLower().Contains("claimant") || p.Party.Name.ToLower().Contains("defendant")).ToList();

            foreach (var participant in participants)
            {
                var participantKey = $"{participant.Id}#{participant.Party.Name}-{participant.Role.Name}";
                Driver = GetDriver(participantKey, _scenarioContext);
                _scenarioContext["driver"] = Driver;

                var script = "document.getElementById('leaveButton-landscape').click();";
                var scriptExecutor = Driver as IJavaScriptExecutor;
                scriptExecutor.ExecuteScript(script);
                ExtensionMethods.FindElementWithWait(Driver, ConsultationRoomPage.ConfirmLeaveButton, _scenarioContext).Click();

                ExtensionMethods.FindElementWithWait(Driver, ParticipantWaitingRoomPage.StartPrivateMeetingButton, _scenarioContext);
                Driver.FindElement(ParticipantWaitingRoomPage.ParticipantDetails($"{participant.Name.FirstName} {participant.Name.LastName}")).Displayed.Should().BeTrue();

                ExtensionMethods.FindElementWithWait(Driver, ParticipantWaitingRoomPage.ParticipantDetails(_hearing.Case.CaseNumber), _scenarioContext, TimeSpan.FromSeconds(Config.DefaultElementWait)).Displayed.Should().BeTrue();
                Driver.FindElement(ParticipantWaitingRoomPage.ChooseCameraAndMicButton).Displayed.Should().BeTrue();
                Driver.FindElement(ParticipantWaitingRoomPage.JoinPrivateMeetingButton).Displayed.Should().BeTrue();
            }
        }

        [When(@"1st participant start a private meeting and selects 2nd participant")]
        public void When1stParticipantStartAPrivateMeetingAndSelects2ndParticipant()
        {
            var participants = _hearing.Participant.Where(p => p.Party.Name.ToLower().Contains("claimant") || p.Party.Name.ToLower().Contains("defendant")).ToList(); ;
            var firstParticipant = participants[0];
            var firstParticipantKey = $"{firstParticipant.Id}#{firstParticipant.Party.Name}-{firstParticipant.Role.Name}";
            Driver = GetDriver(firstParticipantKey, _scenarioContext);
            _scenarioContext["driver"] = Driver;

            var secondParticipant = participants[1];
            var secondParticipantKey = $"{secondParticipant.Id}#{secondParticipant.Party.Name}-{secondParticipant.Role.Name}";

            ExtensionMethods.FindElementWithWait(Driver, ParticipantWaitingRoomPage.StartPrivateMeetingButton, _scenarioContext, TimeSpan.FromSeconds(Config.DefaultElementWait)).Click();
            var inputElement = Driver.FindElement(By.TagName("input"));

            Driver.SwitchTo().ActiveElement();
            var modelDialog = Driver.FindElement(ParticipantWaitingRoomPage.PrivateMeetingModal);
            var checkbox = modelDialog.FindElement(ParticipantWaitingRoomPage.JointPrivateMeetingCheckbox(secondParticipant.Name.FirstName));
            checkbox.Click();

            modelDialog.FindElement(ParticipantWaitingRoomPage.ContinueJoiningPrivateMeetingButton).Click();

            Driver = GetDriver(secondParticipantKey, _scenarioContext);
            _scenarioContext["driver"] = Driver;

            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(Config.DefaultElementWait));
            wait.Until(ExpectedConditions.ElementToBeClickable(ParticipantWaitingRoomPage.AcceptConsultationButton));

            ExtensionMethods.FindElementWithWait(Driver, ParticipantWaitingRoomPage.AcceptConsultationButton, _scenarioContext).Click();
        }

        [Then(@"2nd participant is in the private consultation room")]
        public void Then2ndParticipantIsInThePrivateConsultationRoom()
        {
            var participants = _hearing.Participant.Where(p => p.Party.Name.ToLower().Contains("claimant") || p.Party.Name.ToLower().Contains("defendant")).ToList();
            var secondParticipant = participants[1];
            var secondParticipantKey = $"{secondParticipant.Id}#{secondParticipant.Party.Name}-{secondParticipant.Role.Name}";
            Driver = GetDriver(secondParticipantKey, _scenarioContext);
            _scenarioContext["driver"] = Driver;

            ExtensionMethods.FindElementWithWait(Driver, ConsultationRoomPage.ParticipantTick(secondParticipant.Name.FirstName), _scenarioContext, TimeSpan.FromSeconds(Config.DefaultElementWait));
            var isTickVisible = ExtensionMethods.IsElementVisible(Driver, ConsultationRoomPage.ParticipantTick(secondParticipant.Name.FirstName), _scenarioContext);
            isTickVisible.Should().BeTrue("tick icon in 2nd participant panel not visible");
        }
    }
}

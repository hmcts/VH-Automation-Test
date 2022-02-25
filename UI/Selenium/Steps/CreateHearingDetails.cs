﻿using FluentAssertions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumSpecFlow.Utilities;
using System;
using TechTalk.SpecFlow;
using TestFramework;
using UI.Model;
using UISelenium.Pages;
using System.Linq;
using System.Collections.Generic;
namespace UI.Steps
{
    [Binding]
    public class CreateHearingDetails : ObjectFactory
    {
        private readonly ScenarioContext _scenarioContext;
        private Hearing _hearing;
        
        public CreateHearingDetails(ScenarioContext scenarioContext)
           : base (scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I want to create a hearing with case details")]
        public void GivenIWantToCreateAHearingWithCaseDetails(Table table)
        {
            CreateCaseModel(table);
            EnterCaseDetails(_hearing.Case);
        }

        public void EnterCaseDetails(Case caseDetails)
        {
            Driver.FindElement(HearingDetailsPage.CaseNumber).SendKeys(caseDetails.CaseNumber);
            Driver.FindElement(HearingDetailsPage.CaseName).SendKeys(caseDetails.CaseName);
            var caseTypeElement = Driver.FindElement(HearingDetailsPage.CaseType);
            var selectElement = new SelectElement(caseTypeElement);
            selectElement.SelectByText(caseDetails.CaseType);
            selectElement = new SelectElement(ExtensionMethods.WaitForDropDownListItems(Driver, HearingDetailsPage.HeardingType));
            selectElement.SelectByText(caseDetails.HearingType);
            ExtensionMethods.FindElementWithWait(Driver, HearingDetailsPage.NextButton).Click();
        }

        private void CreateCaseModel(Table table)
        {
            if (_scenarioContext.ContainsKey("Hearing"))
            {
                _hearing=_scenarioContext.Get<Hearing>("Hearing");
            }
            else
            {
                _hearing=new Hearing();
                _scenarioContext.Add("Hearing", _hearing);
            }
            var tableRow = table.Rows[0];
            _hearing.Case.CaseNumber=$"{tableRow["Case Number"]}{new Random().Next(99, 99999).ToString()}";
            _hearing.Case.CaseName=$"{tableRow["Case Name"]}-{_hearing.Case.CaseNumber}";
            _hearing.Case.CaseType=tableRow["Case Type"];
            _hearing.Case.HearingType=tableRow["Hearing Type"];
            _scenarioContext["Hearing"]=_hearing;
        }
    }
}

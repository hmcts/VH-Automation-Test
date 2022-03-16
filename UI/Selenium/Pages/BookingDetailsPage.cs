﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
namespace UISelenium.Pages
{
    public class BookingDetailsPage
    {
        public static By ConfirmBookingButton => By.Id("confirm-button");
        public static By CancelBookingButton => By.Id("cancel-button");
        public static By EditBookingButton => By.Id("edit-button");

        public static By CloseBookingFailureWindowButton = By.Id("btnTryAgain");
        public static By BookingConfirmedStatus = By.XPath("//div[@class='vh-created-booking'][text()='Confirmed']");
    }
}
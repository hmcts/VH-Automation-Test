﻿using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.IO;
using System.Linq;

namespace UI.Utilities
{
    ///<summary>
    /// Class to work with application configuration
    ///</summary>
    public class TestConfigHelper
    {
        public static BrowserType browser { get; private set; }
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static IConfigurationRoot GetIConfigurationBase()
        {
            return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("passwords.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        }

        public static IConfigurationRoot BuildConfig(string userSecretsId, string testSecretsId)
        {
            var testConfigBuilder = new ConfigurationBuilder()
                .AddUserSecrets(testSecretsId)
                .Build();

            return new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json")
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddJsonFile($"appsettings.Production.json", optional: true)
                .AddUserSecrets(userSecretsId)
                .AddConfiguration(testConfigBuilder)
                .Build();
        }

        public static EnvironmentConfigSettings GetApplicationConfiguration()
        {
            EnvironmentConfigSettings configSettings = null;
            LaunchSettingsFixture();
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            browser = (BrowserType)Enum.Parse(typeof(BrowserType), Environment.GetEnvironmentVariable("BROWSER"));
            var systemConfiguration = new SystemConfiguration();
            var iTestConfigurationRoot = BuildConfig("CA353381-2F0D-47D7-A97B-79A30AFF8B86", "18c466fd-9265-425f-964e-5989181743a7");
            //var iTestConfigurationRoot = GetIConfigurationBase();
            Logger.Info("Reading Appsetitngs Json File");
            iTestConfigurationRoot.GetSection("SystemConfiguration").Bind(systemConfiguration);
            if (environment != null)
            {
                if (environment.ToLower() == "Development".ToLower())
                {
                    configSettings = systemConfiguration.DevelopmentEnvironmentConfigSettings;
                }
                else if (environment.ToLower() == "Acceptance".ToLower())
                {
                    configSettings = systemConfiguration.AcceptanceEnvironmentConfigSettings;
                }
                else if (environment.ToLower() == "Production".ToLower())
                {
                    configSettings = systemConfiguration.ProductionEnvironmentConfigSettings;
                }
                //set the correct ElementWait based on execution environment
                if (configSettings.RunOnSaucelabs)
                {
                    configSettings.DefaultElementWait = configSettings.SaucelabsElementWait;
                }
            }
            return configSettings;
        }

        public static void LaunchSettingsFixture()
        {
            var cs = AppDomain.CurrentDomain.BaseDirectory.ToString();
            using (var file = File.OpenText(@"Properties/launchSettings.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);
                var variables = jObject
                    .GetValue("profiles")
                    //select a proper profile here
                    .SelectMany(profiles => profiles.Children())
                    .SelectMany(profile => profile.Children<JProperty>())
                    .Where(prop => prop.Name == "environmentVariables")
                    .SelectMany(prop => prop.Value.Children<JProperty>())
                    .ToList();
                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }
        }
    }
}

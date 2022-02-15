﻿namespace TestLibrary.Utilities
{
    public class SystemConfiguration
    {
        public EnvironmentConfigSettings DevelopmentEnvironmentConfigSettings { get; set; }
        public EnvironmentConfigSettings StagingEnvironmentConfigSettings { get; set; }
        public EnvironmentConfigSettings ProductionEnvironmentConfigSettings { get; set; }
    }
}
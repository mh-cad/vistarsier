using CAPI.Agent;
using CAPI.Common.Config;
using System;

namespace CAPI.UAT.Tests
{
    public class DbConnectionString : IUatTest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
        public string TestGroup { get; set; }
        public CapiConfig CapiConfig { get; set; }
        public AgentRepository Context { get; set; }

        public DbConnectionString()
        {
            Name = "Database Connection String";
            Description = "Checks if a valid database connection string exists in config.json";
            SuccessMessage = "Valid connection string in config.json file";
            FailureMessage = "Invalid connection string in config.json file";
            TestGroup = "Database";
        }

        public bool Run()
        {
            var connectionString = //CapiConfig.AgentDbConnectionString;
                "Server=172.28.43.65;Database=Capi;User Id=sa;Password=radsysadmin;Connection Timeout=120";
            try
            {
                Context = new AgentRepository(connectionString);

                var dbExists = Context.Database.EnsureCreated();

                if (dbExists)
                {
                    Logger.Write("Successfully connected to database using provided connection string", true,
                        Logger.TextType.Success);
                    Logger.Write(connectionString, true, Logger.TextType.Success);
                }
                else
                {
                    Logger.Write("Failed to connect to database using provided connection string", true, Logger.TextType.Fail);
                    Logger.Write(connectionString, true, Logger.TextType.Fail);
                    return false;
                }
            }
            catch (Exception)
            {
                Logger.Write("Error occured while trying to connect to database using provided connection string", true, Logger.TextType.Fail, false, 1, 0);
                Logger.Write(connectionString, true, Logger.TextType.Fail);
                throw;
            }
            return true;
        }

        public void FailureResolution()
        {
            // UPDATE THIS PART

            Logger.Write("Please provide a valid connection string pointing to an active and running instance of SQL server. Database will be automatically created if does not exist", true, Logger.TextType.Fail, false, 1, 0);
        }
    }
}
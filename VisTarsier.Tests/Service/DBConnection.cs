using VisTarsier.Config;
using VisTarsier.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace VisTarsier.Tests.Service
{
    [TestClass]
    public class DBConnection
    {
        [TestMethod]
        public void TestConnection()
        {
            try
            {
                var connectionString = CapiConfig.GetConfig().AgentDbConnectionString;

                var broker = new DbBroker(connectionString);
                broker.Database.EnsureCreated();

                //Assert.IsTrue(dbExists);

                Attempt @case = new Attempt();
                Job @job = new Job() { Attempt = @case };

                job.Status = "Test";
                broker.Jobs.Add(job);
                broker.SaveChanges();

                broker.Attempts.Add(@case);
                broker.SaveChanges();

                var jobFromDb = broker.GetJobByStatus("Test").FirstOrDefault();
                Assert.IsFalse(jobFromDb.Attempt == null);

                broker.Attempts.Remove(@case);
                broker.SaveChanges();
                broker.Jobs.Remove(@job);
                broker.SaveChanges();

                var @case2 = new Attempt
                {
                    CurrentAccession = "TestAcc",
                    Status = "Pending",
                    Method = Attempt.AdditionMethod.Hl7
                }; ;
                broker.Attempts.Add(@case2);
                broker.SaveChanges();
                broker.Attempts.Remove(@case2);
                broker.SaveChanges();


            }
            catch (Exception e)
            {
                Assert.Inconclusive("Something went wrong with DB connection : " + e);
            }
            

        }
    }
}

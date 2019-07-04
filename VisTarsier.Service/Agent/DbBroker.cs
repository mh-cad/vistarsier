using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisTarsier.Service
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DbBroker : DbContext
    {
        private readonly string _connectionString;

        public DbSet<Attempt> Attempts { get; set; }
        public DbSet<Job> Jobs { get; set; }

        public DbBroker()
        {
            _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Capi;Trusted_Connection=True;";
        }

        public DbBroker(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(_connectionString);
        }

        #region "Attempts"
        public IEnumerable<Attempt> GetCaseByStatus(string status)
        {
            return Attempts.Where(c => c != null && c.Status != null && c.Status.Equals(status, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
        #endregion

        #region "Jobs"
        public IEnumerable<Job> GetJobByStatus(string status)
        {
            return Jobs.Where(j => j != null && j.Status != null && j.Status.Equals(status, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
        #endregion
    }
}

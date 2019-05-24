using VisTarsier.Service.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisTarsier.Service.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DbBroker : DbContext
    {
        private readonly string _connectionString;

        public DbSet<Case> Cases { get; set; }
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
            optionsBuilder.UseSqlServer(_connectionString);
        }

        #region "Cases"
        public IEnumerable<Case> GetCaseByStatus(string status)
        {
            return Cases.Where(c => c.Status.Equals(status, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
        #endregion

        #region "Jobs"
        public IEnumerable<Job> GetJobByStatus(string status)
        {
            return Jobs.Where(j => j.Status.Equals(status, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
        #endregion
    }
}

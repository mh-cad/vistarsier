﻿using CAPI.Agent.Abstractions;
using CAPI.Agent.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AgentRepository : DbContext, IAgentRepository
    {
        private readonly string _connectionString;

        public DbSet<Case> Cases { get; set; }
        public DbSet<Job> Jobs { get; set; }

        public AgentRepository()
        {
            _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Capi;Trusted_Connection=True;";
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

        #endregion
    }
}

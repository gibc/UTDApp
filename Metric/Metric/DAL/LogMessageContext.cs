﻿using Metric.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Metric.DAL
{
    public class LogMessageContext : DbContext
    {
        public LogMessageContext() : base("LoggingConnection")
        {
        }

        public DbSet<AppHosts> AppHosts { get; set; }
        public DbSet<LogMessage> LogMessage { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
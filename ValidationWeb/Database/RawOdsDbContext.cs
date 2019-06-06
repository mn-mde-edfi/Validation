﻿using System.Data.Entity;
using ValidationWeb.Models;
using ValidationWeb.Services.Implementations;
using ValidationWeb.Services.Interfaces;

namespace ValidationWeb.Database
{
    public class RawOdsDbContext : DbContext
    {
        static RawOdsDbContext()
        {
            // Fixes a known bug in which EntityFramework.SqlServer.dll is not copied into consumer even when CopyLocal is True.
            // todo: i'm unsure about that entire comment
            var includeSqlServerDLLInConsumer = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        // TOOD: remove ... 
        public RawOdsDbContext(string fourDigitYear)
            : base(new OdsConfigurationValues().GetRawOdsConnectionString(fourDigitYear))
        {
        }
        
        // Use of connection string ensures that a new database won't be created by default.
        public RawOdsDbContext(IOdsConfigurationValues odsConfigurationValues, string fourDigitYear)
            : base(odsConfigurationValues.GetRawOdsConnectionString(fourDigitYear))
        {
        }
        
        public virtual DbSet<RuleValidation> RuleValidations { get; set; }

        public virtual DbSet<RuleValidationDetail> RuleValidationDetails { get; set; }

        public virtual DbSet<RuleValidationRuleComponent> RuleValidationRuleComponents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RuleValidationDetail>()
               .HasRequired(dt => dt.RuleValidation)
               .WithMany(dt => dt.RuleValidationDetails)
               .HasForeignKey(dt => dt.RuleValidationId)
               .WillCascadeOnDelete(true);

            modelBuilder.Entity<RuleValidationRuleComponent>()
               .HasRequired(dt => dt.RuleValidation)
               .WithMany(dt => dt.RuleValidationRuleComponents)
               .HasForeignKey(dt => dt.RuleValidationId)
               .WillCascadeOnDelete(true);
        }
    }
}


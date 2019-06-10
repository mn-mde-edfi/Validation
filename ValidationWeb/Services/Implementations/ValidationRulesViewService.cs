﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

using ValidationWeb.Database;
using ValidationWeb.Models;
using ValidationWeb.Services.Interfaces;

namespace ValidationWeb.Services.Implementations
{
    public class ValidationRulesViewService : IValidationRulesViewService
    {
        public ValidationRulesViewService(
            IDbContextFactory<ValidationPortalDbContext> validationPortalDataContextFactory,
            ISchoolYearDbContextFactory schoolYearDbContextFactory,
            ISchoolYearService schoolYearService)
        {
            ValidationPortalDataContextFactory = validationPortalDataContextFactory;
            SchoolYearDbContextFactory = schoolYearDbContextFactory;
            SchoolYearService = schoolYearService;
        }

        protected IDbContextFactory<ValidationPortalDbContext> ValidationPortalDataContextFactory { get; set; }

        protected ISchoolYearDbContextFactory SchoolYearDbContextFactory { get; set; }

        protected ISchoolYearService SchoolYearService { get; set; }

        public IEnumerable<ValidationRulesView> GetRulesViews(int schoolYearId)
        {
            using (var validationPortalContext = ValidationPortalDataContextFactory.Create())
            {
                return validationPortalContext.ValidationRulesViews
                    .Include(x => x.RulesFields)
                    .Where(x => x.SchoolYearId == schoolYearId)
                    .ToList();
            }
        }

        public void DeleteRulesForSchoolYear(int schoolYearId)
        {
            using (var validationPortalContext = ValidationPortalDataContextFactory.Create())
            {
                var existingViews = validationPortalContext.ValidationRulesViews
                    .Include(x => x.RulesFields)
                    .Where(x => x.SchoolYearId == schoolYearId);

                validationPortalContext.ValidationRulesViews.RemoveRange(existingViews);
                validationPortalContext.SaveChanges();
            }
        }

        public void UpdateRulesForSchoolYear(int schoolYearId)
        {
            DeleteRulesForSchoolYear(schoolYearId);

            var viewsForYear = new List<ValidationRulesView>();

            var schoolYear = SchoolYearService.GetSchoolYearById(schoolYearId);
            using (var schoolYearContext = SchoolYearDbContextFactory.CreateWithParameter(schoolYear.EndYear))
            {
                var connection = schoolYearContext.Database.Connection;
                connection.Open();

                var queryCommand = connection.CreateCommand();
                queryCommand.CommandType = System.Data.CommandType.Text;
                queryCommand.CommandText = @"select OBJECT_SCHEMA_NAME(v.object_id) [Schema], v.name [Name] FROM sys.views as v where OBJECT_SCHEMA_NAME(v.object_id)=@schemaName";
                queryCommand.Parameters.Add(new SqlParameter("@schemaName", "rules"));

                using (var reader = queryCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rulesView = new ValidationRulesView
                        {
                            Enabled = true,
                            Schema = reader["Schema"].ToString(),
                            Name = reader["Name"].ToString(),
                            SchoolYearId = schoolYearId
                        };

                        viewsForYear.Add(rulesView);
                    }
                }
            }

            using (var validationPortalContext = ValidationPortalDataContextFactory.Create())
            {

                foreach (var view in viewsForYear)
                {
                    var fieldNames = GetFieldsForView(schoolYear, view.Schema, view.Name);
                    foreach (var fieldName in fieldNames)
                    {
                        var rulesField = new ValidationRulesField {Enabled = true, Name = fieldName};
                        validationPortalContext.ValidationRulesFields.Add(rulesField);
                        view.RulesFields.Add(rulesField);
                    }
                }

                validationPortalContext.ValidationRulesViews.AddRange(viewsForYear);
                validationPortalContext.SaveChanges();
            }
        }

        public IEnumerable<string> GetFieldsForView(SchoolYear schoolYear, string schema, string name)
        {
            var fieldsForView = new List<string>();

            using (var schoolYearContext = SchoolYearDbContextFactory.CreateWithParameter(schoolYear.EndYear))
            {
                var connection = schoolYearContext.Database.Connection;
                connection.Open();

                var queryCommand = connection.CreateCommand();
                queryCommand.CommandType = System.Data.CommandType.Text;
                queryCommand.CommandText =
                    @"SELECT COLUMN_NAME FROM information_schema.columns WHERE TABLE_SCHEMA=@schemaName and table_name = @viewName";

                queryCommand.Parameters.Add(new SqlParameter("@schemaName", schema));
                queryCommand.Parameters.Add(new SqlParameter("@viewName", name));

                using (var reader = queryCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fieldsForView.Add(reader["COLUMN_NAME"].ToString());
                    }
                }
            }

            return fieldsForView;
        }
    }
}
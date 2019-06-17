﻿using System.Collections.Generic;
using ValidationWeb.Models;

namespace ValidationWeb.Services.Interfaces
{
    public interface IDynamicReportingService
    {
        IEnumerable<ValidationRulesView> GetRulesViews(int schoolYearId);

        void DeleteViewsAndRulesForSchoolYear(int schoolYearId);
         
        void UpdateViewsAndRulesForSchoolYear(int schoolYearId);
        
        IEnumerable<DynamicReportDefinition> GetReportDefinitions();
        
        void SaveReportDefinition(DynamicReportDefinition reportDefinition);

        DynamicReportDefinition GetReportDefinition(int id);

        IList<dynamic> GetReportData(DynamicReportRequest request);
    }
}
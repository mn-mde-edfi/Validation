﻿using System.Collections.Generic;
using ValidationWeb.Models;

namespace ValidationWeb.Services.Interfaces
{
    public interface ISchoolYearService
    {
        IList<SchoolYear> GetSubmittableSchoolYears();
        
        Dictionary<int, string> GetSubmittableSchoolYearsDictionary();
        
        SchoolYear GetSchoolYearById(int id);
        
        bool AddNewSchoolYear(
            string startDate,
            string endDate);

        bool HideSchoolYear(int id);

        bool RevealSchoolYear(int id);

        IList<SchoolYear> GetAllSchoolYears();
    }
}
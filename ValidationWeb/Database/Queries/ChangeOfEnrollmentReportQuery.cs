﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ValidationWeb
{
    public class ChangeOfEnrollmentReportQuery
    {
        public static string ChangeOfEnrollmentQuery = "rules.ChangeOfEnrollment";
        public const string IsCurrentDistrictColumnName = "IsCurrentDistrict";
        public const string CurrentDistEdOrgIdColumnName = "CurrentDistEdOrgId";
        public const string CurrentDistrictNameColumnName = "CurrentDistrictName";
        public const string CurrentSchoolEdOrgIdColumnName = "CurrentSchoolEdOrgId";
        public const string CurrentSchoolNameColumnName = "CurrentSchoolName";
        public const string CurrentEdOrgEnrollmentDateColumnName = "CurrentEdOrgEnrollmentDate";
        public const string CurrentEdOrgExitDateColumnName = "CurrentEdOrgExitDate";
        public const string CurrentGradeColumnName = "CurrentGrade";
        public const string PastDistEdOrgIdColumnName = "CurrentDistEdOrgId";
        public const string PastDistrictNameColumnName = "CurrentDistrictName";
        public const string PastSchoolEdOrgIdColumnName = "CurrentSchoolEdOrgId";
        public const string PastSchoolNameColumnName = "CurrentSchoolName";
        public const string PastEdOrgEnrollmentDateColumnName = "CurrentEdOrgEnrollmentDate";
        public const string PastEdOrgExitDateColumnName = "CurrentEdOrgExitDate";
        public const string PastGradeColumnName = "CurrentGrade";
        public const string StudentIDColumnName = "StudentID";
        public const string StudentLastNameColumnName = "StudentLastName";
        public const string StudentFirstNameColumnName = "StudentFirstName";
        public const string StudentMiddleNameColumnName = "StudentMiddleName";
        public const string StudentBirthDateColumnName = "StudentBirthDate";
        public bool IsCurrentDistrict { get; set; }
        public int CurrentDistEdOrgId { get; set; }
        public string CurrentDistrictName { get; set; }
        public int CurrentSchoolEdOrgId { get; set; }
        public string CurrentSchoolName { get; set; }
        public DateTime? CurrentEdOrgEnrollmentDate { get; set; }
        public DateTime? CurrentEdOrgExitDate { get; set; }
        public string CurrentGrade { get; set; }
        public int PastDistEdOrgId { get; set; }
        public string PastDistrictName { get; set; }
        public int PastSchoolEdOrgId { get; set; }
        public string PastSchoolName { get; set; }
        public DateTime? PastEdOrgEnrollmentDate { get; set; }
        public DateTime? PastEdOrgExitDate { get; set; }
        public string PastGrade { get; set; }
        public string StudentID { get; set; }
        public string StudentLastName { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentMiddleName { get; set; }
        public DateTime? StudentBirthDate { get; set; }
    }
}
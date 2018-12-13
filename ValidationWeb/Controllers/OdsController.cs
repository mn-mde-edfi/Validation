﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Engine.Models;
using ValidationWeb.Services;

namespace ValidationWeb
{
    using System;
    using System.Collections;

    using DataTables.AspNet.Core;
    using DataTables.AspNet.Mvc5;

    using ValidationWeb.DataCache;

    public class OdsController : Controller
    {
        
        public OdsController(
            IAppUserService appUserService,
            IEdOrgService edOrgService,
            IOdsDataService odsDataService,
            IRulesEngineService rulesEngineService,
            ISchoolYearService schoolyearService,
            ICacheManager cacheManager,
            Model engineObjectModel)
        {
            AppUserService = appUserService;
            EdOrgService = edOrgService;
            OdsDataService = odsDataService;
            EngineObjectModel = engineObjectModel;
            RulesEngineService = rulesEngineService;
            SchoolyearService = schoolyearService;
            CacheManager = cacheManager;
        }

        protected readonly IAppUserService AppUserService;
        
        protected readonly IEdOrgService EdOrgService;

        protected readonly IOdsDataService OdsDataService;

        protected readonly IRulesEngineService RulesEngineService;

        protected readonly ISchoolYearService SchoolyearService;

        protected readonly ICacheManager CacheManager;

        protected readonly Model EngineObjectModel;

        // GET: Ods/Reports
        public ActionResult Reports()
        {
            var session = AppUserService.GetSession();
            var edOrg = EdOrgService.GetEdOrgById(session.FocusedEdOrgId, session.FocusedSchoolYearId);
            var edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            var edOrgId = edOrg.Id;
            var theUser = AppUserService.GetUser();
            var model = new OdsReportsViewModel
            {
                EdOrgId = edOrgId,
                EdOrgName = edOrgName,
                User = theUser
            };
            return View(model);
        }

        // GET: Ods/DemographicsReport
        public ActionResult DemographicsReport(
            bool isStateMode = false,
            int? districtToDisplay = null,
            bool isStudentDrillDown = false,
            int? schoolId = null,
            int? drillDownColumnIndex = null,
            OrgType orgType = OrgType.District)
        {
            var session = AppUserService.GetSession();
            var edOrg = EdOrgService.GetEdOrgById(session.FocusedEdOrgId, session.FocusedSchoolYearId);
            var edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            var edOrgId = edOrg.Id;

            // A state user can look at any district via a link, without changing the default district.
            if (districtToDisplay.HasValue && session.UserIdentity.AuthorizedEdOrgs.Select(eorg => eorg.Id).Contains(districtToDisplay.Value))
            {
                edOrgId = districtToDisplay.Value;
                edOrg = EdOrgService.GetEdOrgById(edOrgId, session.FocusedSchoolYearId);
                edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            }

            string schoolName = string.Empty;

            if (orgType == OrgType.School && schoolId.HasValue)
            {
                schoolName = EdOrgService.GetSingleEdOrg(schoolId.Value, session.FocusedSchoolYearId)?.OrganizationName;
            }
            
            var fourDigitSchoolYear = SchoolyearService.GetSchoolYearById(session.FocusedSchoolYearId).EndYear;
            var theUser = AppUserService.GetUser();

            if (isStudentDrillDown)
            {
                var studentDrillDownModel = new StudentDrillDownViewModel
                {
                    ReportName = "Race and Ancestry Ethnic Origin",
                    EdOrgId = edOrgId,
                    EdOrgName = edOrgName,
                    User = theUser,
                    FourDigitSchoolYear = fourDigitSchoolYear,
                    DrillDownColumnIndex = drillDownColumnIndex,
                    IsStateMode = isStateMode,
                    SchoolId = schoolId,
                    OrgType = orgType,
                    SchoolName = schoolName
                };

                return View("StudentDrillDown", studentDrillDownModel);
            }

            var model = new OdsDemographicsReportViewModel
            {
                EdOrgId = edOrgId,
                FourDigitSchoolYear = fourDigitSchoolYear,
                EdOrgName = edOrgName,
                User = theUser,
                DistrictToDisplay = districtToDisplay,
                IsStudentDrillDown = isStudentDrillDown,
                IsStateMode = isStateMode,
                SchoolId = schoolId,
                SchoolName = schoolName
            };

            return View(model);
        }

        public JsonResult GetStudentDrilldownData(
            OrgType orgType,
            int? schoolId,
            int edOrgId,
            int drillDownColumnIndex,
            string fourDigitSchoolYear,
            IDataTablesRequest request)
        {
#if DEBUG
            var startTime = DateTime.Now;
#endif
            IEnumerable<StudentDrillDownQuery> results = CacheManager.GetStudentDrilldownData(
                OdsDataService,
                orgType,
                schoolId,
                edOrgId,
                drillDownColumnIndex,
                fourDigitSchoolYear);
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"GetDistrictAncestryRaceCounts: {(DateTime.Now - startTime).Milliseconds}ms");
#endif 

            var sortedResults = results;

            var sortColumn = request.Columns.FirstOrDefault(x => x.Sort != null);
            if (sortColumn != null)
            {
                Func<StudentDrillDownQuery, string> orderingFunctionString = null;
                Func<StudentDrillDownQuery, int?> orderingFunctionNullableInt = null;
                Func<StudentDrillDownQuery, int> orderingFunctionInt = null;
                Func<StudentDrillDownQuery, DateTime?> orderingFunctionNullableDateTime = null;
                
                switch (sortColumn.Name)
                {
                    case "studentId":
                        {
                            orderingFunctionString = x => x.StudentId;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionString)
                                                : results.OrderByDescending(orderingFunctionString);
                            break;
                        }
                    case "studentName":
                        {
                            // watch this implementation detail! name is being stitched together in javascript now --pocky
                            orderingFunctionString = x => $"{x.StudentLastName}, {x.StudentFirstName} {x.StudentMiddleName}";
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionString)
                                                : results.OrderByDescending(orderingFunctionString);
                            break;
                        }
                    case "schoolId":
                        {
                            orderingFunctionNullableInt = x => x.SchoolId;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionNullableInt)
                                                : results.OrderByDescending(orderingFunctionNullableInt);
                            break;
                        }
                    case "schoolName":
                        {
                            orderingFunctionString = x => x.SchoolName;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionString)
                                                : results.OrderByDescending(orderingFunctionString);
                            break;
                        }
                    case "enrolledDate":
                        {
                            orderingFunctionNullableDateTime = x => x.EnrolledDate;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionNullableDateTime)
                                                : results.OrderByDescending(orderingFunctionNullableDateTime);
                            break;
                        }
                    case "withdrawDate":
                        {
                            orderingFunctionNullableDateTime = x => x.WithdrawDate;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionNullableDateTime)
                                                : results.OrderByDescending(orderingFunctionNullableDateTime);
                            break;
                        }
                    case "grade":
                        {
                            orderingFunctionInt = x => int.Parse(x.Grade);
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionInt)
                                                : results.OrderByDescending(orderingFunctionInt);
                            break;
                        }
                    default:
                        {
                            sortedResults = results;
                            break;
                        }
                }
            }

            var pagedResults = sortedResults.Skip(request.Start).Take(request.Length);
            var response = DataTablesResponse.Create(request, results.Count(), results.Count(), pagedResults);
            var jsonResult = new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public JsonResult GetDemographicsReportData(
            int edOrgId,
            string fourDigitSchoolYear,
            bool isStateMode,
            IDataTablesRequest request)
        {
#if DEBUG
            var startTime = DateTime.Now;
#endif
            var results = CacheManager.GetDistrictAncestryRaceCounts(
                OdsDataService,
                isStateMode,
                edOrgId,
                fourDigitSchoolYear);
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"GetDistrictAncestryRaceCounts: {(DateTime.Now - startTime).Milliseconds}ms");
#endif 
            IEnumerable<DemographicsCountReportQuery> sortedResults = results;

            var sortColumn = request.Columns.FirstOrDefault(x => x.Sort != null);
            if (sortColumn != null)
            {
                Func<DemographicsCountReportQuery, string> orderingFunctionString = null;
                Func<DemographicsCountReportQuery, int?> orderingFunctionNullableInt = null;
                Func<DemographicsCountReportQuery, int> orderingFunctionInt = null;

                switch (sortColumn.Field)
                {
                    case "edOrgId":
                        {
                            orderingFunctionNullableInt = x => x.EdOrgId;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionNullableInt)
                                                : results.OrderByDescending(orderingFunctionNullableInt);
                            break;
                        }
                    case "leaSchool":
                        {
                            orderingFunctionString = x => x.LEASchool;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionString)
                                                : results.OrderByDescending(orderingFunctionString);
                            break;
                        }
                    case "enrollmentCount":
                        {
                            orderingFunctionInt = x => x.EnrollmentCount;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionInt)
                                                : results.OrderByDescending(orderingFunctionInt);
                            break;
                        }
                    case "demographicsCount":
                        {
                            orderingFunctionInt = x => x.DemographicsCount;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionInt)
                                                : results.OrderByDescending(orderingFunctionInt);
                            break;
                        }
                    case "raceGivenCount":
                        {
                            orderingFunctionInt = x => x.RaceGivenCount;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionInt)
                                                : results.OrderByDescending(orderingFunctionInt);
                            break;
                        }
                    case "ancestryGivenCount":
                        {
                            orderingFunctionInt = x => x.AncestryGivenCount;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionInt)
                                                : results.OrderByDescending(orderingFunctionInt);
                            break;
                        }
                    default:
                        {
                            sortedResults = results;
                            break;
                        }
                }
            }

            var pagedResults = sortedResults.Skip(request.Start).Take(request.Length);
            var response = DataTablesResponse.Create(request, results.Count(), results.Count(), pagedResults);
            var jsonResult = new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        // GET: Ods/MultipleEnrollmentsReport
        public ActionResult MultipleEnrollmentsReport(bool isStateMode = false, int? districtToDisplay = null, bool isStudentDrillDown = false, int? schoolId = null, int? drillDownColumnIndex = null, OrgType orgType = OrgType.District)
        {
            var session = AppUserService.GetSession();
            var edOrg = EdOrgService.GetEdOrgById(session.FocusedEdOrgId, session.FocusedSchoolYearId);
            var edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            var edOrgId = edOrg.Id;

            // A state user can look at any district via a link, without changing the default district.
            if (districtToDisplay.HasValue && session.UserIdentity.AuthorizedEdOrgs.Select(eorg => eorg.Id).Contains(districtToDisplay.Value))
            {
                edOrgId = districtToDisplay.Value;
                edOrg = EdOrgService.GetEdOrgById(edOrgId, session.FocusedSchoolYearId);
                edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            }
            var fourDigitSchoolYear = SchoolyearService.GetSchoolYearById(session.FocusedSchoolYearId).EndYear;
            var theUser = AppUserService.GetUser();
            if (isStudentDrillDown)
            {
                var studentDrillDownResults = OdsDataService.GetMultipleEnrollmentStudentDrillDown(orgType, schoolId, schoolId ?? edOrgId, drillDownColumnIndex, fourDigitSchoolYear);
                var studentDrillDownModel = new StudentDrillDownViewModel
                {
                    ReportName = "Multiple Enrollment",
                    EdOrgId = edOrgId,
                    EdOrgName = edOrgName,
                    User = theUser,
                    Results = studentDrillDownResults,
                    IsStateMode = isStateMode
                };
                return View("StudentDrillDown", studentDrillDownModel);
            }
            var results = OdsDataService.GetMultipleEnrollmentCounts(isStateMode ? (int?)null : edOrgId, fourDigitSchoolYear);
            var model = new OdsMultipleEnrollmentsReportViewModel
            {
                EdOrgId = edOrgId,
                EdOrgName = edOrgName,
                User = theUser,
                Results = results,
                IsStateMode = isStateMode
            };
            return View(model);
        }

        // GET: Ods/StudentProgramsReport
        public ActionResult StudentProgramsReport(bool isStateMode = false, int? districtToDisplay = null, bool isStudentDrillDown = false, int? schoolId = null, int? drillDownColumnIndex = null, OrgType orgType = OrgType.District)
        {
            var session = AppUserService.GetSession();
            var edOrg = EdOrgService.GetEdOrgById(session.FocusedEdOrgId, session.FocusedSchoolYearId);
            var edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            var edOrgId = edOrg.Id;
            // A state user can look at any district via a link, without changing the default district.
            if (districtToDisplay.HasValue && session.UserIdentity.AuthorizedEdOrgs.Select(eorg => eorg.Id).Contains(districtToDisplay.Value))
            {
                edOrgId = districtToDisplay.Value;
                edOrg = EdOrgService.GetEdOrgById(edOrgId, session.FocusedSchoolYearId);
                edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            }
            var fourDigitSchoolYear = SchoolyearService.GetSchoolYearById(session.FocusedSchoolYearId).EndYear;
            var theUser = AppUserService.GetUser();
            if (isStudentDrillDown)
            {
                var studentDrillDownResults = OdsDataService.GetStudentProgramsStudentDrillDown(orgType, schoolId, schoolId ?? edOrgId, drillDownColumnIndex, fourDigitSchoolYear);
                var studentDrillDownModel = new StudentDrillDownViewModel
                {
                    ReportName = "Student Characteristics and Program Participation",
                    EdOrgId = edOrgId,
                    EdOrgName = edOrgName,
                    User = theUser,
                    Results = studentDrillDownResults,
                    IsStateMode = isStateMode
                };
                return View("StudentDrillDown", studentDrillDownModel);
            }
            var results = OdsDataService.GetStudentProgramsCounts(isStateMode ? (int?)null : edOrgId, fourDigitSchoolYear);
            var model = new OdsStudentProgramsReportViewModel
            {
                EdOrgId = edOrgId,
                EdOrgName = edOrgName,
                User = theUser,
                Results = results,
                IsStateMode = isStateMode
            };
            return View(model);
        }

        // GET: Ods/ChangeOfEnrollmentReport
        public ActionResult ChangeOfEnrollmentReport()
        {
            var session = AppUserService.GetSession();
            var edOrg = EdOrgService.GetEdOrgById(session.FocusedEdOrgId, session.FocusedSchoolYearId);
            var edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            var edOrgId = edOrg.Id;
            var fourDigitSchoolYear = SchoolyearService.GetSchoolYearById(session.FocusedSchoolYearId).EndYear;
            var theUser = AppUserService.GetUser();
            var results = OdsDataService.GetChangeOfEnrollmentReport(edOrgId, fourDigitSchoolYear);
            var model = new OdsChangeOfEnrollmentReportViewModel
            {
                EdOrgId = edOrgId,
                EdOrgName = edOrgName,
                User = theUser,
                Results = results
            };
            return View(model);
        }

        // GET: Ods/ResidentsEnrolledElsewhereReport
        public ActionResult ResidentsEnrolledElsewhereReport(bool isStateMode = false, int? districtToDisplay = null, bool isStudentDrillDown = false)
        {
            var session = AppUserService.GetSession();
            var edOrg = EdOrgService.GetEdOrgById(session.FocusedEdOrgId, session.FocusedSchoolYearId);
            var edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            var edOrgId = edOrg.Id;
            // A state user can look at any district via a link, without changing the default district.
            if (districtToDisplay.HasValue && session.UserIdentity.AuthorizedEdOrgs.Select(eorg => eorg.Id).Contains(districtToDisplay.Value))
            {
                edOrgId = districtToDisplay.Value;
                edOrg = EdOrgService.GetEdOrgById(edOrgId, session.FocusedSchoolYearId);
                edOrgName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            }
            var fourDigitSchoolYear = SchoolyearService.GetSchoolYearById(session.FocusedSchoolYearId).EndYear;
            var theUser = AppUserService.GetUser();
            if (isStudentDrillDown)
            {
                var studentDrillDownResults = OdsDataService.GetResidentsEnrolledElsewhereStudentDrillDown(isStateMode ? (int?)null : edOrgId, fourDigitSchoolYear);
                var studentDrillDownModel = new StudentDrillDownViewModel
                {
                    ReportName = "Residents Enrolled Elsewhere",
                    EdOrgId = edOrgId,
                    EdOrgName = edOrgName,
                    User = theUser,
                    Results = studentDrillDownResults,
                    IsStateMode = isStateMode
                };
                return View("StudentDrillDown", studentDrillDownModel);
            }
            var results = OdsDataService.GetResidentsEnrolledElsewhereReport(isStateMode ? (int?)null : edOrgId, fourDigitSchoolYear);
            var model = new OdsResidentsEnrolledElsewhereReportViewModel
            {
                EdOrgId = edOrgId,
                EdOrgName = edOrgName,
                User = theUser,
                Results = results,
                IsStateMode = isStateMode
            };
            return View(model);
        }

        // GET: Ods/IdentityIssuesReport
        public ActionResult IdentityIssuesReport()
        {
            var model = new OdsIdentityIssuesReportViewModel
            {

            };
            return View(model);
        }

        // GET: Ods/Level1IssuesReport
        public ActionResult Level1IssuesReport()
        {
            var model = new OdsLevel1IssuesReportViewModel
            {

            };
            return View(model);
        }
    }
}

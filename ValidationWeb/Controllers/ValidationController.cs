﻿using Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ValidationWeb.Services;

namespace ValidationWeb
{
    public class ValidationController : Controller
    {
        protected readonly IAppUserService _appUserService;
        protected readonly IEdOrgService _edOrgService;
        protected readonly IRulesEngineService _rulesEngineService;
        protected readonly ISchoolYearService _schoolYearService;
        protected readonly IValidationResultsService _validationResultsService;
        protected readonly Model _engineObjectModel;

        public ValidationController(
            IAppUserService appUserService,
            IEdOrgService edOrgService,
            IValidationResultsService validationResultsService,
            IRulesEngineService rulesEngineService,
            ISchoolYearService schoolYearService,
            Model engineObjectModel)
        {
            _appUserService = appUserService;
            _edOrgService = edOrgService;
            _engineObjectModel = engineObjectModel;
            _rulesEngineService = rulesEngineService;
            _schoolYearService = schoolYearService;
            _validationResultsService = validationResultsService;
        }

        // GET: Validation/Reports
        public ActionResult Reports()
        {
            var edOrg = _edOrgService.GetEdOrgById(_appUserService.GetSession().FocusedEdOrgId);
            var rulesCollections = _engineObjectModel.Collections.OrderBy(x => x.CollectionId).ToList();
            var theUser = _appUserService.GetUser();
            var districtName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;
            var reportSummaries = (edOrg == null) ? Enumerable.Empty<ValidationReportSummary>().ToList() : _validationResultsService.GetValidationReportSummaries(edOrg.Id);
            var model = new ValidationReportsViewModel
            {
                DistrictName = districtName,
                TheUser = theUser,
                ReportSummaries = reportSummaries,
                RulesCollections = rulesCollections,
                SchoolYears = _schoolYearService.GetSubmittableSchoolYears().ToList()
            };
            return View(model);
        }

        public ActionResult Report(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Reports");
            }

            var model = new ValidationReportDetailsViewModel { Details = _validationResultsService.GetValidationReportDetails(id) };
            return View(model);
        }

        public ActionResult RunEngine(string collectionId, string schoolYear)
        {
            // TODO: Validate the user's access to district, action, school year
            // Kick off Validation
            ValidationReportSummary summary = _rulesEngineService.RunEngine(schoolYear, collectionId);
            return Json(summary);
        }


        // GET: Validation/Reports
        public ActionResult Submissions()
        {
            return View();
        }

    }
}
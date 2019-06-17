﻿using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ValidationWeb.Models;
using ValidationWeb.Services.Interfaces;
using ValidationWeb.Utility;
using ValidationWeb.ViewModels;

namespace ValidationWeb.Controllers
{
    public class DynamicController : Controller
    {
        public DynamicController(
            IDynamicReportingService dynamicReportingService,
            IAppUserService appUserService,
            IEdOrgService edOrgService,
            ISchoolYearService schoolYearService)
        {
            DynamicReportingService = dynamicReportingService;
            AppUserService = appUserService;
            EdOrgService = edOrgService;
            SchoolYearService = schoolYearService;
        }

        protected IDynamicReportingService DynamicReportingService { get; set; }

        protected IAppUserService AppUserService { get; set; }

        protected IEdOrgService EdOrgService { get; set; }

        protected ISchoolYearService SchoolYearService { get; set; }

        // GET: Dynamic
        public ActionResult Index()
        {
            var focusedSchoolYearId = AppUserService.GetSession().FocusedSchoolYearId;
            var focusedSchoolYear = SchoolYearService.GetSchoolYearById(focusedSchoolYearId);

            var focusedEdOrg = EdOrgService.GetEdOrgById(
                AppUserService.GetSession().FocusedEdOrgId,
                focusedSchoolYear.Id);

            var viewModel = new DynamicReportViewModel
            {
                FocusedEdOrg = focusedEdOrg,
                SchoolYear = focusedSchoolYear
            };

            return View(viewModel);
        }

        public ActionResult GetReportDefinitions(int schoolYearId)
        {
            var result = DynamicReportingService.GetReportDefinitions()
                .Where(x => x.SchoolYearId == schoolYearId)
                .OrderBy(x => x.Name);

            var serializedResult = JsonConvert.SerializeObject(
                result,
                Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            return Content(serializedResult, "application/json");
        }

        public ActionResult GenerateReport(DynamicReportRequest request)
        {
            var report = DynamicReportingService.GetReportData(request);
            var csvArray = Csv.WriteCsvToMemory(report);
            var memoryStream = new MemoryStream(csvArray);

            return new FileStreamResult(memoryStream, "text/csv");
        }

    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationWeb.Services
{
    public interface IEdOrgService
    {
        List<EdOrg> GetEdOrgs();

        EdOrg GetEdOrgById(int edOrgId, int fourDigitOdsDbYear);

        void RefreshEdOrgCache(int fourDigitOdsDbYear);

        SingleEdOrgByIdQuery GetSingleEdOrg(int edOrgId, int schoolYearId);
    }
}

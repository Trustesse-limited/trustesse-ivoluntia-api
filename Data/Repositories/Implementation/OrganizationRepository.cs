using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class OrganizationRepository: GenericRepository<Foundation>,IOrganizationRepository
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext;
        public OrganizationRepository(iVoluntiaDataContext iVoluntiaDataContext):base(iVoluntiaDataContext)       
        {
            _iVoluntiaDataContext = iVoluntiaDataContext;
        }
    }
}

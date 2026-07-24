using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class OtpRepo: GenericRepository<Otp>, IOtpRepo
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext;
        public OtpRepo(iVoluntiaDataContext iVoluntiaDataContext):base(iVoluntiaDataContext)
        {
            _iVoluntiaDataContext = iVoluntiaDataContext;
        }
    }
}

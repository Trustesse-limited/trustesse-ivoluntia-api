using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Domain.Entities;


namespace Trustesse.Ivoluntia.Data.DataContext
{
    
    public class iVoluntiaDataContext(DbContextOptions<iVoluntiaDataContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.DataContext
{
    public class SampleContext(DbContextOptions<SampleContext> options) : DbContext (options)
    {
        public DbSet<Clients> Clients { get; set; }
    }
}

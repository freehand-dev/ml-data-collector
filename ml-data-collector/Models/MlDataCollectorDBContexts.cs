using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ml_data_collector.Models
{
    public class MlDataCollectorDBContexts : DbContext
    {
        public DbSet<MlDataCollector> MlDataCollector { get; set; }

        public MlDataCollectorDBContexts(DbContextOptions<MlDataCollectorDBContexts> options)
            : base(options)
        {
            Database.EnsureCreated();
            Database.Migrate();
        }

        public MlDataCollectorDBContexts()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite(@"Data Source=ml-data-collector.db");
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace data_base_practicum
{
    public class AppContextTop10 : DbContext
    {
        public DbSet<MovieTop10> Top10 => Set<MovieTop10>();

        public AppContextTop10()
        {
            Database.EnsureCreated();
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=top10_movies.db");
            optionsBuilder.EnableSensitiveDataLogging();

            
        }
    }
}

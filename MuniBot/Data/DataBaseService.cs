using Microsoft.EntityFrameworkCore;
using MuniBot.Common.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuniBot.Data
{
    public class DataBaseService:DbContext,IDataBaseService
    {
        public DataBaseService(DbContextOptions options):base(options)
        {
            Database.EnsureCreatedAsync();
        }

        public DataBaseService()
        {
            Database.EnsureCreated();
        }

        public DbSet<UserModel> User { get; set; }
        public async Task<bool> SaveAsync()
        {
            return (await SaveChangesAsync()>0);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToContainer("User").HasPartitionKey("channel").HasNoDiscriminator().HasKey("id");

        }
    }
}

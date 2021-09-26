using Microsoft.EntityFrameworkCore;
using Server.Crypt;
using Server.Helpers;
using System;

namespace Server.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=usersdata.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            byte[] salt1 = Hash.GenerateSalt();
            byte[] salt2 = Hash.GenerateSalt();
            byte[] salt3 = Hash.GenerateSalt();

            modelBuilder.Entity<User>().HasData(
                new User[]
                {
                new User { Id=1, UserName="Adm1", Email = "Adm1E", Password = HashManager.GenerateHash("123", salt1), 
                    Salt = salt1, PrivateID = Guid.NewGuid().ToString()},
                new User { Id=2, UserName="Adm2", Email = "Adm2E", Password = HashManager.GenerateHash("123", salt2), 
                    Salt = salt2, PrivateID = Guid.NewGuid().ToString()},
                new User { Id=3, UserName="Adm3", Email = "Adm3E", Password = HashManager.GenerateHash("123", salt3),
                    Salt = salt3, PrivateID = Guid.NewGuid().ToString()}
                });
        }

    }
}
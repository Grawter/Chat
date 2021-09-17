﻿using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }

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
            modelBuilder.Entity<User>().HasData(
                new User[]
                {
                new User { Id=1, UserName="Adm1", Email = "Adm1E", Password = "123"},
                new User { Id=2, UserName="Adm2", Email = "Adm2E", Password = "123"},
                new User { Id=3, UserName="Adm3", Email = "Adm3E", Password = "123"}
                });
        }

    }
}
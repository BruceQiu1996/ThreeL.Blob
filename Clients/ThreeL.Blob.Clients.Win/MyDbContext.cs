﻿using Microsoft.EntityFrameworkCore;
using ThreeL.Blob.Clients.Win.Entities;

namespace ThreeL.Blob.Clients.Win
{
    public class MyDbContext : DbContext
    {
        public DbSet<UploadFileRecord> UploadFileRecords { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>();
            modelBuilder.Entity<UploadFileRecord>();
        }
    }
}

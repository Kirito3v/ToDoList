using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

namespace ToDoList.Data
{
    public class ToDoDataContext : DbContext
    {
        public ToDoDataContext(DbContextOptions<ToDoDataContext> options) : base(options)
        {
        }
        public DbSet<UserData> Users { get; set; }
        public DbSet<UserModel> AuthUsers { get; set; }
        public DbSet<ToDoItemModel> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
                .HasOne(au => au.UserData)
                .WithOne()
                .HasForeignKey<UserModel>(au => au.UserId);
        }
    }
}

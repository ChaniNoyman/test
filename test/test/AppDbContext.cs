namespace test
{
    using Microsoft.EntityFrameworkCore;
    using test.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<BookModel> Books { get; set; }
        public DbSet<ReviewModel> Reviews { get; set; }
    }
}

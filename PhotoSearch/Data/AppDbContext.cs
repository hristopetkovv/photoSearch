namespace PhotoSearch.Data
{
	public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
	{
		public DbSet<ImageEntry> ImageEntries { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasPostgresExtension("vector");

			modelBuilder.Entity<ImageEntry>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Embedding).HasColumnType("vector(512)");
			});

			base.OnModelCreating(modelBuilder);
		}
	}
}

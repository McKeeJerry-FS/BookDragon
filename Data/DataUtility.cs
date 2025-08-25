using BookDragon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookDragon.Data
{
    public class DataUtility
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            // First check for Railway's DATABASE_URL
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                return BuildConnectionString(databaseUrl);
            }

            // Check for other Railway environment variables
            var railwayHost = Environment.GetEnvironmentVariable("PGHOST");
            var railwayPort = Environment.GetEnvironmentVariable("PGPORT");
            var railwayUser = Environment.GetEnvironmentVariable("PGUSER");
            var railwayPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
            var railwayDatabase = Environment.GetEnvironmentVariable("PGDATABASE");

            if (!string.IsNullOrEmpty(railwayHost) && !string.IsNullOrEmpty(railwayDatabase))
            {
                return BuildConnectionStringFromParts(railwayHost, railwayPort, railwayUser, railwayPassword, railwayDatabase);
            }

            // Fallback to configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            throw new InvalidOperationException("No database connection string found. Please set DATABASE_URL environment variable or configure DefaultConnection.");
        }

        private static string BuildConnectionString(string databaseUrl)
        {
            //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        private static string BuildConnectionStringFromParts(string host, string port, string user, string password, string database)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = !string.IsNullOrEmpty(port) ? int.Parse(port) : 5432,
                Username = user,
                Password = password,
                Database = database,
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IServiceProvider serviceProvider)
        {
            var dbContextSvc = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManagerSvc = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var configurationSvc = serviceProvider.GetRequiredService<IConfiguration>();

            // align the database by checking the migrations
            await dbContextSvc.Database.MigrateAsync();

            // Seed Categories if none exist
            if (!await dbContextSvc.Categories.AnyAsync())
            {
                var seedCategories = new List<Category>
                {
                    new() { Name = "Fantasy", Description = "Magical worlds and epic quests" },
                    new() { Name = "Science Fiction", Description = "Futuristic tech and space adventures" },
                    new() { Name = "Mystery", Description = "Whodunits and detective tales" },
                    new() { Name = "Thriller", Description = "Edge-of-your-seat suspense" },
                    new() { Name = "Romance", Description = "Love stories and relationships" },
                    new() { Name = "Historical", Description = "Stories set in the past" },
                    new() { Name = "Horror", Description = "Frightening and supernatural" },
                    new() { Name = "Non-Fiction", Description = "Real events and factual works" },
                    new() { Name = "Biography", Description = "Life stories of notable people" },
                    new() { Name = "Young Adult", Description = "Fiction aimed at teen readers" }
                };

                dbContextSvc.Categories.AddRange(seedCategories);
                await dbContextSvc.SaveChangesAsync();
            }
        }
    }
}

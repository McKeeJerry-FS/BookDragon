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
            // Railway/Heroku style URL (either key)
            var url =
                Environment.GetEnvironmentVariable("DATABASE_URL") ??
                Environment.GetEnvironmentVariable("RAILWAY_DATABASE_URL");

            if (!string.IsNullOrEmpty(url))
            {
                return BuildConnectionString(url);
            }

            // PG* individual variables
            var railwayHost = Environment.GetEnvironmentVariable("PGHOST");
            var railwayPort = Environment.GetEnvironmentVariable("PGPORT");
            var railwayUser = Environment.GetEnvironmentVariable("PGUSER");
            var railwayPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
            var railwayDatabase = Environment.GetEnvironmentVariable("PGDATABASE");

            if (!string.IsNullOrEmpty(railwayHost) && !string.IsNullOrEmpty(railwayDatabase))
            {
                return BuildConnectionStringFromParts(railwayHost, railwayPort, railwayUser, railwayPassword, railwayDatabase);
            }

            // Fallback to configuration (appsettings or env ConnectionStrings__DefaultConnection)
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            throw new InvalidOperationException("No database connection string found. Set DATABASE_URL / RAILWAY_DATABASE_URL or PG* vars, or ConnectionStrings:DefaultConnection.");
        }

        private static string BuildConnectionString(string databaseUrl)
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2); // in case password contains ':'

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = userInfo.Length > 0 ? userInfo[0] : "",
                Password = userInfo.Length > 1 ? userInfo[1] : "",
                Database = uri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };

            // If querystring includes sslmode require, honor it
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (Enum.TryParse<SslMode>(query.Get("sslmode"), true, out var ssl))
            {
                builder.SslMode = ssl;
            }

            return builder.ToString();
        }

        private static string BuildConnectionStringFromParts(string host, string? port, string? user, string? password, string database)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = int.TryParse(port, out var p) ? p : 5432,
                Username = user ?? "",
                Password = password ?? "",
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

            await dbContextSvc.Database.MigrateAsync();

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

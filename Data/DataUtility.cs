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
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
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

        public static async Task ManageDataAsync(IServiceProvider serviceProvider)
        {
            var dbContextSvc = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManagerSvc = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var configurationSvc = serviceProvider.GetRequiredService<IConfiguration>();
            var roleManagerSvc = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // align the database by checking the migrations
            await dbContextSvc.Database.MigrateAsync();

            
            
        }
    }
}

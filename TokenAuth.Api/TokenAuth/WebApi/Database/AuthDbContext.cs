using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Rugal.TokenAuth.WebApi.Database
{
    public class AuthDbContext : IdentityDbContext<IdentityUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
        private void InitDatabase()
        {
            var GetInitSql = Database.GenerateCreateScript();
            //Input the sql command to SSMS 
        }
    }
}

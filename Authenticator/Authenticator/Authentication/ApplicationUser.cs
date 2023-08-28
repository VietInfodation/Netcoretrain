using Microsoft.AspNetCore.Identity;
namespace Authenticator.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        public double BalanceAccount { get; set; }
    }
}

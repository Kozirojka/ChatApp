using Microsoft.AspNetCore.Identity;

namespace ChatApp.Domain;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
}
using OidcServer.Models;

namespace OidcServer.Repositories
{
    public interface IUserRepository
    {
        User? FindByUserName(string user);
    }
}

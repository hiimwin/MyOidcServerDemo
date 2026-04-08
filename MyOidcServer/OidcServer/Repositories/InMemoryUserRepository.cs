using OidcServer.Models;

namespace OidcServer.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List <User> _user = [
            new () { Name = "Alice" },
            new () { Name = "Bob" },
            new () { Name = "Charlie" },
            new () { Name = "Win" }
        ];

        public User? FindByUserName(string userName)
        {
            return _user.FirstOrDefault(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }
    }
}

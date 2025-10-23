using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Data
{
    public interface IUserRepository
    {
        User? FindByEmail(string email);
        User Save(User user);
    }
}
namespace WebReckrytingSystem.Model
{
    public interface IUserRepository
    {
        User FindByEmail(string email);
        User Save(User user);
    }
}

using I72_Backend.Model;

namespace I72_Backend.Interfaces
{
    public interface IUserRepository
    {
     ICollection<User> GetUsers();
    }
}

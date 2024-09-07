using I72_Backend.Data;
using I72_Backend.Interfaces;
using I72_Backend.Model;
namespace I72_Backend.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DB_Context _context;
        public UserRepository(DB_Context context) 
        {
        
            this._context = context;
        
        }

        public ICollection<User> GetUsers()
        {
            return _context.Users.OrderBy(p => p.Id).ToList();
        
        }

    }
}

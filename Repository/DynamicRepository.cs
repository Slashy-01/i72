using I72_Backend.Interfaces;
using I72_Backend.Model;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;
using I72_Backend.Data;

namespace I72_Backend.Repository
{
    public class DynamicRepository : IDynamicRepository
	{
        private readonly DB_Context _context;

        public DynamicRepository(DB_Context context)
        {
            _context = context;
        }

		public ICollection<Dynamic> GetDynamic()
        {
            return _context.Dynamic.OrderBy(p => p.Id).ToList();
        }

        public Dynamic GetUserByDynamic(string name)
        {
            return _context.Dynamic.SingleOrDefault(u => u.Name == name);
        }

        public Dynamic GetDynamicById(int id)
        {
            return _context.Dynamic.Find(id);
        }

        public void AddDynamic(Dynamic dynamic)
        {
            _context.Dynamic.Add(dynamic);
            _context.SaveChanges();
        }

        public void DeleteDynamic(Dynamic dynamic)
        {
            _context.Dynamic.Remove(dynamic);
            _context.SaveChanges();
        }
        public void UpdateDynamic(Dynamic dynamic)
        {
            _context.Dynamic.Update(dynamic);
            _context.SaveChanges();
        }
    }
}


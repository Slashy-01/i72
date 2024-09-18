using Microsoft.EntityFrameworkCore;

namespace I72_Backend.Model
{
	public class Dynamic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public string Branch { get; set; }
	}

}

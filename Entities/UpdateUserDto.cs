namespace I72_Backend.Model
{
    public class UpdateUserDto
    {
        public int Id { get; set; } //ID to identify the user
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
    }
}
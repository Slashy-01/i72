namespace I72_Backend.Model
{

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string? RefreshToken { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
    }




    public class UserDetails
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
    }


    public class UserRegister
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
    }





    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TokenRefreshRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

}

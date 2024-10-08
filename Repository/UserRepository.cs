﻿using I72_Backend.Interfaces;
using I72_Backend.Model;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;
using I72_Backend.Data;

namespace I72_Backend.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DB_Context _context;

        public UserRepository(DB_Context context)
        {
            _context = context;
        }

        public ICollection<User> GetUsers()
        {
            return _context.Users.OrderBy(p => p.Id).ToList();
        }
        public ICollection<User> GetUserList()
        {
            return _context.Users.OrderBy(p => p.Id).ToList();
        }
        public ICollection<User> GetUsersPaginated(int page, int pageSize)
        {
            // Calculate the number of users to skip based on the page and page size
            int skip = (page - 1) * pageSize;

            // Fetch the users with pagination applied
            return _context.Users
                .OrderBy(u => u.Id)
                .Skip(skip)          // Skip the number of records based on the current page
                .Take(pageSize)      // Take only the pageSize number of records
                .ToList();
        }
        public User GetUserByUsername(string username)
        {
            return _context.Users.SingleOrDefault(u => u.Username == username);

        }

        public bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
        }

        public bool Login(string username, string plainTextPassword)
        {
            var user = GetUserByUsername(username);
            if (user == null || !VerifyPassword(plainTextPassword, user.Password))
            {
                return false; 
            }

            return true; 
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetUserById(int id)
        {
            return _context.Users.Find(id);
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        
        public void UpdateUserDetails(User user)
        {
            var existingUser = GetUserById(user.Id);
            if (existingUser != null)
            {
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Phone = user.Phone;
                existingUser.Role = user.Role;
                _context.Users.Update(existingUser);
                _context.SaveChanges();
            }
        }
        
        public void SetUserRefreshToken(string username, string refreshToken)
        {
            var user = GetUserByUsername(username);
            if (user != null)
            {
                user.RefreshToken = refreshToken; 
                UpdateUser(user);
            }
        }

    }
}


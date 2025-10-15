using BMSAPI.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BMSAPI.Services.Authentication
{
    public class AuthenticationServices : IAuthentication
    {
        private readonly string _filePath = "D:/Angular/BMS/BMSAPI/BMSAPI/Data/UsersData.json";
        private readonly string secretKey = "JIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI";
        public List<User> ReadUsersFromFile()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException("users.json file not found.");

            var jsonContent = File.ReadAllText(_filePath);
            var userListWrapper = JsonConvert.DeserializeObject<UserListWrapper>(jsonContent);
            return userListWrapper?.Users ?? new List<User>();
        }

        // Helper method to save users to the file
        public void SaveUsersToFile(List<User> users)
        {
            var userListWrapper = new UserListWrapper { Users = users };
            var jsonContent = JsonConvert.SerializeObject(userListWrapper, Formatting.Indented);
            File.WriteAllText(_filePath, jsonContent);
        }

        // Get all users
        public List<User> GetAllUsers()
        {
            return ReadUsersFromFile();
        }

        // Get a user by ID
        public User GetUserById(long id)
        {
            var users = ReadUsersFromFile();
            return users.FirstOrDefault(u => u.Id == id);
        }

        // Add a new user
        public User AddUser(AddUser user)
        {
            var users = ReadUsersFromFile();

            // Check if the email already exists
            if (users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                // Email already exists
                return null;
            }
            long userId = users.Select(u => u.Id).DefaultIfEmpty(0).Max();
            User NewUser = new User()
            {
                Id= userId+1,
                Email = user.Email,
                Name = user.Name,
                Password = user.Password,
            };
            // Generate new ID for the user (assuming no gaps in IDs)
            users.Add(NewUser);
            SaveUsersToFile(users);
            return NewUser;
        }

        public User EditUser(long id, UserUpdate updatedUser, string fileName)
        {
            var users = ReadUsersFromFile();
            var user = users.FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                // Prevent updating ID or Email
                user.Name = updatedUser.Name; // Allow changing the Name and Password
                user.Password = updatedUser.Password; // Allow changing the Password
                user.Bio = updatedUser.Bio;
                user.ProfileImage = fileName;
                SaveUsersToFile(users);
            }

            return user;
        }
        // Delete a user by ID
        public bool DeleteUser(long id)
        {
            var users = ReadUsersFromFile();
            var user = users.FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                users.Remove(user);
                SaveUsersToFile(users);
                return true;
            }

            return false;
        }

        List<User> IAuthentication.ReadUsersFromFile()
        {
            throw new NotImplementedException();
        }

        public string Login(string email, string password)
        {
            string Token=string.Empty;
            var users = ReadUsersFromFile();
            var GetDetails= users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.Password.ToLower()== password.ToLower());
            if(GetDetails != null)
            {
                Token = GenerateToken(GetDetails);
                return Token;
            }
            else
            {
                return null;
            }
           
        }
        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim("Name", user.Name),
            new Claim("NameIdentifier", user.Id.ToString()),
            new Claim("Email", user.Email)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "BMSApp",
                audience: "BMSApp",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

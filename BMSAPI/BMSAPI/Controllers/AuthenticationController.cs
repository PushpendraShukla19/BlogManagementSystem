using BMSAPI.Models;
using BMSAPI.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BMSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthentication _authentication;
        public AuthenticationController(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = _authentication.GetAllUsers();
                return Ok(new { error = false, data = users, message = "Record fetch successfully.", code = 200, status = true });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = true, data = "null", message = "Record not found.", code = 500, status = false });
            }
            catch (Exception ex)
            {
                return Ok(new { error = true, data = "null", message = ex.Message, code = 500, status = false });
            }
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public IActionResult GetUserById(long id)
        {
            var user = _authentication.GetUserById(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return Ok(user);
        }

        // POST: api/user
        [HttpPost("register")]
        public IActionResult AddUser([FromBody] AddUser user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Registration failed.", code = 500, status = false });
            }
            if (string.IsNullOrEmpty(user.Name) || user.Name == "")
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Name is required.", code = 500, status = false });
            }
            else if(!IsValidEmail(user.Email) || user.Email == "")
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Invalid email", code = 500, status = false });
            }
            else if (!ModelState.IsValid || string.IsNullOrEmpty(user.Password) || user.Password == "")
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Password is required.", code = 500, status = false });
            }
            
            if (!IsValidPassword(user.Password) || user.Password == "")
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Invalid password. Ensure your password is at least 8 characters long and includes a mix of uppercase, lowercase, digits, and special characters.", code = 500, status = false });
            }

            // Add user via service
            var createdUser = _authentication.AddUser(user);

            if (createdUser == null)
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Email is already taken.", code = 500, status = false });

            }
            var response = CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            return Ok(new { error = false, data = response, message = "Register successfully.", code = 200, status = true });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditUser(long id, [FromForm] UserUpdate user, IFormFile ProfileImages)
        {
            if (!ModelState.IsValid && !string.IsNullOrEmpty(user.Name))
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Name is required.", code = 500, status = false });
            }
            else if (!ModelState.IsValid && !string.IsNullOrEmpty(user.Password))
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Password is required.", code = 500, status = false });
            }
            if (!IsValidPassword(user.Password))
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Invalid password. Ensure your password is at least 8 characters long and includes a mix of uppercase, lowercase, digits, and special characters.", code = 500, status = false });
            }

            var Users = _authentication.GetUserById(id);
            if (Users == null)
            {
                return NotFound("User not found.");
            }

            if (Users.ProfileImage != null && Users.ProfileImage.Count() > 0)
            {
                DeleteImage(Users.ProfileImage);
            }


            var fileName=string.Empty;
            if (ProfileImages != null && ProfileImages.Length > 0)
            {
                if (ProfileImages.Length > 0)
                {
                    // Generate a unique timestamp for each image
                    string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"); // Use the current UTC time with milliseconds to ensure uniqueness

                    // Generate the file name with the blog slug and timestamp
                    fileName = $"{user.Name.ToLower()}_{timestamp}{Path.GetExtension(ProfileImages.FileName)}";  // Adding extension from original file name

                    // Define the file path where the image will be saved
                    var filePath = Path.Combine("images/profile", fileName);

                    // Save the file to the local folder
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImages.CopyToAsync(stream);
                    }
                }
            }
            var updatedUser = _authentication.EditUser(id, user, fileName);
            if (updatedUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return Ok(new { error = false, data = updatedUser, message = "User updated successfully.", code = 200, status = true });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(long id)
        {
            var Users = _authentication.GetUserById(id);
            if (Users == null)
            {
                return NotFound("User not found.");
            }

            if (Users.ProfileImage != null && Users.ProfileImage.Count() > 0)
            {
                DeleteImage(Users.ProfileImage);
            }

            var success = _authentication.DeleteUser(id);
            if (!success)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return NoContent(); // Return a 204 status code for successful deletion
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUser request)
        {
            var user = _authentication.Login(request.Email, request.Password);
            if (user == null)
            {
                return BadRequest(new { error = true, data = string.Empty, message = "Invalid username or password", code = 500, status = false });
            }
            return Ok(new { error = false, data = user, message = "User Login successfully.", code = 200, status = true });
        }

        public static bool IsValidPassword(string password)
        {
            // Define the regex pattern for a strong password
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";

            // Use Regex.IsMatch to validate the password
            return Regex.IsMatch(password, passwordPattern);
        }

        public static bool IsValidEmail(string email)
        {
            // Define the regex pattern for a valid email address
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Use Regex.IsMatch to validate the email
            return Regex.IsMatch(email, emailPattern);
        }


        private void DeleteImage(string imagePath)
        {
            try
            {
                var filePath = Path.Combine("images/profile", imagePath);
                // Ensure the path is valid and the file exists
                if (System.IO.File.Exists(filePath))
                {
                    // Delete the file
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting image {imagePath}: {ex.Message}");
            }
        }
    }
}

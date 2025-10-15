using BMSAPI.Models;
using BMSAPI.Services.Authentication;
using BMSAPI.Services.Blogs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace BMSAPI.Controllers
{
    [ApiController]
    [Route("api/blog")]
    public class BlogController : BaseController
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        // GET: api/blog
        [HttpGet]
        public IActionResult GetAllBlogs()
        {
            var blogs = _blogService.ReadBlogFromFile();
            return Ok(new { error = false, data = blogs, message = "Records fetched successfully.", code = 200, status = true });
        }

        // GET: api/blog/{id}
        [HttpGet("{id}")]
        public IActionResult GetBlogById(int id)
        {
            var blog = _blogService.GetBlogById(id);
            if (blog == null)
            {
                return NotFound($"Blog with ID {id} not found.");
            }
            return Ok(new { error = false, data = blog, message = "Records fetched successfully.", code = 200, status = true });
        }

        [HttpPost("Addblog")]
        [Authorize]
        public async Task<IActionResult> CreateBlog([FromForm] AddBlog blogPost, [FromForm] IFormFile[] BlogImages)
        {
            // Extract the token from the Authorization header
            string token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing.");
            }

            // Initialize JwtSecurityTokenHandler to read the JWT token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract claims from the token
            var claims = jwtToken.Claims;

            // Get user details (e.g., username, user ID, etc.)
            var username = claims.FirstOrDefault(c => c.Type == "Name")?.Value; // Assuming the token contains a claim for the username
            var userId = claims.FirstOrDefault(c => c.Type == "NameIdentifier")?.Value; // Assuming the token contains a claim for user ID
            var email = claims.FirstOrDefault(c => c.Type == "Email")?.Value; // Assuming the token contains a claim for user ID

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid token or missing user details.");
            }


            // Check if token is present
            if (string.IsNullOrEmpty(token))
            {
                // Handle the case when token is not provided or is invalid
                return Unauthorized("Token is missing.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = true, message = "Invalid data", code = 400, status = false });
            }
            string BlogSlug = _blogService.GenerateSlug(blogPost.Title.ToLower());  // Assuming GenerateSlug is a method to create slug from title
            bool SlugExist =_blogService.IsSlugExist(BlogSlug);
            if (SlugExist) {
                return BadRequest(new { error = true, message = "Title is aleady exist", code = 400, status = false });
            }
            // Handle file uploads
            List<string> imageUrls = new List<string>();
            List<string> UploadedFiles = new List<string>();
            if (BlogImages != null && BlogImages.Length > 0)
            {
                foreach (var file in BlogImages)
                {
                    if (file.Length > 0)
                    {
                        // Generate a unique timestamp for each image
                        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"); // Use the current UTC time with milliseconds to ensure uniqueness

                        // Generate the file name with the blog slug and timestamp
                        var fileName = $"{BlogSlug}_{timestamp}{Path.GetExtension(file.FileName)}";  // Adding extension from original file name

                        // Define the file path where the image will be saved
                        var filePath = Path.Combine("images/blogs", fileName);

                        // Save the file to the local folder
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        // Add the saved file path to the list of image URLs (or URL if using cloud storage)
                        imageUrls.Add(filePath);
                        UploadedFiles.Add(fileName);
                    }
                }
            }
            var createdBlog = _blogService.AddBlog(blogPost, UploadedFiles.ToArray(), BlogSlug, userId);
            var response = CreatedAtAction(nameof(GetBlogById), new { id = createdBlog.Id }, createdBlog);
            return Ok(new { error = false, data = response, message = "Blog post successfully.", code = 200, status = true });
        }


        [HttpPut("Updateblog/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateBlog(string id, [FromForm] UpdateBlog blogPost, [FromForm] IFormFile[] BlogImages)
        {
            var blog = _blogService.GetBlogById(Convert.ToInt64(id));
            if (blog == null)
            {
                return NotFound("Blog not found.");
            }

            // Process uploaded images (similar to your existing API logic)
            List<string> uploadedImages = new List<string>();
            if (BlogImages != null && BlogImages.Length > 0)
            {
                foreach (var file in BlogImages)
                {
                    if (file.Length > 0)
                    {
                        string fileName = $"{blog.Slug}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine("images/blogs", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        uploadedImages.Add(fileName);
                    }
                }
            }

            // Call your service to update the blog
            var updatedBlog = _blogService.UpdateBlog(Convert.ToInt64(id), blogPost, uploadedImages.ToArray());

            return Ok(new { error = false, data = updatedBlog, message = "Blog updated successfully.", code = 200, status = true });
        }


        // PUT: api/blog/{id}/status
        [HttpPut("{id}/status")]
        public IActionResult ChangeBlogStatus(long id, bool status)
        {
            var blog = _blogService.ChangeBlogStatus(id, status);
            if (!blog)
            {
                return NotFound($"Blog with ID {id} not found.");
            }
            return Ok(new { error = false, data = blog, message = "Blog status updated successfully.", code = 200, status = true });
        }

        // DELETE: api/blog/{id}
        [HttpDelete("DeleteBlog/{id}")]
        [Authorize]
        public IActionResult DeleteBlog(int id)
        {
            var blogToDelete = _blogService.GetBlogById(id);
            if (blogToDelete == null)
            {
                return NotFound("Blog not found.");
            }

            if (blogToDelete.FeaturedImage != null && blogToDelete.FeaturedImage.Count() > 0)
            {
                // Loop through each image associated with the blog and delete it
                foreach (var imagePath in blogToDelete.FeaturedImage)
                {
                    DeleteImage(imagePath);
                }
            }
            var success = _blogService.DeleteBlog(id);
            if (!success)
            {
                return NotFound($"Blog with ID {id} not found.");
            }

            return NoContent();
        }

        private void DeleteImage(string imagePath)
        {
            try
            {
                var filePath = Path.Combine("images/blogs", imagePath);
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
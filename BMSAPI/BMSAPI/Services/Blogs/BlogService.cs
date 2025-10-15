using BMSAPI.Models;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authentication;
using BMSAPI.Services.Authentication;

namespace BMSAPI.Services.Blogs
{
    public class BlogService:IBlogService
    {
        private readonly string FilePath = "D:/Angular/BMS/BMSAPI/BMSAPI/Data/Blogs.json";
        private readonly IAuthentication _authenticationService;
        public BlogService(IAuthentication authenticationService)
        {
                this._authenticationService = authenticationService;
        }
        public List<Blog> ReadBlogFromFile()
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("blogs.json file not found.");

            var jsonContent = File.ReadAllText(FilePath);
            var userListWrapper = JsonConvert.DeserializeObject<List<Blog>>(jsonContent);
            return userListWrapper;
        }
             

        public Blog GetBlogById(long id)
        {
            var blogs = ReadBlogFromFile();
            return blogs.FirstOrDefault(b => b.Id == id);
        }

        public bool IsSlugExist(string slug)
        {
            var blogs = ReadBlogFromFile();  // Assuming this reads all blog data from file
                                             // Check if the slug exists (case-insensitive comparison)
            return blogs.Any(b => b.Slug.ToLower() == slug.ToLower());
        }


        public Blog AddBlog(AddBlog blog, string[] imageUrls, string BlogSlug, string userId)
        {
            var blogs = ReadBlogFromFile();
            long maxId = blogs.Select(u => u.Id).DefaultIfEmpty(0).Max();

           var userDetails = _authenticationService.GetUserById(Convert.ToInt32(userId));

            Blog newblog = new Blog()
            {
                Id = maxId + 1,
                Title = blog.Title,
                Slug = BlogSlug,
                Author= new Author()
                {
                    Id= userDetails.Id,
                    Name= userDetails.Name,
                    Bio= userDetails.Bio,
                    ProfilePicture= userDetails.ProfileImage,  
                },
                Date= DateTime.Now,
                Content= new Content()
                {
                    Intro = blog.Introduction,
                    Body=blog.Content
                },
                Tags= blog.Tags,
                FeaturedImage= imageUrls.ToArray(),
                Category= blog.Category,
                MetaTags= new Meta()
                {
                    Description= blog.Descriptions,
                    Keywords=blog.Keywords,
                },
                Status= blog.Status
            };
            blogs.Add(newblog);
            SaveBlogs(blogs);
            return newblog;
        }

        public Blog UpdateBlog(long id, UpdateBlog updatedBlog, string[] imageUrls)
        {
            var blogs = ReadBlogFromFile();
            var existingBlog = blogs.FirstOrDefault(b => b.Id == id);
            if (existingBlog != null)
            {              
                existingBlog.Content = new Content()
                {
                    Intro= updatedBlog.Introduction,
                    Body=updatedBlog.Content
                };
                existingBlog.Tags = updatedBlog.Tags;
                existingBlog.FeaturedImage = imageUrls.ToArray().Count() > 0 ? imageUrls.ToArray(): existingBlog.FeaturedImage.ToArray();
                existingBlog.Status = updatedBlog.Status;
                existingBlog.Category = updatedBlog.Category;                
                SaveBlogs(blogs);
            }
            return existingBlog;
        }

        public bool DeleteBlog(long id)
        {
            var blogs = ReadBlogFromFile();
            var blogToRemove = blogs.FirstOrDefault(b => b.Id == id);
            if (blogToRemove != null)
            {
                blogs.Remove(blogToRemove);
                SaveBlogs(blogs);
                return true;
            }
            return false;
        }

        public bool ChangeBlogStatus(long id, bool status)
        {
            var blogs = ReadBlogFromFile();
            var blog = blogs.FirstOrDefault(b => b.Id == id);
            if (blog != null && (status == true || status == false))
            {
                blog.Status = status;
                SaveBlogs(blogs);
                return true;
            }
            return false;
        }

        private void SaveBlogs(List<Blog> blogs)
        {
            var jsonData = JsonConvert.SerializeObject(blogs, Formatting.Indented);
            File.WriteAllText(FilePath, jsonData);
        }

        public string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            // Convert to lower case
            title = title.ToLowerInvariant();

            // Remove accents (diacritics)
            title = RemoveAccents(title);

            // Map special characters to more readable strings
            title = ReplaceSpecialCharsWithWords(title);

            // Replace spaces and other non-alphanumeric characters with hyphens
            title = Regex.Replace(title, @"[^a-z0-9\s-]", "");

            // Replace multiple spaces with a single hyphen
            title = Regex.Replace(title, @"\s+", "-");

            // Trim any leading or trailing hyphens
            title = title.Trim('-');

            return title;
        }

        private static string RemoveAccents(string text)
        {
            if (text == null) return string.Empty;

            string normalized = text.Normalize(NormalizationForm.FormD);
            char[] array = new char[normalized.Length];
            int index = 0;

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    array[index++] = c;
                }
            }

            return new string(array, 0, index).Normalize(NormalizationForm.FormC);
        }

        private static string ReplaceSpecialCharsWithWords(string text)
        {
            // Define a dictionary to map special characters to words
            var replacements = new Dictionary<string, string>
    {
         { "C#", "c-sharp" },
        { "&", "and" },
        { "+", "plus" },
        { "%", "percent" },
        { "$", "dollar" },
        { "@", "at" },
        { "!", "exclamation" },
        { "?", "question" },
        { "#", "hash" },
        { "*", "asterisk" },
        { "=", "equals" },
        { "/", "slash" },
        { "\\", "backslash" },
        { "~", "tilde" },
        { ":", "colon" },
        { "<", "leftanglebracket" },
        { ">", "rightanglebracket" },
        { "{", "leftbrace" },
        { "}", "rightbrace" },
        { "[", "leftbracket" },
        { "]", "rightbracket" },
        { "(", "leftparenthesis" },
        { ")", "rightparenthesis" },
        { "'", "apostrophe" },
        { "\"", "quote" },
        { ";", "semicolon" },
        { ",", "comma" },
        { ".", "dot" },
        { "_", "underscore" },
        { "-", "hyphen" },
        { "|", "pipe" },
        { "^", "caret" },
        { "`", "backtick" },
        { "©", "copyright" },
        { "®", "registered" },
        { "™", "trademark" },
        { "§", "section" },
       // { "~", "tilde" },
        { "€", "euro" },
        { "£", "pound" },
        { "¥", "yen" }
    };

            // Replace each special character with its corresponding word
            foreach (var replacement in replacements)
            {
                text = text.Replace(replacement.Key, replacement.Value);
            }

            return text;
        }
    }
}

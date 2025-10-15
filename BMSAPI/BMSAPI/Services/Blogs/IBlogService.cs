using BMSAPI.Models;

namespace BMSAPI.Services.Blogs
{
    public interface IBlogService
    {
        List<Blog> ReadBlogFromFile();
        Blog GetBlogById(long id);
        Blog AddBlog(AddBlog blog, string[] imageUrls,string BlogSlug,string userId);
        Blog UpdateBlog(long id, UpdateBlog updatedBlog, string[] imageUrls);
        bool DeleteBlog(long id);
        bool ChangeBlogStatus(long id, bool status);
        string GenerateSlug(string title);
        bool IsSlugExist(string slug);
    }
}

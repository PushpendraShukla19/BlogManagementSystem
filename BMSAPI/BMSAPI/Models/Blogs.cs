using Newtonsoft.Json;

namespace BMSAPI.Models
{
    public partial class BlogWapper
    {
        public List<Blog> Posts { get; set; }
    }

    public partial class Blog
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("featured_image")]
        public string[] FeaturedImage { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("meta")]
        public Meta MetaTags { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }
    }

    public partial class Author
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("profile_picture")]
        public string ProfilePicture { get; set; }
    }

    public partial class Content
    {
        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("keywords")]
        public string[] Keywords { get; set; }
    }


   public partial class AddBlog
    {

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("intro")]
        public string Introduction { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("body")]
        public string Content { get; set; }  // Ensure it is 'Content' instead of 'content'

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("keywords")]
        public string[] Keywords { get; set; }

        [JsonProperty("descriptions")]
        public string Descriptions { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }
    }

    public partial class UpdateBlog
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("intro")]
        public string Introduction { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("body")]
        public string Content { get; set; }  // Ensure it is 'Content' instead of 'content'

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("keywords")]
        public string[] Keywords { get; set; }

        [JsonProperty("descriptions")]
        public string Descriptions { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }
    }

}
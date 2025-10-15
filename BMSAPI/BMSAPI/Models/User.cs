using Newtonsoft.Json;

namespace BMSAPI.Models
{
    public partial class UserListWrapper
    {
        [JsonProperty("users")]
        public List<User> Users { get; set; }
    }
    public partial class User
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("profile_picture")]
        public string ProfileImage { get; set; }
    }

    public partial class AddUser
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    public partial class UserUpdate
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("bio")]
        public string Bio { get; set; }
       
    }

    public partial class LoginUser
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}

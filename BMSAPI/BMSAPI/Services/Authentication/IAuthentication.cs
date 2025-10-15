using BMSAPI.Models;

namespace BMSAPI.Services.Authentication
{
    public interface IAuthentication
    {
        List<User> ReadUsersFromFile();
        void SaveUsersToFile(List<User> users);
        List<User> GetAllUsers();
        User GetUserById(long id);
        User AddUser(AddUser user);
        User EditUser(long id, UserUpdate updatedUser, string fileName);
        bool DeleteUser(long id);

        string Login(string email, string password);
    }
}

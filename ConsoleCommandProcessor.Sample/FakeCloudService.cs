using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleCommandProcessor.Sample
{
    public class FakeCloudService
    {
        #region Fields

        private Dictionary<string, User> _users = new Dictionary<string, User>();

        #endregion

        #region Constructors

        public FakeCloudService()
        {
            _users.Add("me", new User("me", "123", "Default User"));
        }

        #endregion

        #region Properties

        public bool IsLoggedIn { get; private set; }

        public User CurrentUser { get; private set; }

        public IReadOnlyCollection<User> Users { get { return _users.Values; } }

        #endregion

        #region Methods

        public Task<bool> LoginAsync(string username, string password)
        {
            IsLoggedIn = _users.ContainsKey(username) && _users[username].Password == password;
            if (IsLoggedIn)
                CurrentUser = _users[username];
            return Task.FromResult(IsLoggedIn);
        }

        public Task LogoutAsync()
        {
            CurrentUser = null;
            IsLoggedIn = false;
            return Task.FromResult(0);
        }

        public Task AddUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _users[user.Username] = user;
            return Task.FromResult(0);
        }

        public Task<bool> DeleteUserAsync(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (username == CurrentUser.Username)
                throw new InvalidOperationException("Cannot delete the currently logged in user.");

            var found = _users.ContainsKey(username);
            if (found)
                _users.Remove(username);
            return Task.FromResult(found);
        }

        #endregion
    }

    public class User
    {
        public User(string username, string password, string fullname)
        {
            if (String.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (String.IsNullOrWhiteSpace(fullname))
                throw new ArgumentNullException(nameof(fullname));

            Username = username;
            Password = password;
            Fullname = fullname;
        }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public string Fullname { get; private set; }

    }
}

using System;

namespace Hyprsoft.ConsoleCommandProcessor.Sample
{
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

using ConsoleCommandProcessor.Library;
using System;
using System.Threading.Tasks;

namespace ConsoleCommandProcessor.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Commands
            const string LoginCommandName = "login";
            const string LogoutCommandName = "logout";
            const string AddUserCommandName = "adduser";
            const string DeleteUserCommandName = "deluser";
            const string ListUsersCommandName = "listusers";

            // Parameters
            const string UsernameParameterName = "username";
            const string PasswordParameterName = "password";
            const string FullnameParameterName = "fullname";

            var service = new FakeCloudService();
            var manager = new CommandManager();

            // Login
            manager.AddCommand(LoginCommandName, new Command
            {
                Description = "Authenticates a user using a username and password.",
                CanExecute = () => !service.IsLoggedIn,
                CantExecuteMessage = "Already logged in.",
                Execute = async command =>
                {
                    if (await service.LoginAsync(command.GetParameter(UsernameParameterName).Value, command.GetParameter(PasswordParameterName).Value))
                        Console.WriteLine($"Logged in with {command.GetParameter(UsernameParameterName).Value}.");
                    else
                        Console.WriteLine("Invalid username or password.  Please try again.");
                }
            }).AddParameter(UsernameParameterName, new Parameter
            {
                Prompt = "Username",
                Description = "Username to login with.",
                ValidateFailedMessage = "Username cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            }).AddParameter(PasswordParameterName, new Parameter
            {
                IsPassword = true,
                Prompt = "Password",
                Description = "Password to login with.",
                ValidateFailedMessage = "Password cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            });

            // Logout
            manager.AddCommand(LogoutCommandName, new Command
            {
                Description = "Logs out a user.",
                CanExecute = () => service.IsLoggedIn,
                CantExecuteMessage = "Not logged in.",
                Execute = async command =>
                {
                    await service.LogoutAsync();
                    Console.WriteLine("Logged out.");
                }
            });

            // Add User
            manager.AddCommand(AddUserCommandName, new Command
            {
                Description = "Add a new user.",
                CanExecute = () => service.IsLoggedIn,
                CantExecuteMessage = "Not logged in.",
                Execute = async command =>
                {
                    await service.AddUserAsync(new User(command.GetParameter(UsernameParameterName).Value, command.GetParameter(PasswordParameterName).Value, command.GetParameter(FullnameParameterName).Value));
                    Console.WriteLine($"'{command.GetParameter(UsernameParameterName).Value}' was successfully added.");
                }
            }).AddParameter(UsernameParameterName, new Parameter
            {
                Prompt = "Username",
                Description = "Username of the new user.",
                ValidateFailedMessage = "Username cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            }).AddParameter(PasswordParameterName, new Parameter
            {
                IsPassword = true,
                Prompt = "Password",
                Description = "Password of the new user.",
                ValidateFailedMessage = "Password cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            }).AddParameter(FullnameParameterName, new Parameter
            {
                Prompt = "Full Name",
                Description = "Full name of the new user.",
                ValidateFailedMessage = "Full Name cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            });

            // Delete User
            manager.AddCommand(DeleteUserCommandName, new Command
            {
                Description = "Remove an existing user.",
                CanExecute = () => service.IsLoggedIn,
                CantExecuteMessage = "Not logged in.",
                Execute = async command =>
                {
                    try
                    {
                        if (await service.DeleteUserAsync(command.GetParameter(UsernameParameterName).Value))
                            Console.WriteLine($"'{command.GetParameter(UsernameParameterName).Value}' was successfully removed.");
                        else
                            Console.WriteLine($"'{command.GetParameter(UsernameParameterName).Value}' does not exist.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }).AddParameter(UsernameParameterName, new Parameter
            {
                Prompt = "Username",
                Description = "Username of an existing user.",
                ValidateFailedMessage = "Username cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            });

            // List Users
            manager.AddCommand(ListUsersCommandName, new Command
            {
                Description = "Displays available users.",
                CanExecute = () => service.IsLoggedIn,
                CantExecuteMessage = "Not logged in.",
                Execute = command =>
                {
                    Console.WriteLine("Username | Password | Full Name");
                    foreach (var user in service.Users)
                        Console.WriteLine($"{user.Username} | {user.Password} | {user.Fullname}");
                    return Task.FromResult(0);
                }
            });

            Task.Run(async () => await manager.RunAsync()).Wait();
        }
    }
}

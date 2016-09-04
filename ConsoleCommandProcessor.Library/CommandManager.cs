using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleCommandProcessor.Library
{
    /// <summary>
    /// Represents helper functions for a console command line interface processing application.
    /// </summary>
    public class CommandManager
    {
        #region Fields

        private Dictionary<string, Command> _commands = new Dictionary<string, Command>();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="CommandManager"/> class. 
        /// </summary>
        public CommandManager()
        {
            // Attempt to retrieve our application defaults from our entry assembly.  Fall back to the executing assembly if needed (i.e. unit tests).
            var assembly = Assembly.GetEntryAssembly() != null ? Assembly.GetEntryAssembly() : Assembly.GetExecutingAssembly();
            AppTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute), false))?.Title;
            AppVersion = assembly.GetName()?.Version;
            AppCompany = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false))?.Company;

            // Help
            AddCommand(HelpCommandName, new Command
            {
                Description = "Displays application command usage.",
                Execute = command =>
                {
                    Console.WriteLine("Commands and Parameters:");
                    foreach (var c in _commands)
                    {
                        Console.WriteLine($"{c.Key} - {c.Value.Description}");
                        foreach (var p in c.Value.Parameters)
                            Console.WriteLine($"\t{p.Prompt}{(p.IsRequired ? " * " : String.Empty)} - {p.Description}");
                    }
                    Console.WriteLine("\n* Required parameter value.");
                    return Task.FromResult(0);
                }
            });

            // Clear Console
            AddCommand(ClearCommandName, new Command
            {
                Description = "Clears the console window.",
                Execute = command =>
                {
                    Console.Clear();
                    return Task.FromResult(0);
                }
            });

            // Exit
            AddCommand(ExitCommandName, new Command
            {
                Description = "Exits the application."
            });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the command name for the built-in help command.
        /// </summary>
        public const string HelpCommandName = "help";

        /// <summary>
        /// Gets the command name for the built-in clear command.
        /// </summary>
        public const string ClearCommandName = "clear";

        /// <summary>
        /// Gets the command name for the built-in exit command.
        /// </summary>
        public const string ExitCommandName = "exit";

        /// <summary>
        /// Gets or sets the application title displayed when the command manager's <see cref="RunAsync"/> is invoked.
        /// </summary>
        /// <remarks>The default value is the <see cref="AssemblyTitleAttribute"/> value in the AssemblyInfo.cs of the entry assembly.</remarks>
        public string AppTitle { get; set; }

        /// <summary>
        /// Gets or sets the application version displayed when the command manager's <see cref="RunAsync"/> is invoked.
        /// </summary>
        /// <remarks>The default value is the <see cref="AssemblyVersionAttribute"/> value in the AssemblyInfo.cs of the entry assembly.</remarks>
        public Version AppVersion { get; set; }

        /// <summary>
        /// Gets or sets the application company name displayed when the command manager's <see cref="RunAsync"/> is invoked.
        /// </summary>
        /// <remarks>The default value is the <see cref="AssemblyCompanyAttribute"/> value in the AssemblyInfo.cs of the entry assembly.</remarks>
        public string AppCompany { get; set; }

        /// <summary>
        /// Gets the commands available for this Command Manager.
        /// </summary>
        public IReadOnlyCollection<Command> Commands { get { return _commands.Values; } }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a <see cref="Command"/> to this Command Manager.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="command">The command instance to add.</param>
        /// <returns>The command instance added.</returns>
        /// <exception cref="DuplicateNameException">Throw when the command is already available in this Command Manager.</exception>
        public Command AddCommand(string name, Command command)
        {
            if (_commands.ContainsKey(name))
                throw new DuplicateNameException(name);

            command.Name = name;
            _commands[name] = command;
            return command;
        }

        /// <summary>
        /// Removes a <see cref="Command"/> from this Command Manager.
        /// </summary>
        /// <param name="name">The name of the command to remove.</param>
        /// <returns>The command removed if found; otherwise null.</returns>
        public Command RemoveCommand(string name)
        {
            Command command = null;
            if (_commands.ContainsKey(name) && name != HelpCommandName && name != ClearCommandName && name != ExitCommandName)
            {
                command = _commands[name];
                _commands.Remove(name);
            }
            return command;
        }

        /// <summary>
        /// Get's a specified <see cref="Command"/> by name.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <returns>The command for the specified name if found; otherwise null.</returns>
        public Command GetCommand(string name)
        {
            return _commands.ContainsKey(name) ? _commands[name] : null;
        }

        /// <summary>
        /// Starts the main command processing loop.  Quits when the 'exit' command is executed.
        /// </summary>
        /// <returns>A asynchronous operation <see cref="Task"/>.</returns>
        public async Task RunAsync()
        {
            Console.WriteLine($"{AppTitle} Command Line Interface (CLI) Version {AppVersion}");
            Console.WriteLine($"Copyright © {DateTime.Now.Year} by {AppCompany}.  All rights reserved.\n");
            Console.WriteLine($"Type '{HelpCommandName}' to list available commands.  Commands are case sensitive.\n");
            Command command = null;
            while (command != _commands[ExitCommandName])
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (!String.IsNullOrWhiteSpace(input))
                {
                    if (_commands.ContainsKey(input))
                    {
                        try
                        {
                            command = _commands[input];
                            if (command.CanExecute())
                            {
                                foreach (var parameter in command.Parameters)
                                {
                                    Console.Write($"{parameter.Prompt}{(parameter.IsRequired ? "*" : String.Empty)}: ");
                                    parameter.Value = parameter.IsPassword ? ReadPassword('*') : Console.ReadLine();
                                }
                                if (await command.ValidateAsync())
                                {
                                    await command.ExecuteAsync();
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Console.WriteLine("Invalid command parameter values detected.  Please try again.");
                                    foreach (var parameter in command.Parameters)
                                    {
                                        if (!await parameter.ValidateAsync())
                                            Console.WriteLine($"* {parameter.CantValidateMessage}");
                                    }
                                    Console.WriteLine();
                                }
                            }   // valid command?
                            else
                                Console.WriteLine($"This command is not valid in the current state.  Reason: {command.CantExecuteMessage}\n");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error.  Details: {ex.Message}\n");
                        }
                    }   // recognized command?
                    else
                        Console.WriteLine($"Invalid command.  Type '{HelpCommandName}' to list available commands.  Commands are case sensitive.\n");
                }   // valid input?
            }   // main program loop
            Console.WriteLine("Exiting.");
        }

        private static string ReadPassword(char mask)
        {
            const int EnterKey = 13, BackspaceKey = 8, ControlBackspaceKey = 127;
            int[] FilterdKeys = { 0, 27, 9, 10 };

            var password = new Stack<char>();
            char character = (char)0;

            while ((character = Console.ReadKey(true).KeyChar) != EnterKey)
            {
                if (character == BackspaceKey)
                {
                    if (password.Count > 0)
                    {
                        Console.Write("\b \b");
                        password.Pop();
                    }
                }
                else if (character == ControlBackspaceKey)
                {
                    while (password.Count > 0)
                    {
                        Console.Write("\b \b");
                        password.Pop();
                    }
                }
                else if (FilterdKeys.Count(x => character == x) > 0)
                {
                }
                else
                {
                    password.Push((char)character);
                    Console.Write(mask);
                }
            }

            Console.WriteLine();
            return new string(password.Reverse().ToArray());
        }

        #endregion
    }
}
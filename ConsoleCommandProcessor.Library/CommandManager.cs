using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleCommandProcessor.Library
{
    public class CommandManager
    {
        #region Fields

        private Dictionary<string, Command> _commands = new Dictionary<string, Command>();

        #endregion

        #region Constructors

        public CommandManager()
        {
            // Attempt to retrieve our application defaults from our entry assembly.  Fall back to the executing assembly if needed (i.e. unit tests).
            var assembly = Assembly.GetEntryAssembly() != null ? Assembly.GetEntryAssembly() : Assembly.GetExecutingAssembly();
            ApplicationTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute), false))?.Title;
            ApplicationVersion = assembly.GetName()?.Version;
            ApplicationCompany = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false))?.Company;

            // Help
            AddCommand(HelpCommandName, new Command
            {
                Description = "Displays application command usage.",
                CanExecute = () => true,
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
                CanExecute = () => true,
                Execute = command =>
                {
                    Console.Clear();
                    return Task.FromResult(0);
                }
            });

            // Exit
            AddCommand(ExitCommandName, new Command
            {
                Description = "Exits the application.",
                CanExecute = () => { return true; }
            });
        }

        #endregion

        #region Properties

        public const string HelpCommandName = "help";
        public const string ClearCommandName = "clear";
        public const string ExitCommandName = "exit";

        public string ApplicationTitle { get; set; }

        public Version ApplicationVersion { get; set; }

        public string ApplicationCompany { get; set; }

        public IReadOnlyCollection<Command> Commands { get { return _commands.Values; } }

        #endregion

        #region Methods

        public Command AddCommand(string name, Command command)
        {
            if (_commands.ContainsKey(name))
                throw new DuplicateNameException(name);

            _commands[name] = command;
            return command;
        }

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

        public Command GetCommand(string name)
        {
            return _commands.ContainsKey(name) ? _commands[name] : null;
        }

        public async Task RunAsync()
        {
            Console.WriteLine($"{ApplicationTitle} Command Line Interface (CLI) Version {ApplicationVersion}");
            Console.WriteLine($"Copyright © {DateTime.Now.Year} by {ApplicationCompany}.  All rights reserved.\n");
            Console.WriteLine($"Type '{HelpCommandName}' to list available commands.  Commands are case sensitive.");
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
                                    await command.ExecuteAsync();
                                else
                                {
                                    Console.WriteLine("Invalid command parameter values detected.  Please try again.");
                                    foreach (var parameter in command.Parameters)
                                    {
                                        if (!await parameter.ValidateAsync())
                                            Console.WriteLine($"* {parameter.ValidateFailedMessage}");
                                    }
                                }
                            }   // valid command?
                            else
                                Console.WriteLine($"This command is not valid in the current state.  Reason: {command.CantExecuteMessage}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error.  Details: {ex.Message}");
                        }
                    }   // recognized command?
                    else
                        Console.WriteLine("Invalid command.");
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
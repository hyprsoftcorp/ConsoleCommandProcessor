# ConsoleCommandProcessor
A helper library for console command line interfaces (CLI).  This library provides the following functionality:

1. All asynchronous API.
2. Main program loop for command processing.
3. Command validation.
4. Parameter validation.

## Hello World Code Sample
```csharp
static void Main(string[] args)
{
    var manager = new CommandManager();

    manager.AddCommand("speak", new Command
    {
        Description = "Outputs the word parameter to the console window.",
        Execute = command =>
        {
            Console.WriteLine($"{command.GetParameter("phrase").Value}");
            return Task.FromResult(0);
        }
    }).AddParameter("phrase", new Parameter
    {
        Prompt = "Phrase",
        Description = "The phrase to output to the console window.",
        CantValidateMessage = "Phrase cannot be null or whitespace.",
        Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
    });

    Task.Run(async () => await manager.RunAsync()).Wait();
}
```

## Hello World Screenshot
![alt text](https://raw.github.com/hyprsoftcorp/consolecommandprocessor/master/hello-world-screenshot.png "Hello World Screenshot")
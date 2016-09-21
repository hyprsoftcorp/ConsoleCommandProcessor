# Console Command Processor
A helper library for console command processing applications.  This library provides the following functionality:

1. All asynchronous API.
2. Main program loop for command and parameter validation and processing.
3. Command state and parameter validation.

## Hello World Code Sample
```csharp
static void Main(string[] args)
{
    var manager = new CommandManager();

    manager.AddCommand("speak", new Command
    {
        Description = "Outputs the phrase parameter to the console window.",
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

For a slightly more involved sample, refer to the [ConsoleCommandProcessor.Sample](https://github.com/hyprsoftcorp/ConsoleCommandProcessor/tree/master/ConsoleCommandProcessor.Sample) project.

## Hello World Screenshot
![alt text](https://raw.github.com/hyprsoftcorp/consolecommandprocessor/master/hello-world-screenshot.png "Hello World Screenshot")

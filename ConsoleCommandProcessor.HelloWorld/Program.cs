﻿using ConsoleCommandProcessor.Library;
using System;
using System.Threading.Tasks;

namespace ConsoleCommandProcessor.HelloWorld
{
    class Program
    {
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
    }
}
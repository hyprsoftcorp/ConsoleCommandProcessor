using System;

namespace ConsoleCommandProcessor.Library
{
    public sealed class DuplicateNameException : Exception
    {
        public DuplicateNameException(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public override string Message
        {
            get { return $"The command or parameter named '{Name}' already exists."; }
        }
    }
}

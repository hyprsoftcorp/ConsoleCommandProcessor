using System;

namespace ConsoleCommandProcessor.Library
{
    /// <summary>
    /// Represents an error that occurs when a <see cref="Command"/> or a <see cref="Parameter"/> is added to a collection when it already exists in the collection.
    /// </summary>
    public sealed class DuplicateNameException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref=" DuplicateNameException"/> class with specified name.
        /// </summary>
        /// <param name="name">The name of the <see cref="Command"/> or <see cref="Parameter"/> that is being duplicated.</param>
        public DuplicateNameException(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the <see cref="Command"/> or <see cref="Parameter"/> name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get { return $"The command or parameter named '{Name}' already exists."; }
        }
    }
}

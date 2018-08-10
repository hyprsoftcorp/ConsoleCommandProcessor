using System;
using System.Threading.Tasks;

namespace Hyprsoft.ConsoleCommandProcessor.Library
{
    /// <summary>
    /// Represents a command parameter in a console command processing application.
    /// </summary>
    public class Parameter
    {
        #region Properties

        /// <summary>
        /// Gets the name of this parameter.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets or sets whether or not this parameter is required to execute it's command.
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets whether or not this parameter is a password or not.
        /// </summary>
        public bool IsPassword { get; set; }

        /// <summary>
        /// Gets or sets the prompt used to collect the value for this parameter.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the description for this parameter.  This value is displayed when the help command is executed.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the message displayed to the user when this parameter's value is invalid.
        /// </summary>
        public string CantValidateMessage { get; set; }

        /// <summary>
        /// Gets or sets the value for this parameter.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Func{T, TResult}"/> when this parameter is validated.
        /// </summary>
        public Func<string, Task<bool>> Validate { get; set; } = parameter => Task.FromResult(true);

        #endregion

        #region Methods

        /// <summary>
        /// Validates whether or not this parameter's value is valid.
        /// </summary>
        /// <returns>True if the parameter's value is valid; otherwise false.</returns>
        public async Task<bool> ValidateAsync()
        {
            if (Validate == null)
                return true;
            else
                return await Validate(Value);
        }

        #endregion
    }
}
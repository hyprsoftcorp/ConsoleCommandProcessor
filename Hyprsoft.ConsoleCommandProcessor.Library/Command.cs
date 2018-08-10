using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hyprsoft.ConsoleCommandProcessor.Library
{
    /// <summary>
    /// Represents a command in a console command processing application.
    /// </summary>
    public class Command
    {
        #region Fields

        private Dictionary<string, Parameter> _parameters = new Dictionary<string, Parameter>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of this command.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets or sets the description for this command.  This value is displayed when the help command is executed.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Func{TResult}" /> to indicate whether or not this command can be executed in the current state.
        /// </summary>
        public Func<bool> CanExecute { get; set; } = () => true;

        /// <summary>
        /// Gets or sets the message displayed to the user when this command can not be executed due to the current state.
        /// </summary>
        public string CantExecuteMessage { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Func{T, TResult}"/> when this command is executed.
        /// </summary>
        public Func<Command, Task> Execute { get; set; } = command => Task.FromResult(0);

        /// <summary>
        /// Gets the parameters for this command. 
        /// </summary>
        public IReadOnlyCollection<Parameter> Parameters { get { return _parameters.Values; } }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a <see cref="Parameter"/> to this command.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameter">The parameter instance to add.</param>
        /// <returns>The command instance added.</returns>
        public Command AddParameter(string name, Parameter parameter)
        {
            if (_parameters.ContainsKey(name))
                throw new DuplicateNameException(name);

            parameter.Name = name;
            _parameters[name] = parameter;
            return this;
        }

        /// <summary>
        /// Removes a <see cref="Parameter"/> from this command.
        /// </summary>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <returns>The parameter removed if found; otherwise null.</returns>
        public Parameter RemoveParamter(string name)
        {
            Parameter parameter = null;
            if (_parameters.ContainsKey(name))
            {
                parameter = _parameters[name];
                _parameters.Remove(name);
            }
            return parameter;
        }

        /// <summary>
        /// Get's a specified <see cref="Parameter"/> by name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The parameter for the specified name if found; otherwise null.</returns>
        public Parameter GetParameter(string name)
        {
            return _parameters.ContainsKey(name) ? _parameters[name] : null;
        }

        /// <summary>
        /// Validates whether or not this command and it's parameters are valid in the current state.
        /// </summary>
        /// <returns>True if the command is valid in the current state; otherwise false.</returns>
        public async Task<bool> ValidateAsync()
        {
            var success = true;
            foreach (var parameter in _parameters)
                success = success && await parameter.Value.ValidateAsync();
            return success;
        }

        /// <summary>
        /// Invokes this command's <see cref="Execute"/> method. 
        /// </summary>
        /// <returns>A asynchronous operation <see cref="Task"/>.</returns>
        public Task ExecuteAsync()
        {
            return Execute(this);
        }

        #endregion
    }
}

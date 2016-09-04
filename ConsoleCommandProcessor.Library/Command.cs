using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleCommandProcessor.Library
{
    public class Command
    {
        #region Fields

        private Dictionary<string, Parameter> _parameters = new Dictionary<string, Parameter>();

        #endregion

        #region Properties

        public string Name { get; internal set; }

        public string Description { get; set; }

        public Func<bool> CanExecute { get; set; } = () => true;

        public string CantExecuteMessage { get; set; }

        public Func<Command, Task> Execute { get; set; } = command => Task.FromResult(0);

        public IReadOnlyCollection<Parameter> Parameters { get { return _parameters.Values; } }

        #endregion

        #region Methods

        public Command AddParameter(string name, Parameter parameter)
        {
            if (_parameters.ContainsKey(name))
                throw new DuplicateNameException(name);

            parameter.Name = name;
            _parameters[name] = parameter;
            return this;
        }

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

        public Parameter GetParameter(string name)
        {
            return _parameters.ContainsKey(name) ? _parameters[name] : null;
        }

        public async Task<bool> ValidateAsync()
        {
            var success = true;
            foreach (var parameter in _parameters)
                success = success && await parameter.Value.ValidateAsync();
            return success;
        }

        public Task ExecuteAsync()
        {
            return Execute(this);
        }

        #endregion
    }
}

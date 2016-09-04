using System;
using System.Threading.Tasks;

namespace ConsoleCommandProcessor.Library
{
    public class Parameter
    {
        #region Properties

        public string Name { get; internal set; }

        public bool IsRequired { get; set; } = true;

        public bool IsPassword { get; set; }

        public string Prompt { get; set; }

        public string Description { get; set; }

        public string CantValidateMessage { get; set; }

        public string Value { get; set; }

        public Func<string, Task<bool>> Validate { get; set; } = parameter => Task.FromResult(true);

        #endregion

        #region Methods

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
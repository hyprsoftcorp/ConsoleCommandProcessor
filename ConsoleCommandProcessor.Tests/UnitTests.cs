using ConsoleCommandProcessor.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ConsoleCommandProcessor.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void CommandManagerDefaults()
        {
            var manager = new CommandManager();

            Assert.AreEqual(manager.ApplicationCompany, "Hyprsoft Corporation");
            Assert.AreEqual(manager.ApplicationVersion, new System.Version("1.0.0.0"));
            Assert.AreEqual(manager.ApplicationTitle, "Console Command Processor Library");

            Assert.AreEqual(3, manager.Commands.Count);
            Assert.IsNotNull(manager.GetCommand(CommandManager.HelpCommandName));
            Assert.IsNotNull(manager.GetCommand(CommandManager.ClearCommandName));
            Assert.IsNotNull(manager.GetCommand(CommandManager.ExitCommandName));
        }

        [TestMethod]
        public void CommandManagerAddGetRemoveCommands()
        {
            const string TestCommandName = "test";
            var manager = new CommandManager();

            Assert.AreEqual(3, manager.Commands.Count);
            manager.AddCommand(TestCommandName, new Command());
            Assert.AreEqual(4, manager.Commands.Count);
            Assert.IsNotNull(manager.GetCommand(TestCommandName));
            manager.RemoveCommand(TestCommandName);
            Assert.AreEqual(3, manager.Commands.Count);
            Assert.IsNull(manager.GetCommand(TestCommandName));

            // Make sure we can't remove our 3 default commands.
            Assert.IsNull(manager.RemoveCommand(CommandManager.HelpCommandName));
            Assert.IsNull(manager.RemoveCommand(CommandManager.ClearCommandName));
            Assert.IsNull(manager.RemoveCommand(CommandManager.ExitCommandName));
            Assert.AreEqual(3, manager.Commands.Count);
        }

        [TestMethod]
        public async Task CommandDefaults()
        {
            var command = new Command();

            Assert.IsNull(command.Description);
            Assert.IsNull(command.CanExecute);
            Assert.IsNull(command.CantExecuteMessage);
            Assert.IsNull(command.Execute);
            Assert.AreEqual(0, command.Parameters.Count);
            Assert.IsTrue(await command.ValidateAsync());
            await command.ExecuteAsync();
        }

        [TestMethod]
        public async Task CommandAddGetRemoveParameters()
        {
            var command = new Command();

            const string TestParameter1Name = "param1";
            const string TestParameter2Name = "param2";
            Command executeFuncParamValue = null;
            command = new Command
            {
                Description = "Test Command Description",
                CanExecute = () => true,
                CantExecuteMessage = "Test Command Invalid",
                Execute = c =>
                {
                    executeFuncParamValue = c;
                    return Task.FromResult(0);
                }
            }.AddParameter(TestParameter1Name, new Parameter()).AddParameter(TestParameter2Name, new Parameter());

            Assert.IsNotNull(command.Description);
            Assert.IsNotNull(command.CanExecute);
            Assert.IsNotNull(command.CantExecuteMessage);
            Assert.IsNotNull(command.Execute);
            Assert.AreEqual(2, command.Parameters.Count);
            Assert.IsNotNull(command.GetParameter(TestParameter1Name));
            Assert.IsNotNull(command.GetParameter(TestParameter2Name));
            Assert.IsTrue(await command.ValidateAsync());
            await command.ExecuteAsync();
            Assert.AreEqual(command, executeFuncParamValue);

            Assert.IsNotNull(command.RemoveParamter(TestParameter1Name));
            Assert.IsNotNull(command.RemoveParamter(TestParameter2Name));
            Assert.IsNull(command.RemoveParamter("bad"));
            Assert.AreEqual(0, command.Parameters.Count);
        }

        [TestMethod, ExpectedException(typeof(DuplicateNameException))]
        public void DuplicateCommand()
        {
            const string TestCommandName = "duplicate";
            var manager = new CommandManager();
            manager.AddCommand(TestCommandName, new Command());
            manager.AddCommand(TestCommandName, new Command());
        }

        [TestMethod]
        public async Task ParameterDefaults()
        {
            var parameter = new Parameter();

            Assert.IsTrue(parameter.IsRequired);
            Assert.IsFalse(parameter.IsPassword);
            Assert.IsNull(parameter.Prompt);
            Assert.IsNull(parameter.Description);
            Assert.IsNull(parameter.ValidateFailedMessage);
            Assert.IsNull(parameter.Value);
            Assert.IsNull(parameter.Validate);
            Assert.IsTrue(await parameter.ValidateAsync());
        }

        [TestMethod]
        public async Task Parameter()
        {
            var parameter = new Parameter();

            const string TestParameterValue = "value";
            string validateFuncParamValue = null;
            parameter = new Parameter
            {
                IsRequired = false,
                IsPassword = true,
                Prompt = "Test",
                Description = "Test Parameter",
                ValidateFailedMessage = "Bad Parameter",
                Value = TestParameterValue,
                Validate = value =>
                {
                    validateFuncParamValue = value;
                    return Task.FromResult(true);
                }
            };
            Assert.IsFalse(parameter.IsRequired);
            Assert.IsTrue(parameter.IsPassword);
            Assert.IsNotNull(parameter.Prompt);
            Assert.IsNotNull(parameter.Description);
            Assert.IsNotNull(parameter.ValidateFailedMessage);
            Assert.AreEqual(TestParameterValue, parameter.Value);
            Assert.IsNotNull(parameter.Validate);
            Assert.IsTrue(await parameter.ValidateAsync());
            Assert.AreEqual(TestParameterValue, validateFuncParamValue);

            parameter = new Parameter
            {
                Validate = value => Task.FromResult(false)
            };
            Assert.IsFalse(await parameter.ValidateAsync());
        }

        [TestMethod, ExpectedException(typeof(DuplicateNameException))]
        public void DuplicateParameter()
        {
            const string TestParameterName = "duplicate";
            var command = new Command();
            command = new Command().AddParameter(TestParameterName, new Parameter()).AddParameter(TestParameterName, new Parameter());
        }
    }
}

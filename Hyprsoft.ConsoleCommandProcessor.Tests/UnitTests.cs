using Hyprsoft.ConsoleCommandProcessor.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Hyprsoft.ConsoleCommandProcessor.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void CommandManagerDefaults()
        {
            var manager = new CommandManager();

            Assert.AreEqual("Hyprsoft Corporation", manager.AppCompany);
            Assert.AreEqual(new Version("2.0.0.0"), manager.AppVersion);
            Assert.AreEqual("Console Command Processor Library", manager.AppTitle);

            Assert.AreEqual(3, manager.Commands.Count);
            Assert.AreEqual(CommandManager.HelpCommandName, manager.GetCommand(CommandManager.HelpCommandName).Name);
            Assert.AreEqual(CommandManager.ClearCommandName, manager.GetCommand(CommandManager.ClearCommandName).Name);
            Assert.AreEqual(CommandManager.ExitCommandName, manager.GetCommand(CommandManager.ExitCommandName).Name);
            Assert.IsNull(manager.Startup);
            Assert.IsNull(manager.Shutdown);
        }

        [TestMethod]
        public void CommandManagerStartupShutdown()
        {
            var manager = new CommandManager(() => Task.FromResult(0), null);
            Assert.IsNotNull(manager.Startup);
            Assert.IsNull(manager.Shutdown);

            manager = new CommandManager(null, () => Task.FromResult(0));
            Assert.IsNull(manager.Startup);
            Assert.IsNotNull(manager.Shutdown);
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
            Assert.AreEqual(TestCommandName, manager.RemoveCommand(TestCommandName).Name);
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

            Assert.IsNull(command.Name);
            Assert.IsNull(command.Description);
            Assert.IsNotNull(command.CanExecute);
            Assert.IsNull(command.CantExecuteMessage);
            Assert.IsNotNull(command.Execute);
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
                CantExecuteMessage = "Test Command Invalid",
                Execute = c =>
                {
                    executeFuncParamValue = c;
                    return Task.FromResult(0);
                }
            }.AddParameter(TestParameter1Name, new Parameter()).AddParameter(TestParameter2Name, new Parameter());

            Assert.IsNull(command.Name);
            Assert.IsNotNull(command.Description);
            Assert.IsNotNull(command.CanExecute);
            Assert.IsNotNull(command.CantExecuteMessage);
            Assert.IsNotNull(command.Execute);
            Assert.AreEqual(2, command.Parameters.Count);
            Assert.AreEqual(TestParameter1Name, command.GetParameter(TestParameter1Name).Name);
            Assert.AreEqual(TestParameter2Name, command.GetParameter(TestParameter2Name).Name);
            Assert.IsTrue(await command.ValidateAsync());
            await command.ExecuteAsync();
            Assert.AreEqual(command, executeFuncParamValue);

            Assert.AreEqual(TestParameter1Name, command.RemoveParamter(TestParameter1Name).Name);
            Assert.AreEqual(TestParameter2Name, command.RemoveParamter(TestParameter2Name).Name);
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

            Assert.IsNull(parameter.Name);
            Assert.IsTrue(parameter.IsRequired);
            Assert.IsFalse(parameter.IsPassword);
            Assert.IsNull(parameter.Prompt);
            Assert.IsNull(parameter.Description);
            Assert.IsNull(parameter.CantValidateMessage);
            Assert.IsNull(parameter.Value);
            Assert.IsNotNull(parameter.Validate);
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
                CantValidateMessage = "Bad Parameter",
                Value = TestParameterValue,
                Validate = value =>
                {
                    validateFuncParamValue = value;
                    return Task.FromResult(true);
                }
            };
            Assert.IsNull(parameter.Name);
            Assert.IsFalse(parameter.IsRequired);
            Assert.IsTrue(parameter.IsPassword);
            Assert.IsNotNull(parameter.Prompt);
            Assert.IsNotNull(parameter.Description);
            Assert.IsNotNull(parameter.CantValidateMessage);
            Assert.AreEqual(TestParameterValue, parameter.Value);
            Assert.IsNotNull(parameter.Validate);
            Assert.IsTrue(await parameter.ValidateAsync());
            Assert.AreEqual(TestParameterValue, validateFuncParamValue);
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

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModel;
using Xunit;
using ClassLibrary;
using System.Linq;

namespace ViewModelTests
{
    public class ViewModelTests
    {
        [Fact]
        public void Constructor_InitializesDefaultsCorrectly()
        {
            var viewModel = new MyViewModel();
            Assert.Equal(2000, viewModel.TimePause);
            Assert.Equal(5, viewModel.NewDataItemsCount);
            Assert.True(viewModel.IsStartEnabled);
            Assert.False(viewModel.IsCancelEnabled);
            Assert.NotNull(viewModel.StartCommand);
            Assert.NotNull(viewModel.CancelCommand);
            Assert.NotNull(viewModel.ShowDataCommand);
        }

        [Fact]
        public async Task StartCommand_CanBeCancelled()
        {
            var viewModel = new MyViewModel
            {
                TimePause = 100,
                NewDataItemsCount = 10
            };

            bool cancelCalled = false;
            viewModel.CancelRequested += () => cancelCalled = true;

            viewModel.StartCommand.Execute(null);
            await Task.Delay(150);

            viewModel.CancelCommand.Execute(null);
            await Task.Delay(100);

            Assert.True(cancelCalled);
            Assert.True(viewModel.TextInfo == "The operation was cancelled" ||
                        viewModel.TextInfo == "The operation was completed");
            Assert.True(viewModel.IsStartEnabled);
            Assert.False(viewModel.IsCancelEnabled);
        }

        [Fact]
        public void CancelCommand_InvokesCancelRequested()
        {
            var viewModel = new MyViewModel();
            bool wasCancelled = false;

            viewModel.CancelRequested += () => wasCancelled = true;

            viewModel.CancelCommand.Execute(null);

            Assert.True(wasCancelled);
        }

        [Fact]
        public void ShowDataCommand_InvokesShowDataRequested()
        {
            var viewModel = new MyViewModel();
            bool wasCalled = false;
            string output = null;

            viewModel.ShowDataRequested += data =>
            {
                wasCalled = true;
                output = data;
            };

            viewModel.ShowDataCommand.Execute(null);

            Assert.True(wasCalled);
            Assert.NotNull(output);
            Assert.Contains("User", output);
        }

        [Fact]
        public void PropertyChanged_EventFiresForProperties()
        {
            var viewModel = new MyViewModel();
            string changedProperty = null;

            viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;
            viewModel.TimePause = 1234;

            Assert.Equal("TimePause", changedProperty);
        }
    }
}

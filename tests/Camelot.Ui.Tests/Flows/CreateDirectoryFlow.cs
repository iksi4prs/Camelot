using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Camelot.ViewModels.Implementations.MainWindow.FilePanels;
using Camelot.Views.Dialogs;
using Camelot.Views.Main;
using Camelot.Views.Main.Controls;
using Xunit;

namespace Camelot.Ui.Tests.Flows
{
    public class CreateDirectoryFlow : IDisposable
    {
        private const string DirectoryName = "CreateDirectoryTest__Directory";

        private CreateDirectoryDialog _dialog;
        private string _directoryFullPath;

        [Fact(DisplayName = "Create and remove directory")]
        public async Task CreateAndRemoveDirectoryTest()
        {
            var app = AvaloniaApp.GetApp();
            var window = AvaloniaApp.GetMainWindow();

            await Task.Delay(100);

            Keyboard.PressKey(window, Key.Tab);
            Keyboard.PressKey(window, Key.Down);
            Keyboard.PressKey(window, Key.F7);

            await Task.Delay(100);

            _dialog = app
                .Windows
                .OfType<CreateDirectoryDialog>()
                .Single();
            var directoryNameTextBox = _dialog
                .GetVisualDescendants()
                .OfType<TextBox>()
                .Single();

            directoryNameTextBox.RaiseEvent(new TextInputEventArgs
            {
                Device = KeyboardDevice.Instance,
                Text = DirectoryName,
                RoutedEvent = InputElement.TextInputEvent
            });
            Keyboard.PressKey(window, Key.Enter);

            await Task.Delay(100);

            _dialog = app
                .Windows
                .OfType<CreateDirectoryDialog>()
                .SingleOrDefault();
            Assert.Null(_dialog);

            var filesPanel = app
                .MainWindow
                .GetVisualDescendants()
                .OfType<FilesPanelView>()
                .SingleOrDefault(CheckIfActive);
            Assert.NotNull(filesPanel);

            Keyboard.PressKey(window, Key.F, RawInputModifiers.Control);

            await Task.Delay(100);

            var searchPanel = filesPanel
                .GetVisualDescendants()
                .OfType<SearchView>()
                .SingleOrDefault();
            Assert.NotNull(searchPanel);

            var searchTextBox = searchPanel
                .GetVisualDescendants()
                .OfType<TextBox>()
                .SingleOrDefault();
            Assert.NotNull(searchTextBox);

            searchTextBox.RaiseEvent(new TextInputEventArgs
            {
                Device = KeyboardDevice.Instance,
                Text = DirectoryName,
                RoutedEvent = InputElement.TextInputEvent
            });

            await Task.Delay(1000);

            Keyboard.PressKey(window, Key.Tab);
            Keyboard.PressKey(window, Key.Tab);
            Keyboard.PressKey(window, Key.Down);
            Keyboard.PressKey(window, Key.Down);

            await Task.Delay(100);

            var selectedItemText = GetSelectedItemText(filesPanel);
            Assert.Equal(DirectoryName, selectedItemText);
        }

        public void Dispose()
        {
            _dialog?.Close();

            if (!string.IsNullOrEmpty(_directoryFullPath) && Directory.Exists(_directoryFullPath))
            {
                Directory.Delete(_directoryFullPath);
            }
        }

        private static bool CheckIfActive(FilesPanelView filesPanel)
        {
            var dataGrid = GetDataGrid(filesPanel);
            var viewModel = (FilesPanelViewModel) dataGrid.DataContext;

            return viewModel?.IsActive ?? false;
        }

        private string GetSelectedItemText(IInputElement filesPanel)
        {
            var dataGrid = GetDataGrid(filesPanel);
            var directoryViewModel = (DirectoryViewModel) dataGrid.SelectedItem;
            _directoryFullPath = directoryViewModel.FullPath;

            return directoryViewModel.FullName;
        }

        private static DataGrid GetDataGrid(IVisual filesPanel) =>
            filesPanel
                .GetVisualDescendants()
                .OfType<DataGrid>()
                .Single();
    }
}
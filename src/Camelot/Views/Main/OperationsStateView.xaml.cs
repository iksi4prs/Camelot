using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Camelot.Views.Main
{
    public class OperationsStateView : UserControl
    {
        public OperationsStateView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
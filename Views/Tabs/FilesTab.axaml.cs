using System;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class FilesTab : UserControl
{
    private FilesTabViewModel _viewModel;

    public FilesTab()
    {
        InitializeComponent();
        DataContext = _viewModel = new FilesTabViewModel();
    }

}
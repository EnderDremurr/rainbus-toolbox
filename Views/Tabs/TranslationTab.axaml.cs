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

public partial class TranslationTab : UserControl
{
    public TranslationTab()
    {
        InitializeComponent();
        DataContext ??= new TranslationTabViewModel();
        
    }

}
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views
{
    public partial class PanicTranslationEditor : UserControl, IFileEditor
    {
        public PanicTranslationEditorViewModel VM => (PanicTranslationEditorViewModel)DataContext!;

        public PanicTranslationEditor()
        {
            InitializeComponent();
            DataContext ??= new PanicTranslationEditorViewModel();
        }

        private void OnPreviousClick(object? sender, RoutedEventArgs e) => VM.GoPrevious();

        private void OnNextClick(object? sender, RoutedEventArgs e) => VM.GoNext();

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((PanicInfoFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((PanicInfoFile)file);
        public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
    }
}
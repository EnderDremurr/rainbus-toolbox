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
    public partial class BuffTranslationEditor : UserControl, INotifyPropertyChanged, IFileEditor
    {
        public BuffTranslationEditorViewModel VM => (BuffTranslationEditorViewModel)DataContext!;

        public BuffTranslationEditor()
        {
            InitializeComponent();
            DataContext ??= new BuffTranslationEditorViewModel();
        }

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((BuffsFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((BuffsFile)file);
        public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
    }

}
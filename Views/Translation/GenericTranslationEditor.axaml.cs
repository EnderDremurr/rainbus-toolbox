using Avalonia.Controls;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views.Translation
{
    public partial class GenericTranslationEditor : UserControl, IFileEditor
    {
        public GenericTranslationEditorViewModel VM => (GenericTranslationEditorViewModel)DataContext!;

        public GenericTranslationEditor()
        {
            InitializeComponent();
            DataContext ??= new GenericTranslationEditorViewModel();
        }

        public void SetFileToEdit(LocalizationFileBase file)
        {
            VM.LoadEditableFile(file);
        }

        public void SetReferenceFile(LocalizationFileBase file)
        {
            VM.LoadReferenceFile(file);
        }

        public void SaveUnknownFile()
        {
            VM.SaveEditableFile();
        }
    }
}
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

public interface IFileEditor
{
    public void SetFileToEdit(LocalizationFileBase file);
    public void SetReferenceFile(LocalizationFileBase file);

    public void AskEditorToSave(RepositoryManager repositoryManager);
}
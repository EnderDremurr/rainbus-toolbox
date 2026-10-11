using RainbusToolbox.Services.ExternalServices;
using RainbusToolbox.Services.ExternalServices.Data;

public interface IFileEditor
{
    public void SetFileToEdit(LocalizationFileBase file);
    public void SetReferenceFile(LocalizationFileBase file);

    public void AskEditorToSave(LocalizationManager localizationManager);
}
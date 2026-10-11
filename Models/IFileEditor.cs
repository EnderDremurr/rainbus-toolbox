using RainbusToolbox.Models.LimbusSerialization;
using RainbusToolbox.Services.RepositoryServices;

namespace RainbusToolbox.Models;

public interface IFileEditor
{
    public void SetFileToEdit(LocalizationFileBase file);
    public void SetReferenceFile(LocalizationFileBase file);

    public void AskEditorToSave(LocalizationManager localizationManager);
}
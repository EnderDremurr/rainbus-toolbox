using System.Diagnostics;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class UnknownFileTranslationEditorViewModel : ObservableObject, IFileEditor
{
    [ObservableProperty]
    private string _editableJson = string.Empty;

    [ObservableProperty]
    private string _fileTypeName = string.Empty;

    [ObservableProperty]
    private bool _hasParseError;

    [ObservableProperty]
    private bool _isFileLoaded;

    [ObservableProperty]
    private string _parseErrorMessage = string.Empty;

    [ObservableProperty]
    private string _referenceJson = string.Empty;

    public LocalizationFileBase? EditableFile { get; private set; }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        if (EditableFile == null || string.IsNullOrWhiteSpace(EditableFile.FullPath))
            return;

        try
        {
            JsonConvert.DeserializeObject(EditableJson);

            Directory.CreateDirectory(Path.GetDirectoryName(EditableFile.FullPath)!);
            File.WriteAllText(EditableFile.FullPath, EditableJson, Encoding.UTF8);

            HasParseError = false;
            ParseErrorMessage = string.Empty;
        }
        catch (JsonException ex)
        {
            HasParseError = true;
            ParseErrorMessage = $"JSON Parse Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            HasParseError = true;
            ParseErrorMessage = $"Save Error: {ex.Message}";
        }
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        try
        {
            var raw = File.ReadAllText(file.FullPath);
            var parsedJson = JsonConvert.DeserializeObject(raw);
            ReferenceJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

            // Debug output
            Debug.WriteLine($"ReferenceJson set to: {ReferenceJson.Length} characters");

            FileTypeName = file.GetType().Name;
        }
        catch (Exception ex)
        {
            ReferenceJson = $"Error loading reference file: {ex.Message}";
            Debug.WriteLine($"Error in LoadReferenceFile: {ex.Message}");
        }
    }

    public void SetFileToEdit(LocalizationFileBase file)
    {
        try
        {
            EditableFile = file;
            var raw = File.ReadAllText(file.FullPath);
            // Parse the JSON first, then serialize with formatting
            var parsedJson = JsonConvert.DeserializeObject(raw);
            EditableJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
            IsFileLoaded = true;
            HasParseError = false;
            ParseErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            EditableJson = $"Error loading editable file: {ex.Message}";
            HasParseError = true;
            ParseErrorMessage = ex.Message;
        }
    }

    partial void OnEditableJsonChanged(string value)
    {
        HasParseError = false;
        ParseErrorMessage = string.Empty;
    }
}
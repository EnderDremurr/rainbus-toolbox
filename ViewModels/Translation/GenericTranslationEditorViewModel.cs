using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class GenericTranslationEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isFileLoaded;

    [ObservableProperty]
    private string _referenceJson = string.Empty;

    [ObservableProperty]
    private string _editableJson = string.Empty;

    [ObservableProperty]
    private string _fileTypeName = string.Empty;

    [ObservableProperty]
    private bool _hasParseError;

    [ObservableProperty]
    private string _parseErrorMessage = string.Empty;

    private LocalizationFileBase? _editableFile;

    public LocalizationFileBase? EditableFile => _editableFile;

    public void LoadReferenceFile(LocalizationFileBase file)
    {
        try
        {
            var raw = File.ReadAllText(file.FullPath);
            var parsedJson = JsonConvert.DeserializeObject(raw);
            ReferenceJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        
            // Debug output
            System.Diagnostics.Debug.WriteLine($"ReferenceJson set to: {ReferenceJson.Length} characters");
        
            FileTypeName = file.GetType().Name;
        }
        catch (Exception ex)
        {
            ReferenceJson = $"Error loading reference file: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in LoadReferenceFile: {ex.Message}");
        }
    }

    public void LoadEditableFile(LocalizationFileBase file)
    {
        try
        {
            _editableFile = file;
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
        // No validation - let the user handle JSON structure
        // Clear any previous error states
        HasParseError = false;
        ParseErrorMessage = string.Empty;
    }

    public bool SaveEditableFile()
    {
        if (_editableFile == null || string.IsNullOrEmpty(_editableFile.FullPath))
            return false;

        try
        {
            // Validate JSON before saving
            JsonConvert.DeserializeObject(EditableJson);
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(_editableFile.FullPath)!);
            
            // Write the JSON directly to file
            File.WriteAllText(_editableFile.FullPath, EditableJson, System.Text.Encoding.UTF8);
            
            HasParseError = false;
            ParseErrorMessage = string.Empty;
            return true;
        }
        catch (JsonException ex)
        {
            HasParseError = true;
            ParseErrorMessage = $"JSON Parse Error: {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            HasParseError = true;
            ParseErrorMessage = $"Save Error: {ex.Message}";
            return false;
        }
    }
}
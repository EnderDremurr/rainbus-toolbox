using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RainbusToolbox.Utilities.Data;

public partial class EditableGenericIdContent : ObservableObject
{
    private readonly GenericIdContent _originalContent;

    public EditableGenericIdContent(GenericIdContent originalContent)
    {
        _originalContent = originalContent;
        Id = originalContent.Id;
        Content = originalContent.Content;
        EditContent = originalContent.Content;
        IsEditing = false;
    }

    public string Id { get; }

    [ObservableProperty]
    private string _content;

    [ObservableProperty]
    private string _editContent;

    [ObservableProperty]
    private bool _isEditing;

    public void StartEdit()
    {
        EditContent = Content;
        IsEditing = true;
    }

    public void SaveEdit()
    {
        Content = EditContent;
        IsEditing = false;
    }

    public void CancelEdit()
    {
        EditContent = Content;
        IsEditing = false;
    }
}
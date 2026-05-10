using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using RainbusToolbox.Models;

public class AvaloniaEditTextEditor : ITextEditor
{
    private readonly TextEditor _editor;

    public AvaloniaEditTextEditor(TextEditor editor)
    {
        _editor = editor;
    }

    public string Text
    {
        get => _editor.Text;
        set => _editor.Text = value;
    }

    public TextDocument Document => _editor.Document;

    public string SelectedText =>
        _editor.SelectedText;

    public int SelectionStart => _editor.SelectionStart;
    public int SelectionEnd => _editor.SelectionStart + _editor.SelectionLength;

    public void SetSelection(int start, int end)
    {
        _editor.Select(start, end - start);
    }

    public int CaretIndex
    {
        get => _editor.CaretOffset;
        set => _editor.CaretOffset = value;
    }

    public void BeginUndoGroup()
    {
        _editor.Document.UndoStack.StartUndoGroup();
    }

    public void EndUndoGroup()
    {
        _editor.Document.UndoStack.EndUndoGroup();
    }

    public void Cut()
    {
        _editor.Cut();
    }

    public void Copy()
    {
        _editor.Copy();
    }

    public void Paste()
    {
        _editor.Paste();
    }

    public void SetContextMenu(ContextMenu menu)
    {
        _editor.ContextMenu = menu;
    }
}
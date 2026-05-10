using Avalonia.Controls;
using AvaloniaEdit.Document;

namespace RainbusToolbox.Models;

public interface ITextEditor
{
    string Text { get; set; }
    TextDocument Document { get; }

    string SelectedText { get; }

    int SelectionStart { get; }
    int SelectionEnd { get; }

    int CaretIndex { get; set; }

    void SetSelection(int start, int end);

    void BeginUndoGroup();
    void EndUndoGroup();

    void Cut();

    void Copy();

    void Paste();

    void SetContextMenu(ContextMenu menu);
}
using Avalonia.Controls;

namespace RainbusToolbox.Models;

public interface ITextEditor
{
    string Text { get; set; }

    string SelectedText { get; }

    int SelectionStart { get; }
    int SelectionEnd { get; }

    int CaretIndex { get; set; }

    void SetSelection(int start, int end);

    void Cut();

    void Copy();

    void Paste();

    void SetContextMenu(ContextMenu menu);
}
using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace RainbusToolbox.Utilities.Textbox;

public class DocumentTextBindingBehavior : Behavior<TextEditor>
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<DocumentTextBindingBehavior, string>(nameof(Text));

    private TextEditor _textEditor;

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is TextEditor textEditor)
        {
            _textEditor = textEditor;
            _textEditor.TextChanged += TextChanged;
            this.GetObservable(TextProperty).Subscribe(TextPropertyChanged);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (_textEditor != null) _textEditor.TextChanged -= TextChanged;
    }

    private void TextChanged(object sender, EventArgs eventArgs)
    {
        if (_textEditor != null && _textEditor.Document != null) Text = _textEditor.Document.Text;
    }

    private void TextPropertyChanged(string text)
    {
        if (_textEditor != null && text != null)
        {
            // Ensure Document exists before setting text
            if (_textEditor.Document == null) _textEditor.Document = new TextDocument();

            // Temporarily disable event to prevent loops
            _textEditor.TextChanged -= TextChanged;

            var caretOffset = _textEditor.CaretOffset;
            _textEditor.Document.Text = text;
            _textEditor.CaretOffset = Math.Min(caretOffset, _textEditor.Document.TextLength);

            // Re-enable event
            _textEditor.TextChanged += TextChanged;
        }
    }
}
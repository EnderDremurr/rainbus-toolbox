using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using RainbusToolbox.Utilities;

namespace RainbusToolbox.Views.Misc;

public partial class GameTextEditor : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<GameTextEditor, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> EnableSpellCheckProperty =
        AvaloniaProperty.Register<GameTextEditor, bool>(
            nameof(EnableSpellCheck),
            true);

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<GameTextEditor, bool>(
            nameof(IsReadOnly));


    private bool _updating;

    public GameTextEditor()
    {
        InitializeComponent();

        PART_Editor.TextArea.TextView.LineTransformers.Add(new TagColorizer());
        PART_Editor.TextArea.SelectionBrush = Brushes.Brown;
        PART_Editor.TextArea.SelectionCornerRadius = 0;
        PART_Editor.TextArea.SelectionBorder = null;

        var adapter = new AvaloniaEditTextEditor(PART_Editor);


        PART_Editor.TextChanged += (_, _) =>
        {
            if (_updating)
                return;

            _updating = true;
            SetCurrentValue(TextProperty, PART_Editor.Text);
            _updating = false;
        };

        this.GetObservable(IsReadOnlyProperty).Subscribe(isReadOnly =>
        {
            PART_Editor.IsReadOnly = isReadOnly;
            PART_Editor.IsEnabled = true;
        });

        this.GetObservable(IsReadOnlyProperty)
            .CombineLatest(this.GetObservable(EnableSpellCheckProperty))
            .Subscribe(x =>
            {
                var (readOnly, spell) = x;

                if (readOnly)
                {
                    //TODO: Automatically disable spellcheck for the readonly editors
                }
            });

        this.GetObservable(TextProperty).Subscribe(text =>
        {
            if (_updating)
                return;

            var safeText = text ?? string.Empty;

            if (PART_Editor.Text != safeText)
            {
                _updating = true;
                PART_Editor.Text = safeText;
                _updating = false;
            }
        });

        ContextMenuHelper.Attach(adapter, IsReadOnly);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool EnableSpellCheck
    {
        get => GetValue(EnableSpellCheckProperty);
        set => SetValue(EnableSpellCheckProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
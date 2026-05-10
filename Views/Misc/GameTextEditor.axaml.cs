using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using RainbusToolbox.Services.ExternalServices;
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

    private readonly SpellcheckColorizer _spellcheckColorizer;
    private Point _lastRightClick;


    private bool _updating;

    public GameTextEditor()
    {
        InitializeComponent();

        PART_Editor.TextArea.TextView.LineTransformers.Add(new TagColorizer());
        PART_Editor.TextArea.SelectionBrush = Brushes.Brown;
        PART_Editor.TextArea.SelectionCornerRadius = 0;
        PART_Editor.TextArea.SelectionBorder = null;

        _spellcheckColorizer =
            new SpellcheckColorizer(
                (SpellcheckEngine)App.Current.ServiceProvider
                    .GetService(typeof(SpellcheckEngine))!);

        var adapter = new AvaloniaEditTextEditor(PART_Editor);


        PART_Editor.TextChanged += (_, _) =>
        {
            if (_updating)
                return;

            _updating = true;
            SetCurrentValue(TextProperty, PART_Editor.Text);
            _updating = false;
        };

        this.GetObservable(IsReadOnlyProperty)
            .Subscribe(readOnly =>
            {
                PART_Editor.IsReadOnly = readOnly;
                PART_Editor.IsEnabled = true;

                EnableSpellCheck = !readOnly;
            });

        this.GetObservable(EnableSpellCheckProperty)
            .Subscribe(enabled =>
            {
                if (enabled)
                {
                    if (!PART_Editor.TextArea.TextView.LineTransformers.Contains(_spellcheckColorizer))
                        PART_Editor.TextArea.TextView.LineTransformers.Add(_spellcheckColorizer);
                }
                else
                {
                    PART_Editor.TextArea.TextView.LineTransformers.Remove(_spellcheckColorizer);
                }

                PART_Editor.TextArea.TextView.InvalidateVisual();
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

        PART_Editor.PointerPressed += (s, e) =>
        {
            var point = e.GetCurrentPoint(PART_Editor);

            if (!point.Properties.IsRightButtonPressed)
                return;

            var pos = e.GetPosition(PART_Editor);

            var editor = PART_Editor;

            var visualPos = editor.TextArea.TextView.GetPosition(pos);

            if (visualPos is not null)
                editor.CaretOffset =
                    editor.Document.GetOffset(visualPos.Value.Location);
        };

        ContextMenuHelper.Attach(adapter, IsReadOnly,
            (IsReadOnly
                ? null
                : (SpellCheckerService)App.Current.ServiceProvider.GetService(typeof(SpellCheckerService))!)!);
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
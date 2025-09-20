using System;
using Avalonia;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public class BattleAnnouncerTranslationEditorViewModel
    : TranslationEditorViewModel<BattleAnnouncerFile, BattleAnnouncerEntry>
{
    // Original image size (AnnouncerBG.png)
    private const double BaseWidth = 673.0;
    private const double BaseHeight = 246.0;

    // Original margins at base resolution: left=255, top=30, right=0, bottom=45
    private static readonly Thickness BaseDialogueMargin = new Thickness(400, 30, 0, 45);

    private const double MaxDialogueWidth = 900.0;

    /// <summary>
    /// Margin for the reference text box (scaled).
    /// </summary>
    public Thickness ReferenceMargins => CalculateRelativeMargins();

    /// <summary>
    /// Margin for the editor text box (scaled).
    /// </summary>
    public Thickness EditorMargins => CalculateRelativeMargins();

    private Thickness CalculateRelativeMargins()
    {
        // Protect against zero base sizes (shouldn't happen)
        var scaleX = BaseWidth > 0 ? (CurrentImageWidth / BaseWidth) : 1.0;
        var scaleY = BaseHeight > 0 ? (CurrentImageHeight / BaseHeight) : 1.0;

        // Keep the margins scaled but don't allow negative or absurd values.
        double left = Math.Max(0, BaseDialogueMargin.Left * scaleX);
        double top = Math.Max(0, BaseDialogueMargin.Top * scaleY);
        double right = Math.Max(0, BaseDialogueMargin.Right * scaleX);
        double bottom = Math.Max(0, BaseDialogueMargin.Bottom * scaleY);

        return new Thickness(left, top, right, bottom);
    }

    // image size reported by view (defaults to base so layout starts sane)
    private double _currentImageWidth = BaseWidth;
    public double CurrentImageWidth
    {
        get => _currentImageWidth;
        set
        {
            if (Math.Abs(_currentImageWidth - value) > double.Epsilon)
            {
                _currentImageWidth = value;
                OnPropertyChanged(nameof(ReferenceMargins));
                OnPropertyChanged(nameof(EditorMargins));
            }
        }
    }

    private double _currentImageHeight = BaseHeight;
    public double CurrentImageHeight
    {
        get => _currentImageHeight;
        set
        {
            if (Math.Abs(_currentImageHeight - value) > double.Epsilon)
            {
                _currentImageHeight = value;
                OnPropertyChanged(nameof(ReferenceMargins));
                OnPropertyChanged(nameof(EditorMargins));
            }
        }
    }
}

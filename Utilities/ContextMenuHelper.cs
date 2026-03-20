using Avalonia;
using Avalonia.Controls;
using RainbusToolbox.Services;
using RainbusToolbox.ViewModels;
using RainbusToolbox.Views.Misc;
using Serilog;

namespace RainbusToolbox.Utilities;

public static class ContextMenuHelper
{
    public static readonly AttachedProperty<bool> EnableCustomContextMenuProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("EnableCustomContextMenu", typeof(ContextMenuHelper));

    static ContextMenuHelper()
    {
        EnableCustomContextMenuProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is TextBox textBox && args.NewValue.Equals(true)) SetupContextMenu(textBox);
        });
    }

    public static void SetEnableCustomContextMenu(Control element, bool value)
    {
        element.SetValue(EnableCustomContextMenuProperty, value);
    }

    public static bool GetEnableCustomContextMenu(Control element)
    {
        return element.GetValue(EnableCustomContextMenuProperty);
    }

    private static void SetupContextMenu(TextBox textBox)
    {
        var contextMenu = new ContextMenu();


        #region Fonts

        var fontsMenu = new MenuItem { Header = "Шрифты" };

        var pretendardFontItem = new MenuItem { Header = "Pretendard" };
        pretendardFontItem.Click += (s, e) => { WrapInTag("<font=\"Pretendard-Regular SDF\">", "</font>", textBox); };

        var bebasKaiFontItem = new MenuItem { Header = "BebasKai" };
        bebasKaiFontItem.Click += (s, e) => { WrapInTag("<font=\"BebasKai SDF\">", "</font>", textBox); };

        var excelsiorSansFontItem = new MenuItem { Header = "Excelsior Sans" };
        excelsiorSansFontItem.Click += (s, e) => { WrapInTag("<font=\"ExcelsiorSans SDF\">", "</font>", textBox); };

        var mikodacsFontItem = new MenuItem { Header = "Mikodacs" };
        mikodacsFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"en/Title)Mikodacs/Mikodacs SDF\">", "</font>", textBox);
        };

        var notoSerifFontItem = new MenuItem { Header = "Noto Serif" };
        notoSerifFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"common/notoserifkr-semibold sdf\">", "</font>", textBox);
        };

        var katyoubbFontItem = new MenuItem { Header = "Katyoubb" };
        katyoubbFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"jp/cur)katyoubb/katyoubb sdf\">", "</font>", textBox);
        };

        var corporateLogoFontItem = new MenuItem { Header = "Corporate Logo" };
        corporateLogoFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"jp/title)corporate logo(bold)/corporate-logo-bold-ver2 sdf\">", "</font>", textBox);
        };

        var higashiomeFontItem = new MenuItem { Header = "Higashiome" };
        higashiomeFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"jp/higashiome/higashiome-gothic-c-1\">", "</font>", textBox);
        };

        var caveatSemiboldFontItem = new MenuItem { Header = "Caveat Semibold" };
        caveatSemiboldFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"en/cur)caveat-semibold/caveat-semibold sdf\">", "</font>", textBox);
        };

        var cafe24ShiningStarFontItem = new MenuItem { Header = "Cafe 24 Shining Star" };
        cafe24ShiningStarFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"kr/cur)cafe24shiningstar/cafe24shiningstar sdf\">", "</font>", textBox);
        };

        var scDreamFontItem = new MenuItem { Header = "Sc Dream" };
        scDreamFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"kr/p)scdream(light)/scdream5 sdf\">", "</font>", textBox);
        };

        var kotraBoldFontItem = new MenuItem { Header = "Kotra Bold" };
        kotraBoldFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"kr/title)kotra_bold/kotra_bold sdf\">", "</font>", textBox);
        };

        var maryJaneAntiqueFontItem = new MenuItem { Header = "Mary Jane" };
        maryJaneAntiqueFontItem.Click += (s, e) =>
        {
            WrapInTag("<font=\"mary_jane_antique sdf\">", "</font>", textBox);
        };

        fontsMenu.Items.Add(pretendardFontItem);
        fontsMenu.Items.Add(bebasKaiFontItem);
        fontsMenu.Items.Add(excelsiorSansFontItem);
        fontsMenu.Items.Add(mikodacsFontItem);
        fontsMenu.Items.Add(notoSerifFontItem);
        fontsMenu.Items.Add(katyoubbFontItem);
        fontsMenu.Items.Add(corporateLogoFontItem);
        fontsMenu.Items.Add(higashiomeFontItem);
        fontsMenu.Items.Add(caveatSemiboldFontItem);
        fontsMenu.Items.Add(cafe24ShiningStarFontItem);
        fontsMenu.Items.Add(scDreamFontItem);
        fontsMenu.Items.Add(kotraBoldFontItem);
        fontsMenu.Items.Add(maryJaneAntiqueFontItem);

        contextMenu.Items.Add(fontsMenu);

        #endregion

        #region Styles

        var stylesMenu = new MenuItem { Header = "Стили" };
        //TODO: Fill when enough styles are discovered

        #endregion

        #region Tags

        var tagsMenu = new MenuItem { Header = "Теги" };

        var boldItem = new MenuItem { Header = "Жирный" };
        boldItem.Click += (s, e) => WrapInTag("<b>", "</b>", textBox);

        var italicItem = new MenuItem { Header = "Курсив" };
        italicItem.Click += (s, e) => WrapInTag("<i>", "</i>", textBox);

        var strikeItem = new MenuItem { Header = "Зачёркнутый" };
        strikeItem.Click += (s, e) => WrapInTag("<s>", "</s>", textBox);

        var underlineItem = new MenuItem { Header = "Подчёркнутый" };
        underlineItem.Click += (s, e) => WrapInTag("<u>", "</u>", textBox);

        var subItem = new MenuItem { Header = "Подстрочный текст" };
        subItem.Click += (s, e) => WrapInTag("<sub>", "</sub>", textBox);

        var supItem = new MenuItem { Header = "Надстрочный текст" };
        supItem.Click += (s, e) => WrapInTag("<sup>", "</sup>", textBox);

        var rubyItem = new MenuItem { Header = "Текст над другим текстом" };
        rubyItem.Click += (s, e) => WrapInTag("<ruby=ЭТОТ ТЕКСТ БУДЕТ ПОКАЗЫВАТЬСЯ СВЕРХУ>", "</ruby>", textBox);

        var lowercaseItem = new MenuItem { Header = "Обычные буквы" };
        lowercaseItem.Click += (s, e) => WrapInTag("<lowercase>", "</lowercase>", textBox);

        var uppercaseItem = new MenuItem { Header = "Капс" };
        uppercaseItem.Click += (s, e) => WrapInTag("<uppercase>", "</uppercase>", textBox);

        var smallcapsItem = new MenuItem { Header = "Маленький капс" };
        smallcapsItem.Click += (s, e) => WrapInTag("<smallcaps>", "</smallcaps>", textBox);

        var nobrItem = new MenuItem { Header = "Неразрывный" };
        nobrItem.Click += (s, e) => WrapInTag("<nobr>", "</nobr>", textBox);

        var noparseItem = new MenuItem { Header = "Без парсинга" };
        noparseItem.Click += (s, e) => WrapInTag("<noparse>", "</noparse>", textBox);

        var markItem = new MenuItem { Header = "Выделение" };
        markItem.Click += (s, e) => WrapInTag("<mark=#ffff00aa>", "</mark>", textBox);

        var fontWeightItem = new MenuItem { Header = "Толщина шрифта" };
        fontWeightItem.Click += (s, e) => WrapInTag("<font-weight=400>", "</font-weight>", textBox);

        var colorItem = new MenuItem { Header = "Цвет" };
        colorItem.Click += (s, e) => WrapInTag("<color=#ffffff>", "</color>", textBox);

        var alphaItem = new MenuItem { Header = "Прозрачность" };
        alphaItem.Click += (s, e) => WrapInTag("<alpha=#FF>", "</alpha>", textBox);

        var sizeItem = new MenuItem { Header = "Размер" };
        sizeItem.Click += (s, e) => WrapInTag("<size=100%>", "</size>", textBox);

        var pageItem = new MenuItem { Header = "Разрыв страницы" };
        pageItem.Click += (s, e) => WrapInTag("<page>", "", textBox);

        var indentItem = new MenuItem { Header = "Отступ" };
        indentItem.Click += (s, e) => WrapInTag("<indent=10%>", "</indent>", textBox);

        var lineHeightItem = new MenuItem { Header = "Высота строки" };
        lineHeightItem.Click += (s, e) => WrapInTag("<line-height=100%>", "</line-height>", textBox);

        var lineIndentItem = new MenuItem { Header = "Отступ строки" };
        lineIndentItem.Click += (s, e) => WrapInTag("<line-indent=10%>", "</line-indent>", textBox);

        var marginItem = new MenuItem { Header = "Поля" };
        marginItem.Click += (s, e) => WrapInTag("<margin=10px>", "</margin>", textBox);

        var posItem = new MenuItem { Header = "Позиция" };
        posItem.Click += (s, e) => WrapInTag("<pos=50%>", "", textBox);

        var spaceItem = new MenuItem { Header = "Пробел" };
        spaceItem.Click += (s, e) => WrapInTag("<space=10px>", "", textBox);

        var widthItem = new MenuItem { Header = "Ширина" };
        widthItem.Click += (s, e) => WrapInTag("<width=80%>", "</width>", textBox);

        var voffsetItem = new MenuItem { Header = "Вертикальное смещение" };
        voffsetItem.Click += (s, e) => WrapInTag("<voffset=1em>", "</voffset>", textBox);

        var mspaceItem = new MenuItem { Header = "Моноширина" };
        mspaceItem.Click += (s, e) => WrapInTag("<mspace=1em>", "</mspace>", textBox);

        var rotateItem = new MenuItem { Header = "Поворот" };
        rotateItem.Click += (s, e) => WrapInTag("<rotate=15>", "</rotate>", textBox);

        var spriteItem = new MenuItem { Header = "Спрайт" };
        spriteItem.Click += (s, e) => WrapInTag("<sprite index=0>", "", textBox);

        var alignItem = new MenuItem { Header = "Выравнивание" };
        alignItem.Click += (s, e) => WrapInTag("<align=\"center\">", "</align>", textBox);

        var cspaceItem = new MenuItem { Header = "Межбуквенный интервал" };
        cspaceItem.Click += (s, e) => WrapInTag("<cspace=1em>", "</cspace>", textBox);


        var previewItem = new MenuItem { Header = "Превью" };
        previewItem.Click += (s, e) =>
        {
            var text = textBox.Text ?? string.Empty;
            var previewWindow = new RichTextPreviewWindow();
            previewWindow.SetTextToDisplay(text);
            previewWindow.Show();
        };

        tagsMenu.Items.Add(boldItem);
        tagsMenu.Items.Add(italicItem);
        tagsMenu.Items.Add(strikeItem);
        tagsMenu.Items.Add(underlineItem);
        tagsMenu.Items.Add(subItem);
        tagsMenu.Items.Add(supItem);
        tagsMenu.Items.Add(rubyItem);
        tagsMenu.Items.Add(lowercaseItem);
        tagsMenu.Items.Add(uppercaseItem);
        tagsMenu.Items.Add(smallcapsItem);
        tagsMenu.Items.Add(nobrItem);
        tagsMenu.Items.Add(noparseItem);
        tagsMenu.Items.Add(markItem);
        tagsMenu.Items.Add(fontWeightItem);
        tagsMenu.Items.Add(colorItem);
        tagsMenu.Items.Add(alphaItem);
        tagsMenu.Items.Add(sizeItem);
        tagsMenu.Items.Add(pageItem);
        tagsMenu.Items.Add(indentItem);
        tagsMenu.Items.Add(lineHeightItem);
        tagsMenu.Items.Add(lineIndentItem);
        tagsMenu.Items.Add(marginItem);
        tagsMenu.Items.Add(posItem);
        tagsMenu.Items.Add(spaceItem);
        tagsMenu.Items.Add(widthItem);
        tagsMenu.Items.Add(voffsetItem);
        tagsMenu.Items.Add(mspaceItem);
        tagsMenu.Items.Add(rotateItem);
        tagsMenu.Items.Add(spriteItem);
        tagsMenu.Items.Add(alignItem);
        tagsMenu.Items.Add(cspaceItem);


        contextMenu.Items.Add(tagsMenu);
        contextMenu.Items.Add(previewItem);

        #endregion

        contextMenu.Items.Add(new Separator());

        #region Angela

        var angelaItem = new MenuItem { Header = "Позвать Анджелу" };
        angelaItem.Click += async (s, e) =>
        {
            LoadingScreenViewModel.StartLoading("Запрос данных у сервера...");

            try
            {
                await ProcessTextWithAngela(textBox);
            }
            finally
            {
                LoadingScreenViewModel.FinishLoading();
            }
        };
        contextMenu.Items.Add(angelaItem);

        #endregion

        contextMenu.Items.Add(new Separator());

        #region Default actions

        var cutItem = new MenuItem { Header = "Cut" };
        cutItem.Click += (s, e) => textBox.Cut();

        var copyItem = new MenuItem { Header = "Copy" };
        copyItem.Click += (s, e) => textBox.Copy();

        var pasteItem = new MenuItem { Header = "Paste" };
        pasteItem.Click += (s, e) => textBox.Paste();

        contextMenu.Items.Add(cutItem);
        contextMenu.Items.Add(copyItem);
        contextMenu.Items.Add(pasteItem);

        #endregion

        textBox.ContextMenu = contextMenu;
    }

    private static async Task<string> ProcessTextWithAngela(TextBox textBox)
    {
        var text = textBox!.Text ?? string.Empty;

        Log.Debug("Received Angela command.");

        var angela = App.Current.ServiceProvider.GetService(typeof(Angela)) as Angela;
        var response = await angela?.ProcessText(text)! ?? text;

        Log.Debug("Setting new text from angela.");
        textBox.Text = string.IsNullOrWhiteSpace(response) ? text : response;

        return response ?? text;
    }

    private static void WrapInTag(string tagStart, string tagEnd, TextBox textBox)
    {
        var start = textBox.SelectionStart;
        var end = textBox.SelectionEnd;
        var selectedText = textBox.SelectedText;

        // no text selected
        if (start == end) return;

        var wrapped = $"{tagStart}{selectedText}{tagEnd}";
        textBox.Text = textBox.Text!.Remove(start, end - start).Insert(start, wrapped);
        textBox.CaretIndex = start + wrapped.Length;
    }
}
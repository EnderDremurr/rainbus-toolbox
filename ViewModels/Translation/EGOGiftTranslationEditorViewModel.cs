using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class EGOGiftTranslationEditorViewModel : TranslationEditorViewModel<EgoGiftsLocalizationFile, EgoGift>
{
    [ObservableProperty] private Bitmap _currentGiftImage;


    protected override void UpdateReferenceItem()
    {
        if (ReferenceFile != null && CurrentItem != null)
        {
            ReferenceItem = ReferenceFile.DataList.FirstOrDefault(x => x.Id == CurrentItem.Id);

            if (ReferenceItem == null)
            {
                CurrentGiftImage = null;
                return;
            }

            var imageName = ReferenceItem.Name!.Replace("+", "") + " Gift.png";
            var pathToCachedGifts = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RainbusToolbox", "cache", "egogifts");

            var pathToImage = Path.Combine(pathToCachedGifts, imageName);

            if (File.Exists(pathToImage))
            {
                var old = CurrentGiftImage;
                CurrentGiftImage = new Bitmap(pathToImage);
                old?.Dispose();
            }
            else
            {
                CurrentGiftImage = null;
            }
        }

        else
        {
            ReferenceItem = null;
        }
    }
}
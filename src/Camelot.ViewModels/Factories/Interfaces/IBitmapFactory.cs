using Avalonia.Media.Imaging;

namespace Camelot.ViewModels.Factories.Interfaces;

public interface IBitmapFactory
{
    Bitmap Create(string filePath);
}
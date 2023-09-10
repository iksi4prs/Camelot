using System.Drawing;

namespace Camelot.Services.Abstractions;

public interface ISystemIconsService
{
    // There are 3 methods and not only simple one "getter",
    // in order to maintain a cache in the Avalonia level,
    // so cached images are of type Avalonoia and not System.Drawing,
    // which is more efficient.
    // Using information provided by methods below,
    // the Avalonia cache "knows" which key to use for cache.

    enum SystemIconType
    {
        Extension,
        Application
    }
    SystemIconType GetIconType(string filename);
    Image GetIconForApplication(string pathToExe);
    Image GetIconForExtension(string extension);
}
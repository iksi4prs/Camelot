using System.Drawing;

namespace Camelot.Services.Abstractions;

public interface ISystemIconsService
{
    // There are 3 methods and not only simple one "getter",
    // in order to maintain a cache in the Avalonia level,
    // so cached images are of type Avalonia and not System.Drawing,
    // which is more efficient.
    // Using information provided by methods below,
    // the Avalonia cache "knows" which key to use for cache.

    enum SystemIconType
    {
        Extension,
        FullPath,
    }
    SystemIconType GetIconType(string filename);
    Image GetIconForPath(string path);
    Image GetIconForExtension(string extension);
}
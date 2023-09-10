using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Camelot.Services.Abstractions;
using Camelot.Services.Windows.SystemIcons;

namespace Camelot.Services.Windows;

[SupportedOSPlatform("windows")]
public class WindowsSystemIconsService : ISystemIconsService
{
    public Image GetIconForExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentNullException(nameof(extension));
        if (!extension.StartsWith("."))
            throw new ArgumentOutOfRangeException(nameof(extension));

        var iconFilename = ShellIcon.GetIconForExtension(extension);

        if (string.IsNullOrEmpty(iconFilename))
        {
            // shell has no ico for this one
            return null;
        }

        if (UWPIcon.IsPriString(iconFilename))
        {
            iconFilename = UWPIcon.ReslovePackageResource(iconFilename);
        }
        return LoadIcon(iconFilename);
    }

    public Image GetIconForPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        if (GetIconType(path) != ISystemIconsService.SystemIconType.Path)
            throw new ArgumentOutOfRangeException(nameof(path));

        var ext = Path.GetExtension(path).ToLower();
        if (ext == ".lnk")
        {
            var resolved = ShellLink.ResolveLink(path);
            // need again to check if resolved is extentsion or path
            var type = GetIconType(resolved);
            if (type == ISystemIconsService.SystemIconType.Extension)
            {
                var ext2 = Path.GetExtension(resolved);
                return GetIconForExtension(ext2);
            }
            else
            {
                // Continue with flow for path.
                // Check if resolved still exists, sometimes the target of .lnk files
                // dont exist anymore
                if (!File.Exists(resolved))
                {
                    // WIP333 - return some SVG
                    return null;
                }
                path = resolved;
            }
        }

        return LoadIcon(path);
    }

    private Image LoadIcon(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        Image result;

        var needsExtract = WindowsIconTypes.IsIconThatRequiresExtract(path);
        if (needsExtract)
        {
            var icon = IconExtractor.ExtractIcon(path);
            // WIP333
            // looks like bit lossy ??
            // try other options ?
            // https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imageconverter.canconvertfrom?view=dotnet-plat-ext-7.0
            result = icon.ToBitmap();
        }
        else
        {
            result = new Bitmap(path);
        }
        return result;
    }

    public ISystemIconsService.SystemIconType GetIconType(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentNullException(nameof(filename));
        
        var ext = Path.GetExtension(filename).ToLower();

        // next extensions require that the icon will be resolved by full path,
        // and not just the extension itself.
        var extensionForFullPaths = new string[] { ".exe", ".lnk", ".cpl", ".appref-ms", ".msc" };
        if (extensionForFullPaths.Contains(ext))
            return ISystemIconsService.SystemIconType.Path;
        
        return ISystemIconsService.SystemIconType.Extension;
    }
}
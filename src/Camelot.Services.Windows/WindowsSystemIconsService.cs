using System;
using System.Drawing;
using System.IO;
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

    public Image GetIconForApplication(string pathToExe)
    {
        if (string.IsNullOrEmpty(pathToExe))
            throw new ArgumentNullException(nameof(pathToExe));

        if (GetIconType(pathToExe) != ISystemIconsService.SystemIconType.Application)
            throw new ArgumentOutOfRangeException(nameof(pathToExe));

        return LoadIcon(pathToExe);
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
        
        var ext = Path.GetExtension(filename);
        if (ext != null && ext.ToLower() == ".exe")
            return ISystemIconsService.SystemIconType.Application;
        
        return ISystemIconsService.SystemIconType.Extension;
    }
}
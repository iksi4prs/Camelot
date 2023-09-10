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

    public WindowsSystemIconsService()
    {
    }


    public Image GetIconForExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentNullException(nameof(extension));
        if (!extension.StartsWith("."))
            throw new ArgumentOutOfRangeException(nameof(extension));

       var iconFilename = ShellIcon.GetIconForExtension(extension);

        // WIP333 rermove debug code
        if (string.IsNullOrEmpty(iconFilename))
       {
            // shell has no ico for this one
            // todo - return some dummy
            return null;
       }
       if (extension == ".png")
        {
            int dbg = 0;
            dbg = 2;
        }

        if (extension == ".mp3")
        {
            int dbg = 0;
            dbg = 2;
        }
        if (iconFilename.StartsWith("@{"))
        {
            
            // TODO - add support for app store (UWP) - X3X3X3
            // see LogoUriFromManifest in .cs file of lucy
           return new Bitmap("C:/MyProjects/FilesCommander/uwp.bmp");
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

    private Image LoadIcon(string iconFilenameOrUwpUri)
    {
        if (string.IsNullOrEmpty(iconFilenameOrUwpUri))
            throw new ArgumentNullException(nameof(iconFilenameOrUwpUri));

        Image result;
        var needsExtract = WindowsIconTypes.IsIconThatRequiresExtract(iconFilenameOrUwpUri);
        if (needsExtract)
        {
            var icon = IconExtractor.ExtractIcon(iconFilenameOrUwpUri);
            // WIP333
            // looks like bit lossy ??
            // try other options ?
            // https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imageconverter.canconvertfrom?view=dotnet-plat-ext-7.0

            result = icon.ToBitmap();
        }
        else
        {
            // WIP333 remove
            result = new Bitmap("C:/MyProjects/FilesCommander/test333.bmp");
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
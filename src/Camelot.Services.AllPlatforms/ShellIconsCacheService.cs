using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Drives;
using Camelot.Services.Abstractions.Operations;
using System.Drawing.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Rectangle = System.Drawing.Rectangle;
using AvaloniaPixelFormat = Avalonia.Platform.PixelFormat;
using AlphaFormat = Avalonia.Platform.AlphaFormat;
using PixelSize = Avalonia.PixelSize;
using Vector = Avalonia.Vector;
using System.IO;
using System.Runtime.CompilerServices;
using System;


namespace Camelot.Services.AllPlatforms;

public class ShellIconsCacheService : IShellIconsCacheService
{
    private readonly IShellLinksService _shellLinksService;
    private readonly ISystemIconsService _systemIconsService;

    public ShellIconsCacheService(
        IShellLinksService shellLinksService,
        ISystemIconsService systemIconsService)
    {
        _shellLinksService = shellLinksService;
        _systemIconsService = systemIconsService;
    }

    // result is always Avalonia's Bitmap, but since interface is defined
    // in "Camelot.Services.Abstractions" which is not referncing Avalonia,
    // the return type is 'object'
    public object GetIcon(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentNullException(nameof(filename));

        return GetShellIcon(filename);
    }
    private Bitmap GetShellIcon(string filename)
    {
        Bitmap result = null;
        string path = filename;
        var isLink = _shellLinksService.IsShellLink(filename);
        if (isLink)
        {
            var resolved = _shellLinksService.ResolveLink(filename);
            // Check if resolved still exists, sometimes the target of .lnk files
            // dont exist anymore, or links to a folder
            if (!File.Exists(resolved))
            {
                if (Directory.Exists(resolved))
                {
                    int dbg = 9;
                    dbg = 8;
                }
                return null;
            }
            // cotinue with new path
            path = resolved;
        }

        /*              {
      var resolved = ShellLink.ResolveLink(path);
      // need again to check if resolved is extension or path,
      // or maybe even a folder.
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
              return null;
          }
          path = resolved;
      }
  }
          }
  */
        // TODO WIP333
        // first - check if in cache
        // TODO WIP333
        // depending on implemntaion of Limux,
        // maybe move cache to common layer, eg
        // SystemIconsCache (impelmeted under the "Camelot.Services.AllPlatforms" ?)

        // not in cache, get from system/shell
        System.Drawing.Image image;
        var t = _systemIconsService.GetIconType(path);
        switch (t)
        {
            case ISystemIconsService.SystemIconType.Extension:
                var ext = System.IO.Path.GetExtension(path);
                if (string.IsNullOrEmpty(ext))
                {
                    // a file with no extension. caller should use other icon.
                    return null;
                }
                else
                {
                    image = _systemIconsService.GetIconForExtension(ext);
                }
                break;
            case ISystemIconsService.SystemIconType.FullPath:
                image = _systemIconsService.GetIconForPath(path);
                break;
            default:
                throw new System.Exception();
        }
        result = ConvertToAvaloniaBitmap(image);
        return result;
    }


    // TODO WIP333 - move to extnetsion method or helper file
    // see https://github.com/AvaloniaUI/Avalonia/discussions/5908
    public static Bitmap ConvertToAvaloniaBitmap(System.Drawing.Image bitmap)
    {
        if (bitmap == null)
            return null;
        var bitmapTmp = new System.Drawing.Bitmap(bitmap);
        var bitmapdata = bitmapTmp.LockBits(
            new Rectangle(0, 0, bitmapTmp.Width, bitmapTmp.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        Bitmap bitmap1 = new Bitmap(AvaloniaPixelFormat.Bgra8888,
            AlphaFormat.Unpremul,
            bitmapdata.Scan0,
            new PixelSize(bitmapdata.Width, bitmapdata.Height),
            new Vector(96, 96),
            bitmapdata.Stride);
        bitmapTmp.UnlockBits(bitmapdata);
        bitmapTmp.Dispose();
        return bitmap1;
    }
}
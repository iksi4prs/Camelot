using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
using Camelot.Services.Abstractions;
//using Camelot.Services.Abstractions.Drives;
//using Camelot.Services.Abstractions.Operations;
using System.Drawing.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Rectangle = System.Drawing.Rectangle;
using AvaloniaPixelFormat = Avalonia.Platform.PixelFormat;
using AlphaFormat = Avalonia.Platform.AlphaFormat;
using PixelSize = Avalonia.PixelSize;
using Vector = Avalonia.Vector;
using System.IO;
using Avalonia.Controls;
using Camelot.Services.Abstractions.Models;
//using System.Runtime.CompilerServices;
//using System;
using Camelot.Images;

namespace Camelot.Services.AllPlatforms;

public class ShellIconsCacheService : IShellIconsCacheService
{
    private readonly IShellLinksService _shellLinksService;
    private readonly ISystemIconsService _systemIconsService;
    private readonly Dictionary<string, Bitmap> _cache = new();
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
    public ImageModel GetIcon(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentNullException(nameof(filename));

        var bitmap = GetShellIcon(filename);
        var result = new ConcreteImage(bitmap);
        return result;
    }
    private Bitmap GetShellIcon(string filename)
    {
        Bitmap result;

        // step #1
        // resolve links, if any
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

        // step #2
        // check if cache, and if not, get from shell.
        // IMPORTANT:
        // keys in cache are both extensions only, and full paths,
        // based on result returned from shell.
        // eg, on Windows all .txt files will have same shell icon,
        // but each .exe will have its own icon (if was embdded in resource of .exe)
        var iconType = _systemIconsService.GetIconType(path);
        switch (iconType)
        {
            case ISystemIconsService.SystemIconType.Extension:
                {
                    var ext = Path.GetExtension(path);
                    if (string.IsNullOrEmpty(ext))
                    {
                        // a file with no extension. caller should use other icon.
                        return null;
                    }
                    else
                    {
                        if (_cache.ContainsKey(ext))
                        {
                            result = _cache[ext];
                        }
                        else
                        {
                            System.Drawing.Image image = _systemIconsService.GetIconForExtension(ext);
                            result = ConvertToAvaloniaBitmap(image);
                            _cache[ext] = result;
                        }
                    }
                }
                break;
            case ISystemIconsService.SystemIconType.FullPath:
                {
                    if (_cache.ContainsKey(path))
                    {
                        result = _cache[path];
                    }
                    else
                    {
                        System.Drawing.Image image = _systemIconsService.GetIconForPath(path);
                        result = ConvertToAvaloniaBitmap(image);
                        _cache[path] = result;
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(iconType));
        }
        
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
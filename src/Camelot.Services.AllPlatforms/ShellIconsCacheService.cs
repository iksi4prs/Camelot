using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
using Camelot.Services.Abstractions;
//using Camelot.Services.Abstractions.Drives;
//using Camelot.Services.Abstractions.Operations;

using System.IO;
using Avalonia.Controls;
using Camelot.Services.Abstractions.Models;
//using System.Runtime.CompilerServices;
//using System;
using Camelot.Images;
using Avalonia.Media.Imaging;

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
        Bitmap result = null;

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
                            var image = _systemIconsService.GetIconForExtension(ext);
                            if (image != null)
                            {
                                var concreteImage = image as ConcreteImage;
                                if (concreteImage == null)
                                    throw new InvalidCastException();
                                result = concreteImage.Bitmap;
                            }
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
                        var image = _systemIconsService.GetIconForPath(path);
                        if (image != null)
                        {
                            var concreteImage = image as ConcreteImage;
                            if (concreteImage == null)
                                throw new InvalidCastException();
                            result = concreteImage.Bitmap;
                        }
                        _cache[path] = result;
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(iconType));
        }
        
        return result;
    }
}
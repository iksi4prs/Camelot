using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Behaviors;
using Camelot.Services.Abstractions.Models.Enums;
using Camelot.ViewModels.Interfaces.Behaviors;
using Camelot.ViewModels.Interfaces.MainWindow.FilePanels.Nodes;
using Camelot.ViewModels.Services.Interfaces;
using ReactiveUI;
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

namespace Camelot.ViewModels.Implementations.MainWindow.FilePanels.Nodes;

public class FileViewModel : FileSystemNodeViewModelBase, IFileViewModel
{
    private readonly IFileSizeFormatter _fileSizeFormatter;
    private readonly IFileTypeMapper _fileTypeMapper;
    private readonly ISystemIconsService _systemIconsService;
    private readonly IShellLinksService _shellLinksService;
    private long _size;
    private Bitmap _systemIcon = null;
    // Helper to load icon only on demand.
    // Can't use icon member itself, since null is valid value,
    // in case file has no shell icon.
    private bool _loadedShellIcon = false;
    public string Extension { get; set; }

    public long Size
    {
        get => _size;
        set
        {
            this.RaiseAndSetIfChanged(ref _size, value);
            this.RaisePropertyChanged(nameof(FormattedSize));
        }
    }

    public string FormattedSize => _fileSizeFormatter.GetFormattedSize(Size);

    public FileContentType Type => _fileTypeMapper.GetFileType(Extension);

    public FileViewModel(
        IFileSystemNodeOpeningBehavior fileSystemNodeOpeningBehavior,
        IFileSystemNodePropertiesBehavior fileSystemNodePropertiesBehavior,
        IFileSystemNodeFacade fileSystemNodeFacade,
        bool shouldShowOpenSubmenu,
        IFileSizeFormatter fileSizeFormatter,
        IFileTypeMapper fileTypeMapper,
        ISystemIconsService systemIconsService,
        IShellLinksService shellLinksService)
        : base(
            fileSystemNodeOpeningBehavior,
            fileSystemNodePropertiesBehavior,
            fileSystemNodeFacade,
            shouldShowOpenSubmenu)
    {
        _fileSizeFormatter = fileSizeFormatter;
        _fileTypeMapper = fileTypeMapper;
        _systemIconsService = systemIconsService;
        _shellLinksService = shellLinksService;
    }


    private Bitmap GetShellIcon()
    {
        Bitmap result = null;
        string path = FullPath;
        var isLink = _shellLinksService.IsShellLink(FullPath);
        if (isLink)
        {
            var resolved = _shellLinksService.ResolveLink(FullPath);
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
    public Bitmap SystemIcon
    {
        get
        {
            if (!_loadedShellIcon)
            {
                _systemIcon = GetShellIcon();
                _loadedShellIcon = true;
            }
            return _systemIcon;
        }
    }

    public bool UseSystemIcons
    {
        get
        {
            // TODO WIP333
            // 1) check if window
            // 2) check value from settings

            // if not first time, and already have value
            if (!_loadedShellIcon)
            {
                _systemIcon = GetShellIcon();
                _loadedShellIcon = true;
            }

            if (_systemIcon != null)
            {
                return true;
            }
            else
            {
                // file has no shell icon, so fallback to use builtin icons
                return false;
            }
        }
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
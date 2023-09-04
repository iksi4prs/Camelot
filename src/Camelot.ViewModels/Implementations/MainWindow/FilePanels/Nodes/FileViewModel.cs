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

namespace Camelot.ViewModels.Implementations.MainWindow.FilePanels.Nodes;

public class FileViewModel : FileSystemNodeViewModelBase, IFileViewModel
{
    private readonly IFileSizeFormatter _fileSizeFormatter;
    private readonly IFileTypeMapper _fileTypeMapper;
    private readonly ISystemIconsService _systemIconsService;
    private long _size;

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
        ISystemIconsService systemIconsService)
        : base(
            fileSystemNodeOpeningBehavior,
            fileSystemNodePropertiesBehavior,
            fileSystemNodeFacade,
            shouldShowOpenSubmenu)
    {
        _fileSizeFormatter = fileSizeFormatter;
        _fileTypeMapper = fileTypeMapper;
        _systemIconsService = systemIconsService;
    }

    
    private Bitmap _systemIcon = null;
    public Bitmap SystemIcon
    {
        get
        {
            if (_systemIcon == null)
            {
                // first - check if in cache
                // TODO

                // not in cache, get from system/shell
                var task = _systemIconsService.GetFileIcon(FullPath);
                task.Wait();
                var bitmap = task.Result;
                _systemIcon = ConvertToAvaloniaBitmap(bitmap);
            }
            return _systemIcon;
        }
    }

    public bool UseSystemIcons
    {
        get
        {
            // TODO WIP
            // 1) check if window
            // 2) check value from settings
            return true;
        }
    }

    // TODO WIP - move to extnetsion method or helper file
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
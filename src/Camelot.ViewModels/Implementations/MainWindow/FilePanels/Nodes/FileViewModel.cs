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

// WIP333 some cleanup in this file
//using System.Drawing.Imaging;
//using Bitmap = Avalonia.Media.Imaging.Bitmap;
//using Rectangle = System.Drawing.Rectangle;
//using AvaloniaPixelFormat = Avalonia.Platform.PixelFormat;
//using AlphaFormat = Avalonia.Platform.AlphaFormat;
//using PixelSize = Avalonia.PixelSize;
//using Vector = Avalonia.Vector;
//using System.IO;
//using System.Runtime.CompilerServices;
//using System;

namespace Camelot.ViewModels.Implementations.MainWindow.FilePanels.Nodes;

public class FileViewModel : FileSystemNodeViewModelBase, IFileViewModel
{
    private readonly IFileSizeFormatter _fileSizeFormatter;
    private readonly IFileTypeMapper _fileTypeMapper;
    //private readonly ISystemIconsService _systemIconsService;
    //private readonly IShellLinksService _shellLinksService;
    private readonly IShellIconsCacheService _shellIconsCacheService;
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
        IShellLinksService shellLinksService,
        IShellIconsCacheService shellIconsCacheService)
        : base(
            fileSystemNodeOpeningBehavior,
            fileSystemNodePropertiesBehavior,
            fileSystemNodeFacade,
            shouldShowOpenSubmenu)
    {
        _fileSizeFormatter = fileSizeFormatter;
        _fileTypeMapper = fileTypeMapper;
        // WIP333 remove next 2, if not needed
        //_systemIconsService = systemIconsService;
        //_shellLinksService = shellLinksService;
        _shellIconsCacheService = shellIconsCacheService;
    }



    public Bitmap SystemIcon
    {
        get
        {
            if (!_loadedShellIcon)
            {
                _systemIcon = (Bitmap)_shellIconsCacheService.GetIcon(FullPath);
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
            // 1) check if windows os
            // 2) check value from settings

            // if not first time, and already have value
            if (!_loadedShellIcon)
            {
                _systemIcon = (Bitmap)_shellIconsCacheService.GetIcon(FullPath);
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
}
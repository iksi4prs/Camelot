using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Camelot.Images;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Behaviors;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.Enums;
using Camelot.ViewModels.Interfaces.Behaviors;
using Camelot.ViewModels.Interfaces.MainWindow.FilePanels.Nodes;
using Camelot.ViewModels.Services.Interfaces;
using ReactiveUI;
using System;

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
    private readonly IShellIconsCacheService _shellIconsCacheService;
    private readonly IIconsService _iconsService;
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
        IShellIconsCacheService shellIconsCacheService,
        IIconsService iconsService)
        : base(
            fileSystemNodeOpeningBehavior,
            fileSystemNodePropertiesBehavior,
            fileSystemNodeFacade,
            shouldShowOpenSubmenu)
    {
        _fileSizeFormatter = fileSizeFormatter;
        _fileTypeMapper = fileTypeMapper;
        _shellIconsCacheService = shellIconsCacheService;
        _iconsService = iconsService;
    }



    public Bitmap SystemIcon
    {
        get
        {
            if (!_loadedShellIcon)
            {
                var imageModel = _shellIconsCacheService.GetIcon(FullPath);
                _systemIcon = FromImageModel(imageModel);
                _loadedShellIcon = true;
            }
            return _systemIcon;
        }
    }

    private IconsType? _iconsType = null;
    private IconsType GetUserSelectedType()
    {
        // TODO - later - how to reflect without restart ?
        // check only once
        if (_iconsType == null)
        {
            var model = _iconsService.GetIconsSettings();
            if (model == null)
            {
                _iconsType = IconsType.Shell;
            }
            else
            {
                _iconsType = model.SelectedIconsType;
            }
        }
        return (IconsType)_iconsType;
    }
    public bool UseSystemIcons
    {
        get
        {
            var selected = GetUserSelectedType();
            if (selected == IconsType.Builtin)
                return false;
            
            // still need to some check, before can return true
            // if not first time, and already have value
            if (!_loadedShellIcon)
            {
                var imageModel = _shellIconsCacheService.GetIcon(FullPath);
                _systemIcon = FromImageModel(imageModel);
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

    // WIP333 maybe this, and the opposye should be in new file ?
    // ImageModelConverter.cs ?
    // BETTER put under new folder, not "Converters" so not to confuse...
    // can put under folder Converters, but add not this is not IValueConverter
    // which used in xaml
    private static Bitmap FromImageModel(ImageModel imageModel)
    {
        if (imageModel == null)
            throw new ArgumentNullException(nameof(imageModel));

        var concreteImage = imageModel as ConcreteImage;
        var result = concreteImage.Bitmap;
        return result;
    }
}
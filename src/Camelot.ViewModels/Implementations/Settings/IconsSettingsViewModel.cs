﻿using System.Collections.Generic;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.Enums;
using Camelot.ViewModels.Interfaces.Settings;
using ReactiveUI.Fody.Helpers;

namespace Camelot.ViewModels.Implementations.Settings;

public class IconsSettingsViewModel : ViewModelBase, ISettingsViewModel
{
    private readonly IIconsService _iconsService;
    private IconsType _initialIconsType;

    private bool _isActivated;

    [Reactive]
    public IconsType CurrentIconsType { get; set; }

    public IEnumerable<IconsType> IconsTypeOptions
    {
        get
        {
            return new []{ IconsType.Builtin, IconsType.Shell };
        }
    }

    public bool IsChanged => _initialIconsType != CurrentIconsType;
    
    public IconsSettingsViewModel(
        IIconsService iconsService)
    {
        _iconsService = iconsService;
    }

    public void Activate()
    {
        if (_isActivated)
        {
            return;
        }

        _isActivated = true;

        var model = _iconsService.GetIconsSettings();
        _initialIconsType = model.SelectedIconsType;
        CurrentIconsType = _initialIconsType;
    }

    public void SaveChanges()
    {
        var model = new IconsSettingsModel(CurrentIconsType);
        _iconsService.SaveIconsSettings(model);
    }
}
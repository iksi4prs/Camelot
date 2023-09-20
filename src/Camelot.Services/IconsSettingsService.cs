using System;
using Camelot.DataAccess.UnitOfWork;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.Enums;

namespace Camelot.Services;

public class IconsSettingsService : IIconsSettingsService
{
    private const string SettingsId = "IconsSettings";
    private readonly IconsSettingsModel _default;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private IconsSettingsModel _value = null;

    public IconsSettingsService(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _default = new IconsSettingsModel(IconsType.Shell);
    }

    
    public IconsSettingsModel GetIconsSettings()
    {
        if (_value == null)
        {
            using var uow = _unitOfWorkFactory.Create();
            var repository = uow.GetRepository<IconsSettingsModel>();
            var dbModel = repository.GetById(SettingsId);
            if (dbModel != null)
                _value = dbModel;
            else
                _value = _default;
        }
        return _value;
    }

    public void SaveIconsSettings(IconsSettingsModel iconsSettingsModel)
    {
        if (iconsSettingsModel is null)
        {
            throw new ArgumentNullException(nameof(iconsSettingsModel));
        }

        using var uow = _unitOfWorkFactory.Create();
        var repository = uow.GetRepository<IconsSettingsModel>();
        repository.Upsert(SettingsId, iconsSettingsModel);
        _value = iconsSettingsModel;
    }
}
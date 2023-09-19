using System;
using Camelot.DataAccess.UnitOfWork;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.Enums;

namespace Camelot.Services;

public class IconsService : IIconsService
{
    private const string SettingsId = "IconsSettings";
    private readonly IconsSettingsModel _default;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public IconsService(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _default = new IconsSettingsModel(IconsType.Shell);
    }

    
    public IconsSettingsModel GetIconsSettings()
    {
        using var uow = _unitOfWorkFactory.Create();
        var repository = uow.GetRepository<IconsSettingsModel>();
        var dbModel = repository.GetById(SettingsId);
        if (dbModel != null)
            return dbModel;
        else
            return _default;
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
    }
}
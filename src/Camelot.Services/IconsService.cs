using System;
using Camelot.DataAccess.UnitOfWork;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models;

namespace Camelot.Services;

public class IconsService : IIconsService
{
    private const string SettingsId = "IconsSettings";

    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public IconsService(
        IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }


    public IconsSettingsModel GetIconsSettings()
    {
        using var uow = _unitOfWorkFactory.Create();
        var repository = uow.GetRepository<IconsSettingsModel>();
        var dbModel = repository.GetById(SettingsId);
        return dbModel;
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
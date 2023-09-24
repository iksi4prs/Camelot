using System;
using System.Collections.Generic;
using Camelot.DataAccess.UnitOfWork;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.Enums;

namespace Camelot.Services;

public class QuickSearchService : IQuickSearchService
{
    private const string SettingsId = "QuickSearchSettings";
    private readonly QuickSearchModel _default;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private QuickSearchModel _cachedValue = null;

    public QuickSearchService(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _default = new QuickSearchModel(QuickSearchMode.Letter);
        GetQuickSearchSettings();
    }

    public bool Enabled()
    {
        return _cachedValue.SelectedMode != QuickSearchMode.Disabled;
    }

    public QuickSearchModel GetQuickSearchSettings()
    {
        if (_cachedValue == null)
        {
            using var uow = _unitOfWorkFactory.Create();
            var repository = uow.GetRepository<QuickSearchModel>();
            var dbModel = repository.GetById(SettingsId);
            if (dbModel != null)
                _cachedValue = dbModel;
            else
                _cachedValue = _default;
        }
        else
        {
            // we set value of _cachedValue in 'save',
            // so no need to read from the repository every time.
        }
        return _cachedValue;
    }

    public void OnCharDown(char c, 
        List<QuickSearchFileModel> files,
        out bool handled)
    {
        if (!Enabled())
        {
            handled = false;
            return;
        }

        // Filter out, all files that dont start with the typed key
        //var files = ViewModel.FileSystemNodes;
        foreach (var file in files)
        {
            var name = file.Name.ToLower();
            if (name.StartsWith(c))
            {
                file.Found = true;
                // how to disable row ??
                // not possible ??
                // https://github.com/AvaloniaUI/Avalonia/issues/7766
                //var x = FilesDataGrid.
            }
            else
            {
                file.Found = false;
            }
        }

        //// 
        //var items = FilesDataGrid.Items;
        //foreach (var item in items)
        //{
        //    var file = item as FileViewModel;
        //    var dir = item as DirectoryViewModel;
        //    //var model = GetNode(item.)
        //}

        // Go to next item in filtered items
        // WIP555
        //if ((modifiers & KeyModifiers.Shift) == 0)
        {
            // WIP777 = how to do this from here ???
            //ViewModel.SelectNextItem();
        }
        //else
        //{
        //    //ViewModel.SelectPreviousItem();
        //}

        handled = true;
    }

    public void OnEscapeKeyDown(
        List<QuickSearchFileModel> files,
        out bool handled)
    {
        if (!Enabled())
        {
            handled = false;
            return;
        }

        // Mark all as found, to clear filter
        foreach (var file in files)
        {
            file.Found = true;
        }
        handled = true;
    }

    public void SaveQuickSearchSettings(QuickSearchModel quickSearchModel)
    {
        if (quickSearchModel is null)
        {
            throw new ArgumentNullException(nameof(quickSearchModel));
        }

        using var uow = _unitOfWorkFactory.Create();
        var repository = uow.GetRepository<QuickSearchModel>();
        repository.Upsert(SettingsId, quickSearchModel);
        _cachedValue = quickSearchModel;
    }
}
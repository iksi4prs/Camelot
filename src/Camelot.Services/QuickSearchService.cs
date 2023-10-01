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
    private QuickSearchModel _cachedSettingsValue = null;
    private string _searchString = string.Empty;

    public QuickSearchService(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _default = new QuickSearchModel(QuickSearchMode.Letter);
        GetQuickSearchSettings();
    }

    public bool Enabled()
    {
        return _cachedSettingsValue.SelectedMode != QuickSearchMode.Disabled;
    }

    public QuickSearchModel GetQuickSearchSettings()
    {
        if (_cachedSettingsValue == null)
        {
            using var uow = _unitOfWorkFactory.Create();
            var repository = uow.GetRepository<QuickSearchModel>();
            var dbModel = repository.GetById(SettingsId);
            if (dbModel != null)
                _cachedSettingsValue = dbModel;
            else
                _cachedSettingsValue = _default;
        }
        else
        {
            // we set value of _cachedValue in 'save',
            // so no need to read from the repository every time.
        }
        return _cachedSettingsValue;
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

        c = Char.ToLower(c);
        switch(_cachedSettingsValue.SelectedMode)
        {
            case QuickSearchMode.Letter:
                {
                    if (_searchString.Length > 0)
                    {
                        if (_searchString[0] != c)
                        {
                            _searchString = string.Empty;
                        }
                    }
                    // Keep repeats, to know which one to select
                    _searchString += c;
                    break;
                }
            case QuickSearchMode.Word:
                {
                    _searchString += c;
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }


        // Step #1
        // 2 values computed in loop:
        // Value #1 - set 'Found' - whether file was found in quick search, namely start with the typed letter/word
        // Value #2 - reset all values of 'Selected' to false (value set is later)
        int found = 0;
        for (int i=0; i<files.Count; i++)
        {
            var file = files[i];
            file.Selected = false;
            if (IncludeInSearchResults(file))
            {
                file.Found = true;
                found++;
                // WIP777 - move this comment to somewhere else ??
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

        // Step #2
        // Set 'Selected' - which file should be selected by ui.
        // Reminders:
        // a. Also for letter, we keep repeats in the string.
        // b. We use modolu, so if previously selected item was last,
        //    we move selection to first item, so will be more intuitive to user.
        int selectedItemOrder = _searchString.Length % found + 1;
        int counter = 0;
        foreach(var file in files)
        {
            if (file.Found)
            {
                counter++;
                if (counter == selectedItemOrder)
                {
                    file.Selected = true;
                    break;
                }
            }
        }
        

        handled = true;
    }

    private bool IncludeInSearchResults(QuickSearchFileModel file)
    {
        switch (_cachedSettingsValue.SelectedMode)
        {
            case QuickSearchMode.Letter:
               return _searchString[0] == Char.ToLower(file.Name[0]);
            case QuickSearchMode.Word:
               return _searchString.StartsWith(file.Name, StringComparison.OrdinalIgnoreCase);
            default:
                throw new ArgumentOutOfRangeException();
        }
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

        _searchString = string.Empty;
        // Mark all as found, to clear filter
        foreach (var file in files)
        {
            file.Found = true;
            file.Selected = false;
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
        _cachedSettingsValue = quickSearchModel;
    }
}
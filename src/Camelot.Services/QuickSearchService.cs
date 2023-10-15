using System;
using System.Collections.Generic;
using System.Linq;
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
    private string _searchWord = string.Empty;
    private char _searchLetter = Char.MinValue;
    private int _selectedIndex = -1;
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
        bool isShiftDown,
        List<QuickSearchFileModel> files,
        out bool handled)
    {
        if (!Enabled())
        {
            handled = false;
            return;
        }

        if (files == null)
            throw new ArgumentNullException(nameof(files));
        
        c = Char.ToLower(c);
        switch(_cachedSettingsValue.SelectedMode)
        {
            case QuickSearchMode.Letter:
                {
                    if (_searchLetter != c)
                    {
                        _selectedIndex = -1;
                    }
                    _searchLetter = c;
                    break;
                }
            case QuickSearchMode.Word:
                {
                    _searchWord += c;
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
        SearchFilesAndSetFound(files);
        SetSelectedItem(files, isShiftDown);
        handled = true;
    }

    /// <summary>
    /// Set value of <see cref="QuickSearchFileModel.Found"/> 
    /// which indicates whether file was found in quick search,
    /// namely start with the typed letter/word
    /// </summary>
    private void SearchFilesAndSetFound(List<QuickSearchFileModel> files)
    {
        if (files == null)
            throw new ArgumentNullException(nameof(files));

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            if (IncludeInSearchResults(file))
            {
                file.Found = true;
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
    }

    /// <summary>
    /// Set value of <see cref="QuickSearchFileModel.Selected"/> 
    /// which indicates to UI which item should be selected.
    /// </summary>

    private void SetSelectedItem(List<QuickSearchFileModel> files,
        bool isShiftDown)
    {
        if (files == null)
            throw new ArgumentNullException(nameof(files));
        if (files.Where(x => x.Selected).Any())
            throw new ArgumentOutOfRangeException(nameof(files));

        bool selected = false;
        int start, end, jump;
        if (!isShiftDown)
        {
            start = _selectedIndex + 1;
            end = files.Count;
            jump = 1;
        }
        else
        {
            start = _selectedIndex - 1;
            end = -1;
            jump = -1;
        }
        for (int i = start; i != end; i = i + jump)
        {
            var file = files[i];
            if (file.Found)
            {
                _selectedIndex = i;
                file.Selected = true;
                selected = true;
                break;
            }
        }

        if (!selected)
        {
            // in case 'Shift' not down:
            // "cycle from last to first"
            // reset, so and start again from first
            // Done in 2 'half' loops, in sake of effiency.
            if (!isShiftDown)
            {
                start = 0;
                end = _selectedIndex;
                jump = 1;
            }
            else
            {
                start = files.Count - 1;
                end = _selectedIndex;
                jump = -1;
            }
            for (int i = start; i != end; i = i + jump)
            {
                var file = files[i];
                if (file.Found)
                {
                    _selectedIndex = i;
                    file.Selected = true;
                    break;
                }
            }
        }
    }

    private bool IncludeInSearchResults(QuickSearchFileModel file)
    {
        switch (_cachedSettingsValue.SelectedMode)
        {
            case QuickSearchMode.Letter:
               return _searchLetter == Char.ToLower(file.Name[0]);
            case QuickSearchMode.Word:
               return _searchWord.StartsWith(file.Name, StringComparison.OrdinalIgnoreCase);
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

        if (files == null)
            throw new ArgumentNullException(nameof(files));

        ClearSearch(files);

        handled = true;
    }

    private void ClearSearch(List<QuickSearchFileModel> files)
    {
        if (files == null)
            throw new ArgumentNullException(nameof(files));

        _searchWord = string.Empty;
        _searchLetter = Char.MinValue;
        _selectedIndex = -1;

        // Mark all as found, to clear filter
        foreach (var file in files)
        {
            file.Found = true;
            file.Selected = false;
        }
    }

    public void SaveQuickSearchSettings(QuickSearchModel quickSearchModel)
    {
        if (quickSearchModel == null)
            throw new ArgumentNullException(nameof(quickSearchModel));

        using var uow = _unitOfWorkFactory.Create();
        var repository = uow.GetRepository<QuickSearchModel>();
        repository.Upsert(SettingsId, quickSearchModel);
        _cachedSettingsValue = quickSearchModel;
    }
}
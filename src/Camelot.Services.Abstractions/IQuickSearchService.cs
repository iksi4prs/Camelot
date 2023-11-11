using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.Enums.Input;
using System.Collections.Generic;

namespace Camelot.Services.Abstractions;

public interface IQuickSearchService
{
    QuickSearchModel GetQuickSearchSettings();

    void SaveQuickSearchSettings(QuickSearchModel quickSearchModel);

    /// <summary>
    /// </summary>
    /// <param name="c">
    /// This arg is is of type 'char' and not 'Key', since translation from Key to char
    //  is platform dependent, and should be done in caller level.</param>
    /// <param name="isShiftDown"></param>
    /// <param name="files"></param>
    /// <param name="handled"></param>
    void OnCharDown(char c, bool isShiftDown, List<QuickSearchFileModel> files, out bool handled);

    void ClearSearch();

    // WIP555 - next is needed ???
    void OnEscapeKeyDown(List<QuickSearchFileModel> files, out bool handled);

    bool Enabled();
}
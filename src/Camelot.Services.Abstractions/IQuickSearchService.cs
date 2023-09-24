using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.Enums.Input;
using System.Collections.Generic;

namespace Camelot.Services.Abstractions;

public interface IQuickSearchService
{
    QuickSearchModel GetQuickSearchSettings();

    void SaveQuickSearchSettings(QuickSearchModel quickSearchModel);

    // arg is Char and not Key, since translation from Key to Char
    // is platform dependent, and should be done in caller level.
    //void OnKeyDown(Key key, KeyModifiers modifiers, out bool handled);
    void OnCharDown(char c, List<QuickSearchFileModel> files, out bool handled);
    void OnEscapeKeyDown(List<QuickSearchFileModel> files, out bool handled);

    bool Enabled();
}
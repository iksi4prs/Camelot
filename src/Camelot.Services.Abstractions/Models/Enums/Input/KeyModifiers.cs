using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camelot.Services.Abstractions.Models.Enums.Input;

// WIP444 = can remove this ???

// Needed to be duplicated here, since nor Avalonia, nor Windows.Forms, etc
// are referenced in "Camelot.Services.Abstractions"

[Flags]
public enum KeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Meta = 8,
}

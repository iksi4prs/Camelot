using System.Drawing;

namespace Camelot.Services.Abstractions;

public interface IShellIconsCacheService
{
    object GetIcon(string filename);
}
using System.Drawing;

namespace Camelot.Services.Abstractions;

public interface IShellLinksService
{
    bool IsShellLink(string path);
    string ResolveLink(string path);
}
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Camelot.Services.Abstractions;

public interface ISystemIconsService
{
    Task<Bitmap> GetFileIcon(string filename);
}
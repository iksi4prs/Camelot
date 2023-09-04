using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Camelot.Services.Abstractions;

namespace Camelot.Services.Windows;

public class WindowsSystemIconsService : ISystemIconsService
{

    public WindowsSystemIconsService()
    {
    }

    public Task<Bitmap> GetFileIcon(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentNullException(nameof(filename));
        if (!File.Exists(filename))
            throw new FileNotFoundException(filename);

        var task = Task<Bitmap>.Factory.StartNew(() =>
        {
            // TODO WIP - take correct icon
            var x = new Bitmap("C:/MyProjects/FilesCommander/test333.bmp");
            return x;
        });
        return task; 
    }
}
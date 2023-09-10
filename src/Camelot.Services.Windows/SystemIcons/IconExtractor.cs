using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Runtime.Versioning;

namespace Camelot.Services.Windows.SystemIcons;

[SupportedOSPlatform("windows")]
internal class IconExtractor
{
    public static Icon ExtractIcon(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentNullException(nameof(filename));

        Icon result;

        // see if path contains seperator "?" for index
        // namely the icon is a numbered/index in a file with more icons,
        // or the file contains just one icon.

        // TODO WIP333 - make use of PathParseIconLocationW ?
        // not good here
        // see https://learn.microsoft.com/en-us/windows/win32/api/shlwapi/nf-shlwapi-pathparseiconlocationw

        int questionMark = filename.IndexOf('?');
        if (questionMark >= 0)
        {
            string iconPath = filename.Substring(0, questionMark);
            string strIndex = filename.Substring(questionMark + 1);
            int index = Int32.Parse(strIndex);
            result = ExtractIconImpl(iconPath, index, true);
        }
        else
        {
            result = ExtractIconImpl(filename);
        }
        return result;
    }
    private static Icon ExtractIconImpl (string filename)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException(nameof(filename));
        if (!File.Exists(filename))
            throw new ArgumentOutOfRangeException(nameof(filename));

        return Icon.ExtractAssociatedIcon(filename);
    }

    // needed for indexed icons (when more than one in file, and there's a number/index for the icon we need)
    // src: https://stackoverflow.com/questions/6872957/how-can-i-use-the-images-within-shell32-dll-in-my-c-sharp-project
    private static Icon ExtractIconImpl(string filename, int number, bool largeIcon)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException(nameof(filename));
        if (!File.Exists(filename))
            throw new ArgumentOutOfRangeException(nameof(filename));

        try
        {
            IntPtr large;
            IntPtr small;
            Icon result;
            ExtractIconEx(filename, number, out large, out small, 1);

            // no need to destory after "FromHandle", but need to free the non-used one
            // https://stackoverflow.com/questions/30979653/icon-fromhandle-should-i-dispose-it-or-call-destroyicon
            if (largeIcon)
            {
                result = Icon.FromHandle(large);
                DestroyIcon(small);
            }
            else
            {
                result = Icon.FromHandle(small);
                DestroyIcon(large);
            }
            return result;
        }
        catch(Exception)
        {
            return null;
        }
    }
    [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool DestroyIcon(IntPtr hIcon);
}

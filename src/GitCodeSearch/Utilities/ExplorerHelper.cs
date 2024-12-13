using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace GitCodeSearch.Utilities;

public static class ExplorerHelper
{
    [DllImport("shell32.dll", SetLastError = true)]
    public static extern int SHOpenFolderAndSelectItems(nint pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] nint[] apidl, uint dwFlags);

    [DllImport("shell32.dll", SetLastError = true)]
    public static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, nint bindingContext, [Out] out nint pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

    public static void OpenFolderAndSelectItem(string folderPath, string file)
    {
        SHParseDisplayName(folderPath, nint.Zero, out nint nativeFolder, 0, out _);

        if (nativeFolder == nint.Zero)
        {
            // Log error, can't find folder
            return;
        }

        SHParseDisplayName(Path.Combine(folderPath, file), nint.Zero, out nint nativeFile, 0, out _);

        nint[] fileArray;
        if (nativeFile == nint.Zero)
        {
            // Open the folder without the file selected if we can't find the file
            fileArray = [];
        }
        else
        {
            fileArray = [nativeFile];
        }

        SHOpenFolderAndSelectItems(nativeFolder, (uint)fileArray.Length, fileArray, 0);

        Marshal.FreeCoTaskMem(nativeFolder);
        if (nativeFile != nint.Zero)
        {
            Marshal.FreeCoTaskMem(nativeFile);
        }
    }

    public static void OpenFileInDefaultProgram(string filePath)
    {
        try
        {
            using Process explorerProcess = new();
            explorerProcess.StartInfo.FileName = "explorer";
            explorerProcess.StartInfo.Arguments = $"\"{filePath.Replace('/', '\\')}\"";
            explorerProcess.Start();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public static void OpenUrlInDefaultBrowser(string url)
    {
        try
        {
            using var _ = Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}

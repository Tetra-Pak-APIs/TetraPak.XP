using System.IO;
using Microsoft.Extensions.FileProviders;

namespace TetraPak.XP.FileManagement
{
    public interface IXpFileInfo : IFileInfo
    {
        string Extension { get; }

        Outcome CopyTo(DirectoryInfo targetFolder);

        Outcome MoveTo(DirectoryInfo targetFolder);
    }
}
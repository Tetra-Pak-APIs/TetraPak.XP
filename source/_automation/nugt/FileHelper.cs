using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using nugt.policies;
using TetraPak.XP;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace nugt
{
    public static class FileHelper
    {
        public static FileInfo[] GetAllFiles(
            this DirectoryInfo root, 
            string? searchPattern = null, 
            Func<FileInfo, GetFilesPolicy>? criteria = null)
        {
            if (!root.Exists)
                throw new DirectoryNotFoundException($"Directory not found: {root}");

            var list = new List<FileInfo>();
            traverse(root);
            return list.ToArray();

            void traverse(DirectoryInfo dir)
            {
                var files = string.IsNullOrEmpty(searchPattern) 
                    ? dir.GetFiles().ToArray() 
                    : dir.GetFiles(searchPattern).ToArray();

                
                if (criteria is null)
                {
                    list.AddRange(files);
                }
                else
                {
                    var isBreaking = false;
                    for (var i = 0; i < files.Length && !isBreaking; i++)
                    {
                        var file = files[i];
                        switch (criteria(file))
                        {
                            case GetFilesPolicy.Skip:
                                break;

                            case GetFilesPolicy.Get:
                                list.Add(file);
                                break;

                            case GetFilesPolicy.Break:
                                isBreaking = true;
                                break;

                            case GetFilesPolicy.GetAndBreak:
                                list.Add(file);
                                isBreaking = true;
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                var folders = dir.GetDirectories();
                foreach (var folder in folders)
                {
                    traverse(folder);
                }
            }
        }

        [Flags]
        public enum GetFilesPolicy
        {
            Skip = 0,
            
            Get = 1,
            
            Break = 8,
            
            SkipAndContinue = Skip,
            
            SkipAndBreak = Break,
            
            GetAndContinue = Get,
            
            GetAndBreak = Get | Break
        }
        
    }

    public interface IXpFileInfo : IFileInfo
    {
        string Extension { get; }

        Outcome CopyTo(DirectoryInfo targetFolder);

        Outcome MoveTo(DirectoryInfo targetFolder);
    }

    public class XpFileInfo : IXpFileInfo
    {
        readonly FileInfo _fileInfo;

        public bool IsSimulation { get; }

        public ILog? Log { get; }
        
        public Stream CreateReadStream() => _fileInfo.OpenRead();

        public DirectoryInfo? Directory => _fileInfo.Directory;
        
        public bool Exists => _fileInfo.Exists;

        public long Length => _fileInfo.Length;

        public string PhysicalPath => _fileInfo.FullName;

        public string Name => _fileInfo.Name;

        public string Extension => _fileInfo.Extension;

        public DateTimeOffset LastModified => new(_fileInfo.LastWriteTime);

        public bool IsDirectory => false;
        
        public Outcome CopyTo(DirectoryInfo targetFolder)
        {
            if (!targetFolder.Exists)
                return Outcome.Fail(new DirectoryNotFoundException($"Directory not found: {targetFolder}"));

            if (IsSimulation)
                return log(RepositionMethod.Copy, targetFolder, Outcome.Success());

            var target = Path.Combine(targetFolder.FullName, _fileInfo.Name);
            _fileInfo.CopyTo(target);
            return log(RepositionMethod.Copy, targetFolder, Outcome.Success());
        }

        public Outcome MoveTo(DirectoryInfo targetFolder)
        {
            if (!targetFolder.Exists)
                return Outcome.Fail(new DirectoryNotFoundException($"Directory not found: {targetFolder}"));
            
            if (IsSimulation)
                return log(RepositionMethod.Move, targetFolder, Outcome.Success());

            var target = Path.Combine(targetFolder.FullName, _fileInfo.Name);
            _fileInfo.MoveTo(target);
            return log(RepositionMethod.Move, targetFolder, Outcome.Success());
        }

        Outcome log(RepositionMethod repositionMethod, DirectoryInfo targetFolder, Outcome outcome)
        {
            if (!outcome)
            {
                Log.Error(outcome.Exception!);
                return outcome;
            }
            
            var method = repositionMethod switch
            {
                RepositionMethod.Copy => "copied",
                RepositionMethod.Move => "moved",
                _ => throw new ArgumentOutOfRangeException(nameof(repositionMethod))
            }; 
            Log.Information($"{_fileInfo} {method} to {targetFolder}");
            return outcome;
        }


        protected XpFileInfo(FileInfo fileInfo, ILog? log = null, bool isSimulation = false)
        {
            _fileInfo = fileInfo;
            Log = log;
            IsSimulation = isSimulation;
        }
    }
}
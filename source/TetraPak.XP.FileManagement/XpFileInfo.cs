using System;
using System.IO;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.FileManagement
{
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
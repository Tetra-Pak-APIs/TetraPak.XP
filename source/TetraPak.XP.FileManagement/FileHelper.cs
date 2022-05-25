using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TetraPak.XP.FileManagement
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
}
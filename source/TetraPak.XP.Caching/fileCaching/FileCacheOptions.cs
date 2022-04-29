using System;
using System.IO;

namespace TetraPak.XP.Caching
{
    public class FileCacheOptions
    {
        public const string DefaultFileSuffix = ".cache";
        
        public DirectoryInfo Directory { get; }

        public string FileSuffix { get; private set; }

        public bool RetainInMemory { get; }

        public static FileCacheOptions Default(IFileSystem fileSystem, string? fileSuffix = null) =>
            new(fileSystem.GetCacheDirectory(), string.IsNullOrWhiteSpace(fileSuffix) ? DefaultFileSuffix : fileSuffix);

        public FileCacheOptions WithFileSuffix(string fileSuffix)
        {
            FileSuffix = fileSuffix;
            return this;
        }

        public FileCacheOptions(DirectoryInfo directory, string? fileSuffix = null, bool retainInMemory = false)
        {
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            FileSuffix = string.IsNullOrWhiteSpace(fileSuffix) 
                ? DefaultFileSuffix 
                : fileSuffix!.EnsurePrefix('.');
            RetainInMemory = retainInMemory;
        }
    }
}
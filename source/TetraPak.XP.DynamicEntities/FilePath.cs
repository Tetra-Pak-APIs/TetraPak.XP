using System;
using System.Collections.Generic;
using System.Text;

namespace TetraPak.XP.DynamicEntities
{
    /// <summary>
    ///   A specialized <see cref="DynamicPath"/> that makes it easier to work with
    ///   Unix based and Windows file systems.
    /// </summary>
    public class FilePath : DynamicPath
    {
        public const char UnixSeparator = '/';
        public const char WindowsSeparator = '\\';
        
        /// <summary>
        ///   Gets or sets a global policy for how to automatically resolve file system separators.
        /// </summary>
        public static FileSystemSeparatorResolutionPolicy SeparatorResolutionPolicy { get; set; }

        public const string AutoResolveFileSystemSeparator = @"\/";

        /// <summary>
        ///   Overrides base method to provide automatic <see cref="DynamicPath.Separator"/> resolution.
        /// </summary>
        protected override void OnConstructStack(string stringValue, string separator)
        {
            switch (separator)
            {
                case "/":
                case @"\":
                    base.OnConstructStack(stringValue, separator);
                    break;
               
                case AutoResolveFileSystemSeparator:
                    var countUnixSeparators = 0;
                    var countWindowsSeparators = 0;
                    var ca = stringValue.ToCharArray();
                    var sb = new StringBuilder();
                    var list = new List<string>();
                    for (var i = 0; i < ca.Length; i++)
                    {
                        var c = ca[i];
                        if (c == UnixSeparator)
                        {
                            ++countUnixSeparators;
                            if (sb.Length != 0)
                            {
                                list.Add(sb.ToString());
                                sb.Clear();
                            }

                            continue;
                        }

                        if (c == WindowsSeparator)
                        {
                            ++countWindowsSeparators;
                            if (sb.Length != 0)
                            {
                                list.Add(sb.ToString());
                                sb.Clear();
                            }
                            continue;
                        }

                        sb.Append(c);
                    }

                    if (sb.Length != 0)
                    {
                        list.Add(sb.ToString());
                    }

                    switch (SeparatorResolutionPolicy)
                    {
                        case FileSystemSeparatorResolutionPolicy.Unix:
                            WithSeparator(UnixSeparator.ToString());
                            break;
                        
                        case FileSystemSeparatorResolutionPolicy.Windows:
                            WithSeparator(WindowsSeparator.ToString());
                            break;

                        case FileSystemSeparatorResolutionPolicy.Majority:
                            WithSeparator(countUnixSeparators >= countWindowsSeparators
                                ? UnixSeparator.ToString()
                                : WindowsSeparator.ToString());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    SetInternal(list);
                    break;
                
                default:
                    throw new ArgumentException($"Unsupported file system separator: '{separator}'", nameof(separator));
            }
        }

        public static implicit operator FilePath(string stringValue) => new(stringValue);

        public static implicit operator string?(FilePath filePath) => filePath.StringValue;

        public FilePath(string path, string separator = AutoResolveFileSystemSeparator) 
        : base(path, separator)
        {
        }
    }
}
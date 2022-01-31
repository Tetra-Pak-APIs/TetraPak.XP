// using System;
// using System.IO;
// using System.Linq;
// using System.Threading.Tasks;
// using TetraPak.XP.Caching;
//
// namespace TetraPak.XP.Xamarin.Caching
// {
// public class FileCache<T> // : MemoryCache<T>
//     {
//         readonly TaskCompletionSource<bool> _loadingTcs;
//         readonly DirectoryInfo _directory;
//         readonly string _fileSuffix;
//
//         public override async Task<bool> ContainsAsync(string key = null)
//         {
//             await ifPreloading();
//             return await base.ContainsAsync(key);
//         }
//
//         async Task ifPreloading()
//         {
//             if (_loadingTcs.Task.IsActive())
//             {
//                 await _loadingTcs.Task;
//             }
//         }
//
//         public override async Task<Outcome<T>> TryGetAsync(string? key = null)
//         {
//             var isLoaded = await base.TryGetAsync(key);
//             if (isLoaded)
//                 return isLoaded;
//
//             var path = Path.Combine(_directory.FullName, $"{key}{_fileSuffix}");
//             if (!File.Exists(path))
//                 return Outcome<T>.Fail(new FileNotFoundException($"File does not exist: {path}"));
//             
//             var json = File.ReadAllText(path);
//             var cachedItem = json.FromJson<T>();
//             await base.AddAsync(cachedItem.Path, cachedItem.Value, true, cachedItem.ExpiresUtc);
//             return Outcome<T>.Success(cachedItem.Value);
//         }
//         
//         public override async Task AddAsync(string key, T item, bool replace = false, DateTime? expires = null)
//         {
//             await ifPreloading();
//             validateKey(key);
//             var path = toPath(key);
//             var cachedItem = new CachedItem<object>(key, item, expires ?? DateTime.MaxValue);
//             var json = cachedItem.ToJson();
//             File.WriteAllText(path, json);
//             await base.AddAsync(key, item, replace, expires);
//         }
//
//         void validateKey(string key)
//         {
//             if (key.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
//                 throw new ArgumentException($"Invalid key: {key}. Only letters, digits and '_' are accepted");
//         }
//
//         public override async Task RemoveAsync(string key = null)
//         {
//             await ifPreloading();
//             throw new NotImplementedException();
//         }
//
//         protected override Task OnRemoveAsync(CachedItem<T> item)
//         {
//             var path = toPath(item.Path);
//             File.Delete(path);
//             return base.OnRemoveAsync(item);
//         }
//
//         string toPath(string key) => Path.Combine(_directory.FullName, $"{key}{_fileSuffix}");
//
//         public FileCache(DirectoryInfo directory, string fileSuffix, bool preload)
//         {
//             _directory = directory ?? throw new ArgumentNullException(nameof(directory));
//             _fileSuffix = fileSuffix ?? throw new ArgumentNullException(nameof(fileSuffix));
//             _loadingTcs = new TaskCompletionSource<bool>();
//             if (preload)
//             {
//                 loadFromFileSystemAsync();
//                 return;
//             }
//             _loadingTcs.SetCanceled();
//         }
//
//         void loadFromFileSystemAsync()
//         {
//             Task.Run(async () =>
//             {
//                 try
//                 {
//                     var files = _directory.GetFiles($"*{_fileSuffix}");
//                     foreach (var fileInfo in files)
//                     {
//                         var json = File.ReadAllText(fileInfo.FullName);
//                         var item = json.FromJson<T>();
//                         await AddAsync(item.Path, item.Value, false, item.ExpiresUtc);
//                     }
//                     _loadingTcs.SetResult(true);
//                 }
//                 catch (Exception ex) 
//                 {
//                     _loadingTcs.SetException(ex);
//                 }
//             });
//          }
//     }
// }
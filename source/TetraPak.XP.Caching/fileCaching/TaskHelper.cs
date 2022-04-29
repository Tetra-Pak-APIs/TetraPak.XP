using System.Threading.Tasks;

namespace TetraPak.XP.Caching.TetraPak.XP.Caching
{
    /// <summary>
    ///   This implementation of the <see cref="ICache{T}"/> interface relies on files saved within
    ///   a specified folder of the file system.
    /// </summary>
    
    
    static class TaskHelper
    {
        public static bool IsActive(this Task self)
        {
            return self.Status < TaskStatus.RanToCompletion;
        }
    }

}
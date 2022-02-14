using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Streaming
{
    public static class StreamHelper
    {
        const string StreamLengthRepo = "_length_";
        static readonly ITimeLimitedRepositories s_cache;

        /// <summary>
        ///   Examines a <see cref="Stream"/> and attempts to resolve whether it is empty (zero length).
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="Stream"/> to be examined.
        /// </param>
        /// <param name="force">
        ///   Specifies whether to go ahead and read the stream to obtain the requested value when
        ///   the stream does not support the <see cref="Stream.Length"/> property
        ///   (<see cref="Stream.CanSeek"/> = <c>false</c>).
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Enables cancellation of the operation.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="bool"/> or, on failure, an <see cref="Exception"/>.
        /// </returns>
        /// <seealso cref="GetLengthAsync"/>
        public static async Task<Outcome<bool>> IsEmptyAsync(
            this Stream? stream, 
            bool force = false, 
            CancellationToken? cancellationToken = null)
        {
            if (stream is null)
                return Outcome<bool>.Success(false);
            
            var lengthOutcome = await stream.GetLengthAsync(force, cancellationToken);
            return lengthOutcome 
                ? Outcome<bool>.Success(lengthOutcome.Value == 0) 
                : Outcome<bool>.Fail(lengthOutcome.Exception!);
        }
        
        /// <summary>
        ///   Examines a <see cref="Stream"/> and attempts to resolve its length.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="Stream"/> to be examined.
        /// </param>
        /// <param name="force">
        ///   Specifies whether to go ahead and read the stream to obtain the requested value when
        ///   the stream does not support the <see cref="Stream.Length"/> property
        ///   (<see cref="Stream.CanSeek"/> = <c>false</c>).
        /// </param>
        /// <param name="cancellationToken">
        ///   (optional)<br/>
        ///   Enables cancellation of the operation.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate success/failure and, on success, also carry
        ///   a <see cref="long"/> (the requested length) or, on failure, an <see cref="Exception"/>.
        /// </returns>
        /// <seealso cref="IsEmptyAsync"/>
        public static async Task<Outcome<long>> GetLengthAsync(
            this Stream? stream, 
            bool force = false, 
            CancellationToken? cancellationToken = null)
        {
            if (stream is null)
                return Outcome<long>.Success(0);
            
            var cachedOutcome = await tryGetCachedLength(stream);
            if (cachedOutcome)
                return cachedOutcome;
            
            long originalPosition = 0;
            if (stream.CanSeek)
            {
                try
                {
                    originalPosition = stream.Position;
                    stream.Position = 0;
                    return await cacheLengthAsync(stream, stream.Length);
                }
                catch
                {
                    // ignored
                }
            }
            
            if (!force)
                return Outcome<long>.Fail(new Exception("Could not read stream length"), -1);
            
            // stream does not support the 'Length' property so we're obtaining length from reading the stream ...
            var totalBytesRead = 0;
            try
            {
                const int BufferSize = 4096;
                var readBuffer = new byte[BufferSize];
                int bytesRead;
                var ct = cancellationToken ?? CancellationToken.None;
                while ((bytesRead = await stream.ReadAsync(readBuffer, totalBytesRead, BufferSize, ct)) > 0)
                {
                    totalBytesRead += bytesRead;
                }
                return ct.IsCancellationRequested 
                    ? Outcome<long>.Fail(new Exception("Reading stream was cancelled"), -1) 
                    : await cacheLengthAsync(stream, totalBytesRead);
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }

        }

        static Task<Outcome<long>> tryGetCachedLength(Stream stream)
        {
            var key = getCachedStreamKey(stream);
            return s_cache.ReadAsync<long>(StreamLengthRepo, key);
        }

        static string getCachedStreamKey(Stream stream) => stream.GetHashCode().ToString();

        static async Task<Outcome<long>> cacheLengthAsync(Stream stream, long length)
        {
            var key = getCachedStreamKey(stream);
            await s_cache.CreateOrUpdateAsync(length, StreamLengthRepo, key);
            return Outcome<long>.Success(length);
        }

        static StreamHelper()
        {
            s_cache = new SimpleCache(null)
            {
                DefaultLifeSpan = TimeSpan.FromSeconds(30)
            };
        }
    }
}
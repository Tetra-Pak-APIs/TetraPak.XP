using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Caching
{
    /// <summary>
    ///   Used internally to enumerate a thread-static collection of <see cref="IITimeLimitedRepositoriesDelegate"/>s,
    ///   that can be cut short when a delegate opts to call a successive delegate.
    /// </summary>
    sealed class DelegatesCollection : IEnumerable<IITimeLimitedRepositoriesDelegate>
    {
        readonly IITimeLimitedRepositoriesDelegate[] _delegates;
        bool _isDone;
        Enumerator? _enumerator;

        public static DelegatesCollection Empty => new(Array.Empty<IITimeLimitedRepositoriesDelegate>());

        public void End() => _isDone = true;

        public DelegatesCollection(IEnumerable<IITimeLimitedRepositoriesDelegate> delegates) 
            => _delegates = delegates.ToArray();

        public IEnumerator<IITimeLimitedRepositoriesDelegate> GetEnumerator()
        {
            _enumerator ??= new Enumerator(this);
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        internal bool SkipTo(Func<IITimeLimitedRepositoriesDelegate,bool>? filter) => _enumerator!.SkipTo(filter);


        class Enumerator : IEnumerator<IITimeLimitedRepositoriesDelegate>
        {
            readonly DelegatesCollection _collection;
            int _index = -1;

            public bool MoveNext()
            {
                if (_collection._isDone || _index == _collection._delegates.Length)
                    return false;

                ++_index;
                return true;
            }

            public void Reset() => _index = -1;

            public IITimeLimitedRepositoriesDelegate Current => _collection._delegates[_index];

            object IEnumerator.Current => Current;

            public Enumerator(DelegatesCollection collection) => _collection = collection;

            public void Dispose()
            {
            }

            public bool SkipTo(Func<IITimeLimitedRepositoriesDelegate,bool>? filter)
            {
                if (filter is null)
                    return true;
                
                for (var i = _index+1; i < _collection._delegates.Length; i++)
                {
                    var @delegate = _collection._delegates[i];
                    if (!filter(@delegate)) 
                        continue;
                    
                    _index = i;
                    return true;
                }

                return false;
            }
        }
    }
}
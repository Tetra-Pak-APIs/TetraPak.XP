using System;
using System.Collections;
using System.Collections.Generic;

namespace TetraPak.XP
{
    public sealed class ArrayEnumerator<T> : IEnumerator<T>
    {
        int _index;
        readonly T[]? _items;
        
        public bool MoveNext()
        {
            if (_items is null || _items.Length == 0)
                return false;
                
            if (_index == _items!.Length-1)
                return false;

            ++_index;
            return true;
        }

        bool IsInsideRange => _index >= 0 && _index < _items?.Length;

        public void Reset()
        {
            _index = -1;
        }

        public T Current => (_items?.Length == 0 ? default : IsInsideRange ? _items![_index] : default) ?? throw new InvalidOperationException();

        object IEnumerator.Current => Current!;

        public ArrayEnumerator(T[] items)
        {
            _items = items;
            Reset();
        }

        public void Dispose()
        {
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace HexLibrary
{
    public class CircularQueue<T> : IEnumerable<T>
    {
        private readonly T[] _elements;
        private readonly int _mask = 0;
        private int _startIndex = 0;

        public int Length { get; private set; }

        public int Capacity => _elements.Length;

        public CircularQueue(int sizeLog)
        {
            _elements = new T[1 << sizeLog];
            _mask = _elements.Length - 1;
        }

        public void Queue(T val)
        {
            if (Length == Capacity)
            {
                throw new IndexOutOfRangeException("Index out of range in CircularQueue");
            }
            var insertIndex = (_startIndex + Length) & _mask;
            _elements[insertIndex] = val;
            Length++;
        }

        public T Dequeue()
        {
            var ret = _elements[_startIndex];
            _startIndex = (_startIndex + 1) & _mask;
            Length--;
            return ret;
        }

	    public void Clear()
	    {
		    Length = 0;
	    }

        public T this[int index]
        {
            get
            {
                if (index >= Length)
                {
                    throw new IndexOutOfRangeException("Index out of range in CircularQueue");
                }

                return _elements[(_startIndex + index) & _mask];
            }
            set
            {
                if (index >= Length)
                {
                    throw new IndexOutOfRangeException("Index out of range in CircularQueue");
                }

                _elements[(_startIndex + index) & _mask] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }
    }
}

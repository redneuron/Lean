using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Lean.Engine.DataFeeds.Enumerators
{
    /// <summary>
    /// Provides an implementation of <see cref="IEnumerator{T}"/> that will
    /// always return true via MoveNext.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RefreshEnumerator<T> : IEnumerator<T>
    {
        private T _current;
        private IEnumerator<T> _enumerator;
        private readonly Func<IEnumerator<T>> _enumeratorFactory;

        public RefreshEnumerator(Func<IEnumerator<T>> enumeratorFactory)
        {
            _enumeratorFactory = enumeratorFactory;
        }

        public bool MoveNext()
        {
            if (_enumerator == null)
            {
                _enumerator = _enumeratorFactory.Invoke();
            }

            var moveNext = _enumerator.MoveNext();
            if (moveNext)
            {
                _current = _enumerator.Current;
            }
            else
            {
                _enumerator = null;
                _current = default(T);
            }

            return true;
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public T Current
        {
            get { return _current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}

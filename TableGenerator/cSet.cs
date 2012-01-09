using System;
using System.Collections.Generic;
using System.Text;

namespace TableGenerator
{
    public class cSet<T>:HashSet<T>, IEnumerable<T>, ICloneable
    {
		public bool AddRange(IEnumerable<T> a_items)
		{
			int _countBefore = base.Count;
			base.UnionWith(a_items);
			return _countBefore!=base.Count;
		}

        public T[] ToArray()
        {
            T[] _retArr = new T[base.Count];
			base.CopyTo(_retArr);
            return _retArr;
        }

        public bool ContainsAny(IEnumerable<T> a_items)
        {
            return base.Overlaps(a_items);
        }

		public object Clone()
		{
			cSet<T> _retSet = new cSet<T>();
			_retSet.UnionWith(this);
			return _retSet;
		}
	}
}

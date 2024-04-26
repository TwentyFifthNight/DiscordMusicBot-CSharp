using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satescuro
{
    public class Shuffler<T> : IComparer<T>
    {
        public Shuffler()
        {
        }

        public int Compare(T? x, T? y)
        {
            var val1 = Random.Shared.Next();
            var val2 = Random.Shared.Next();

            if (val1 > val2)
                return 1;
            if (val1 < val2)
                return -1;
            return 0;
        }
    }
}

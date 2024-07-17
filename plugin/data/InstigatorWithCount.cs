using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatMe.plugin.data
{
    internal class InstigatorWithCount
    {
        public EmoteInstigatorCounter.InstigatorData data = null!;
        public uint count;
        public InstigatorWithCount(EmoteInstigatorCounter.InstigatorData instigator, uint thisCount)
        {
            data = instigator; count = thisCount;

        }
    }
}

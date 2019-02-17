using System;
using System.Collections.Generic;
using System.Text;

namespace PGN
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SynchronizableAttribute : Attribute
    {
        public SynchronizableAttribute()
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schubert
{
    public struct RangeOfDateTime
    {
        private DateTime m_min;
        private DateTime m_max;

        public RangeOfDateTime(DateTime min, DateTime max)
        {
            this.m_min = min;
            this.m_max = max;
        }

        public DateTime Minimum
        {
            get { return this.m_min; }
            set { this.m_min = value; }
        }

        public DateTime Maximum
        {
            get { return this.m_max; }
            set { this.m_max = value; }
        }
    }
}

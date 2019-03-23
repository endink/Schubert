using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class SettingRecord
    {
        public SettingRecord()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public String Id { get; set; }

        public string Name { get; set; }

        public string RawValue { get; set; }
        
        public string Region { get; set; }
    }
}

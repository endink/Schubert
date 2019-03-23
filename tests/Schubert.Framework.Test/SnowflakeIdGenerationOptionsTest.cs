using Schubert.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test
{
    //public class SnowflakeIdGenerationOptionsTest
    //{

    //    [Fact]
    //    public void ComputeServerIdTest()
    //    {
    //        SnowflakeIdGenerationOptionsForTest options = new SnowflakeIdGenerationOptionsForTest("192.168.192.254");
    //        Assert.Equal(null, options.GetServerId());
            
    //        options.SetTestIp("192.168.254.1");
    //        Assert.Equal(1, options.GetServerId());

    //        options.SetTestIp("192.168.188.1");
    //        Assert.Equal(257, options.GetServerId());

    //        options.SetTestIp("192.168.190.1");
    //        Assert.Equal(769, options.GetServerId());

    //        options.SetTestIp("192.168.190.254");
    //        Assert.Equal(1022, options.GetServerId());
    //    }

    //    private class SnowflakeIdGenerationOptionsForTest : LanOptions
    //    {
    //        private string _ipAddress = null;

    //        public SnowflakeIdGenerationOptionsForTest(String ipAddress)
    //        {
    //            this.LAN1IPMask = "192.168.254.*";
    //            this.LAN2IPMask = "192.168.188.*";
    //            this.LAN3IPMask = "192.168.233.*";
    //            this.LAN4IPMask = "192.168.190.*";
    //            this._ipAddress = ipAddress;
    //        }

    //        public void SetTestIp(String ipAddress)
    //        {
    //            _ipAddress = ipAddress;
    //        }

    //        protected override string[] GetLocalHostIP()
    //        {
    //            return new String[] { _ipAddress };
    //        }
    //    }
    //}
}

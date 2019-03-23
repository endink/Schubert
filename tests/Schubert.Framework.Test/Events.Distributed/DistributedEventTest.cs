using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Schubert.Framework.DependencyInjection;

namespace Schubert.Framework.Test.Events.Distributed
{
    public class DistributedEventTest
    {
        //private IServiceProvider CreateDI()
        //{
        //    var collection = TestContextHelper.CreateFrameworkDI(
        //        builer=> 
        //        {
        //            builer.AddDistributedEventNotification();
                    
        //        });

        //    return collection.BuildServiceProvider();
        //}
        
        //[Fact(DisplayName = "DistributedEvent Server")]
        //public void Server()
        //{
        //    var sp = CreateDI();
        //    var server = sp.GetRequiredService<IDistributedEventNotificationServer>();
        //    server.Open();
        //    server.Shutdown();
        //}

        //[Fact(DisplayName = "DistributedEvent Notify")]
        //public void Communicate()
        //{
        //    var sp = CreateDI();
        //    var server = sp.GetRequiredService<IDistributedEventNotificationServer>();
        //    server.Open();

        //    var client = sp.GetRequiredService<IEventNotification>();
        //    client.Notify("CCC", this, "Hello");

        //    server.Shutdown();
        //}
    }
}

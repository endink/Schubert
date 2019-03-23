using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.DependencyInjection;
using Schubert.Framework.Environment.Modules.Finders;
using System;
using System.IO;

namespace Schubert.Framework.Test
{
//    public static class TestContextHelper
//    {
//        public const string Configuration = @"{
//  'Schubert': {
//    'SerialNumber': '20150723',
//    'AppName': '蜡笔街PC网站',
//    'Version': '1.0.0',
//    'DefaultCulture': 'zh-Hans',
//    'DefaultTimeZone': 'China Standard Time',

//    'Data': {
//      'SequenceSupported': false,
//      'DbConnectionString': 'Server=(localdb)\\mssqllocaldb;Database=SchubertUnitTest;Trusted_Connection=True;MultipleActiveResultSets=true'
//    },

//    'Azure': {
//      'UseDevelopmentAccount': true
//    }
//  }
//}";

//        public static ServiceCollection CreateFrameworkDI(Action<SchubertServicesBuilder> building = null)
//        {
//            ConfigurationBuilder builder = new ConfigurationBuilder();
//            builder.AddJsonContent(Configuration);
//            var c = builder.Build();

//            ServiceCollection collection = new ServiceCollection();
//            collection.AddSingleton<IModuleFinder>(new ModuleFinderMock());
             
//            collection.AddSchubertFramework(c, building);

//            return collection;
//        }
//    }
}

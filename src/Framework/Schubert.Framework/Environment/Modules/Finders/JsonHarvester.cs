using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Schubert.Framework.Environment.Modules.Finders
{
    /// <summary>
    /// 实现在指定位置加载 Json 格式的模块清单。
    /// </summary>
    public class JsonHarvester : FileContentHarvester
    {
        public const string SchemaResourceName = "module-schema.json";
        //private JSchema _schema = null;
        //private static readonly object SchemaReadLock = new object();

        public JsonHarvester(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        { }

        protected override string ManifestFileName
        {
            get { return "module.json"; }
        }


        protected override bool ReadFromFileContent(string fileContent, out ModuleDescriptor descriptor)
        {
            // JSchema.Parse 方法会发生调用错误，可能由于引用版本问题。暂时无法解决，先不验证架构。
            //JObject jsonModule = ReadAndValidateManifest(schema, file);
            
            JObject jsonModule = JObject.Parse(fileContent);

            descriptor = new ModuleDescriptor();

            ParseModuleProperties(jsonModule, descriptor);
            ParseModuleFeatures(jsonModule, descriptor);

            return true;
        }
        



        //private JSchema GetOrReadSchema()
        //{
        //    if (_schema == null)
        //    {
        //        lock(SchemaReadLock)
        //        {
        //            if (_schema == null)
        //            {
        //                using (Stream resourceStream = typeof(JsonHarvester).GetTypeInfo().Assembly
        //                .GetManifestResourceStream(SchemaResourceName))
        //                {
        //                    string json = resourceStream.ReadToEnd(Encoding.UTF8);
        //                    _schema = JSchema.Parse(json);
        //                }
        //            }
        //        }
        //    }

        //    return _schema;
        //}

        private void ParseModuleFeatures(JObject jsonModule, ModuleDescriptor descriptor)
        {
            IEnumerable<JObject> featuresJson = jsonModule.GetValue("features")?.Values<JObject>() ?? Enumerable.Empty<JObject>();
            List<FeatureDescriptor> features = new List<FeatureDescriptor>();
            foreach (var f in featuresJson)
            {
                FeatureDescriptor feature = new FeatureDescriptor();
                feature.Name = jsonModule.GetValue("name").Value<String>();
                feature.Description = jsonModule.GetValue("description").Value<String>();
                feature.Category = jsonModule.GetValue("category")?.Value<String>().IfNullOrWhiteSpace(null);
                feature.Dependencies = jsonModule.GetValue("dependencies")?.Values<String>()?.ToArray() ?? Enumerable.Empty<String>();
                feature.ModuleName = descriptor.Name;
            }

            descriptor.Features = features.AsReadOnly();
        }

        private void ParseModuleProperties(JObject jsonModule, ModuleDescriptor descriptor)
        {
            descriptor.Name = jsonModule.GetValue("name").Value<String>();

            if (descriptor.Name.IsNullOrWhiteSpace())
            {
                throw new SchubertException($"读取 Json 模块配置文件出错，必须包含属性 name。");
            }

            descriptor.RootNamespce = jsonModule.GetValue("rootNamespace")?.Value<String>();
            descriptor.Author = jsonModule.GetValue("author").Value<String>();
            descriptor.Description = jsonModule.GetValue("description").Value<String>();
            descriptor.Category = jsonModule.GetValue("category")?.Value<String>().IfNullOrWhiteSpace(null);
            descriptor.IncludeUserInterface = jsonModule.GetValue("includeUserInterface")?.Value<Boolean?>() ?? true;
            descriptor.Version = jsonModule.GetValue("version")?.Value<String>();
            descriptor.SupportVersions = jsonModule.GetValue("supportVersions")?.Values<String>()?.ToArray() ?? Enumerable.Empty<String>();
            descriptor.Dependencies = jsonModule.GetValue("dependencies")?.Values<String>()?.ToArray() ?? Enumerable.Empty<String>();
        }

        //private static JObject ReadAndValidateManifest(JSchema schema, string file)
        //{
        //    JObject jsonModule = JObject.Parse(File.ReadAllText(file));
        //    IList<String> errors = null;
        //    if (!jsonModule.IsValid(schema, out errors))
        //    {
        //        throw new SchubertException(
        //            $@"Json 模块清单文件 {file} 格式不正确。{Env.NewLine}{errors.ToArrayString(Env.NewLine)}");
        //    }

        //    return jsonModule;
        //}
    }
}

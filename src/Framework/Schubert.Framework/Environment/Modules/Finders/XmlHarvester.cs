using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Schubert.Framework.Environment.Modules.Finders
{
    /// <summary>
    /// 从 Xml 清单中组织模块的对象。
    /// </summary>
    public class XmlHarvester : FileContentHarvester
    {
        public XmlHarvester(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        { }

        protected override string ManifestFileName
        {
            get { return "module.xml"; }
        }


        protected override bool ReadFromFileContent(string fileContent, out ModuleDescriptor descriptor)
        {
            descriptor = null;
            XDocument doc = XDocument.Parse(fileContent);
            XElement element = doc.Element(XName.Get("manifest", @"http://www.labiji.com/ModuleXMLSchema"));
            string schemaVersion = element.Attribute(XName.Get("schemaVersion"))?.Value ?? String.Empty;
            switch (schemaVersion)
            {
                case "1.0":
                    descriptor = this.ReadV1(element);
                    return descriptor != null;
                default:
                    return false;
            }
        }


        /// <summary>
        ///  用于读取版本为 1.0 清单文件。
        /// </summary>
        /// <returns></returns>
        private ModuleDescriptor ReadV1(XElement manifestElement)
        {
            var moduleElement = manifestElement.Element(XName.Get("module"));
            
            if (moduleElement != null)
            {
                ModuleDescriptor module = new ModuleDescriptor();
                this.ParseModuleAttributes(module, moduleElement);
                this.ParseModuleFeatures(module, moduleElement);

                return module;
            }
            return null;
        }

        private void ParseModuleFeatures(ModuleDescriptor module, XElement moduleElement)
        {
            var featureElements = moduleElement.Element(XName.Get("features"))?.Elements(XName.Get("feature"));

            List<FeatureDescriptor> features = new List<FeatureDescriptor>();
            features.Add(this.BuildDefaultFeature(module)); //创建模块的默认 Feature。

            if (featureElements != null)
            {
                foreach (var f in featureElements)
                {
                    FeatureDescriptor feature = new FeatureDescriptor();
                    this.ParseFeature(f, feature);
                    feature.ModuleName = module.Name;
                    features.Add(feature);
                }
            }
            module.Features = features.ToArray();
        }

        /// <summary>
        /// 此方法为模块创建一个默认的 <see cref="FeatureDescriptor"/>。
        /// 意味着任何模块都包含至少一个 Feature，该 Feature 表示没有明确通过 <see cref="SchubertFeatureAttribute"/> 属性标记的组件。
        /// </summary>
        private FeatureDescriptor BuildDefaultFeature(ModuleDescriptor moduleDescriptor)
        {
            return new FeatureDescriptor()
            {
                Category = moduleDescriptor.Category,
                Priority = 100, //模块级的 Feature 总是拥有更高的优先级。
                Description = moduleDescriptor.Description,
                ModuleName = moduleDescriptor.Name,
                Name = moduleDescriptor.Name,
                Dependencies = moduleDescriptor.Dependencies
            };
        }

        private void ParseModuleAttributes(ModuleDescriptor module, XElement moduleElement)
        {
            module.Name = moduleElement.Attribute(XName.Get("name"))?.Value ?? String.Empty;

            if (module.Name.IsNullOrWhiteSpace())
            {
                throw new SchubertException($@"读取 XML 模块配置文件发生错误， module 元素必须制定 name 属性。");
            }
            module.RootNamespce = moduleElement.Attribute(XName.Get("Namespace"))?.Value ?? null;
            module.Author = moduleElement.Attribute(XName.Get("author"))?.Value ??  "unknown";
            module.Version = moduleElement.Attribute(XName.Get("version"))?.Value ?? "unknown";
            module.IncludeUserInterface = Boolean.Parse(moduleElement.Attribute(XName.Get("includeUserInterface"))?.Value ?? "true");
            module.Description = this.GetElementDescription(moduleElement);

            var versionElement = moduleElement.Element(XName.Get("supportVersions"));
            if (versionElement != null)
            {
                module.SupportVersions = versionElement.Elements(XName.Get("versions")).Select(e => e.Value).ToArray();
            }

             module.Dependencies = (moduleElement.Element(XName.Get("dependencies"))?.Value ?? String.Empty)
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<String>();
        }

        private void ParseFeature(XElement featureElement, FeatureDescriptor feature)
        {
            feature.Name = featureElement.Attribute(XName.Get("name")).Value ?? String.Empty;
            feature.Category = featureElement.Attribute(XName.Get("category"))?.Value ?? String.Empty;
            string priorityString = featureElement.Attribute(XName.Get("priority"))?.Value ?? String.Empty;
            int priority = 0;
            if (int.TryParse(priorityString, out priority))
            {
                feature.Priority = priority;
            }
            feature.Description = GetElementDescription(featureElement);
            feature.Dependencies = (featureElement.Element(XName.Get("dependencies"))?.Value ?? String.Empty)
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<String>();
        }

        private string GetElementDescription(XElement element)
        {
            var descElement = element.Element(XName.Get("description"));
            return descElement?.Attribute(XName.Get("resourceName"))?.Value ?? String.Empty;
        }

    }
}
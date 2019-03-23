using Schubert.Framework.Domain;
using Schubert.Framework.Environment;
using Schubert.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Schubert.Framework.Localization
{
    public class LocalizedStringManager : ILocalizedStringManager
    {
        private const string XmlNamespace = "http://www.labiji.com/LanguageXMLSchema";
        private IWorkContextAccessor _workContextAccessor;

        public LocalizedStringManager(IWorkContextAccessor workContextAccessor)
        {
            Guard.ArgumentNotNull(workContextAccessor, nameof(workContextAccessor));
            _workContextAccessor = workContextAccessor;
        }

        private ILanguageService CreateLanguageService()
        {
            return _workContextAccessor.GetContext().Resolve<ILanguageService>() ?? new NullLanguageService();
        }

        public async Task<IEnumerable<String>> GetLocalizedStringKeys(string cultureName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(cultureName, nameof(cultureName));
            var langSvc = this.CreateLanguageService();
            
            var lang = await langSvc.GetLanguageAsync(cultureName);
            return lang?.StringResources?.Select(r => r.ResourceName) ?? Enumerable.Empty<String>();
        }

        public async Task<string> ExportLanguageXmlAsync(string cultureName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(cultureName, nameof(cultureName));

            var langSvc = this.CreateLanguageService();
            Language language = await langSvc.GetLanguageAsync(cultureName);
            if (language != null)
            {
                var content = GetLanguageElementContent(language).ToArray();
                XDocument xd = new XDocument(
                    new XElement(XName.Get("language", XmlNamespace), content));
                return xd.ToString();
            }
            return String.Empty;
        }

        private IEnumerable<Object> GetLanguageElementContent(Language language)
        {
            yield return new XAttribute("culture", language.Culture);
            foreach (var r in language.StringResources)
            {
                var resourceElement = new XElement("resource",
                    new XAttribute("name", r.ResourceValue),
                    new XText(r.ResourceValue)
                    );
                if (!r.IsStatic)
                {
                    resourceElement.Add(new XAttribute("dynamic", true));
                }
                yield return resourceElement;
            }
        }

        public string GetLocalizedString(string cultureName, string key)
        {
            var langSvc = this.CreateLanguageService();
            string ls = langSvc.GetLanguageAsync(cultureName).GetAwaiter().GetResult()?
                .StringResources?.FirstOrDefault(r => r.ResourceName == key)?.ResourceValue;

            return ls.IfNullOrWhiteSpace(String.Empty);
        }

        public async Task ImportLanguageXmlAsync(string cultureName, string xml, AdditionPolicy policy)
        {
            if (!xml.IsNullOrWhiteSpace())
            {
                XDocument xd = XDocument.Load(xml);
                string culture;
                XElement root = LoadLanguageElement(xd, out culture);
                Dictionary<string, StringResource> resources = LoadLanguageResources(culture, root);

                var langSvc = this.CreateLanguageService();
                await langSvc.AddStringResourcesAsync(resources.Values, policy);
            }
        }

        private static Dictionary<string, StringResource> LoadLanguageResources(string culture, XElement root)
        {
            Dictionary<String, StringResource> resources = new Dictionary<String, StringResource>();
            foreach (var r in root.Elements(XName.Get("resource")))
            {
                string name = r.Attribute(XName.Get("name"))?.Value;
                string value = r.Value;
                string staticString = r.Attribute(XName.Get("dynamic"))?.Value ?? "false";
                bool IsDynamic = false;
                bool.TryParse(staticString, out IsDynamic);
                if (!name.IsNullOrWhiteSpace() && !value.IsNullOrWhiteSpace())
                {
                    StringResource resource = new StringResource()
                    {
                        Culture = culture,
                        ResourceName = name,
                        ResourceValue = value,
                        IsStatic = !IsDynamic
                    };
                    resources[resource.ResourceName] = resource;
                }
            }

            return resources;
        }

        private static  XElement LoadLanguageElement(XDocument xd, out string culture)
        {
            XElement root = xd.Element(XName.Get("language", XmlNamespace));
            if (root == null)
            {
                throw new SchubertException(@"XML 文档中 ""language"" 根元素节点不存在或命名空间不正确。");
            }
            var attribute = root.Attribute(XName.Get("culture"));
            culture = attribute?.Value;
            if (culture.IsNullOrWhiteSpace())
            {
                throw new SchubertException(@"XML 文档中 ""language"" 根元素节上缺少 ""culture"" 属性节点或该属性节点值为空。");
            }
            return root;
        }

    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Schubert.Framework.FileSystem.AppData;
using Schubert.Framework.Localization;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Schubert.Framework.Environment.Modules
{
    /// <summary>
    /// Single service instance registered at the host level. Provides storage
    /// and recall of shell descriptor information. Default implementation uses
    /// app_data, but configured replacements could use other per-host writable location.
    /// </summary>
    public interface IShellDescriptorCache
    {
        /// <summary>
        /// Recreate the named configuration information. Used at startup. 
        /// Returns null on cache-miss.
        /// </summary>
        ShellDescriptor Fetch(string shellName);

        /// <summary>
        /// Commit named configuration to reasonable persistent storage.
        /// This storage is scoped to the current-server and current-webapp.
        /// Loss of storage is expected.
        /// </summary>
        void Store(string shellName, ShellDescriptor descriptor);
    }

    public class NullShellDescriptorCache : IShellDescriptorCache
    {
        public ShellDescriptor Fetch(string shellName)
        {
            return null;
        }

        public void Store(string shellName, ShellDescriptor descriptor)
        {
        }
    }

    public class FileShellDescriptorCache : IShellDescriptorCache
    {
        private readonly IAppDataFolder _appDataFolder;
        private const string DescriptorCacheFileName = "shellDesc.cache";
        private static readonly object _synLock = new object();
        public FileShellDescriptorCache(IAppDataFolder appDataFolder, ILoggerFactory loggerFactory)
        {
            _appDataFolder = appDataFolder;
            Logger = loggerFactory?.CreateLogger<ShellDescriptor>() ?? (ILogger)NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public bool Disabled { get; set; }

        public ShellDescriptor Fetch(string name)
        {
            if (Disabled)
            {
                return null;
            }

            lock (_synLock)
            {
                VerifyCacheFile();
                var text = _appDataFolder.ReadFile(DescriptorCacheFileName);
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(text);
                XmlNode rootNode = xmlDocument.DocumentElement;
                if (rootNode != null)
                {
                    foreach (XmlNode tenantNode in rootNode.ChildNodes)
                    {
                        if (String.Equals(tenantNode.Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            return GetShellDecriptorForCacheText(tenantNode.InnerText);
                        }
                    }
                }

                return null;

            }

        }

        public void Store(string name, ShellDescriptor descriptor)
        {
            if (Disabled)
            {
                return;
            }

            lock (_synLock)
            {
                VerifyCacheFile();
                var text = _appDataFolder.ReadFile(DescriptorCacheFileName);
                bool tenantCacheUpdated = false;
                Schubert.Helpers.ToolHelper.WriteUtf8Xml(saveWriter =>
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(text);
                    XmlNode rootNode = xmlDocument.DocumentElement;
                    if (rootNode != null)
                    {
                        foreach (XmlNode tenantNode in rootNode.ChildNodes)
                        {
                            if (String.Equals(tenantNode.Name, name, StringComparison.OrdinalIgnoreCase))
                            {
                                tenantNode.InnerText = GetCacheTextForShellDescriptor(descriptor);
                                tenantCacheUpdated = true;
                                break;
                            }
                        }
                        if (!tenantCacheUpdated)
                        {
                            XmlElement newTenant = xmlDocument.CreateElement(name);
                            newTenant.InnerText = GetCacheTextForShellDescriptor(descriptor);
                            rootNode.AppendChild(newTenant);
                        }
                    }

                    xmlDocument.Save(saveWriter);

                    _appDataFolder.CreateFile(DescriptorCacheFileName, saveWriter.ToString());
                });
            }
        }

        private static string GetCacheTextForShellDescriptor(ShellDescriptor descriptor)
        {
            var sb = new StringBuilder();
            sb.Append(descriptor.ApplicationName + "|");
            foreach (var feature in descriptor.DisabledFeatures)
            {
                sb.Append(feature + ";");
            }
            sb.Append("|");
            foreach (var parameter in descriptor.Parameters)
            {
                sb.Append(parameter.Component + "," + parameter.Name + "," + parameter.Value);
                sb.Append(";");
            }

            return sb.ToString();
        }

        private static ShellDescriptor GetShellDecriptorForCacheText(string p)
        {
            string[] fields = p.Trim().Split(new[] { "|" }, StringSplitOptions.None);
            var shellDescriptor = new ShellDescriptor { ApplicationName = fields[0] };
            string[] features = fields[1].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            shellDescriptor.DisabledFeatures = features;
            string[] parameters = fields[2].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            shellDescriptor.Parameters = parameters.Select(parameter => 
            parameter.Split(new[] { "," }, StringSplitOptions.None)).Select(parameterFields => new ShellParameter { Component = parameterFields[0], Name = parameterFields[1], Value = parameterFields[2] }).ToList();

            return shellDescriptor;
        }

        /// <summary>
        /// Creates an empty cache file if it doesn't exist already
        /// </summary>
        private void VerifyCacheFile()
        {
            if (!_appDataFolder.FileExists(DescriptorCacheFileName))
            {
                using (var writer = new StringWriter())
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = Encoding.UTF8;
                    using (var xmlWriter = XmlWriter.Create(writer, settings))
                    {
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("Tenants");
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                    }
                    _appDataFolder.CreateFile(DescriptorCacheFileName, writer.ToString());
                }
            }
        }
    }
}
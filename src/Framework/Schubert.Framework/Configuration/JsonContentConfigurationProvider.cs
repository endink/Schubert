using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Configuration
{
    public class JsonContentConfigurationProvider : ConfigurationProvider
    {
        public JsonContentConfigurationProvider(string jsonString)
        {
            this.Content = jsonString;
        }

        /// <summary>
        /// 获取 <see cref="JsonContentConfigurationProvider"/> 原始的 Json 内容。
        /// </summary>
        public string Content { get; }
        
        public override void Load()
        {
            if (!this.Content.IsNullOrWhiteSpace())
            {
                try
                {
                    Load(this.Content);
                }
                catch (FormatException ex)
                {
                    throw new ConfigurationException("加载配置文件时发生错误。", ex);
                }
            }
        }

        internal void Load(String json)
        {
            JsonConfigurationParser parser = new JsonConfigurationParser();
            try
            {
                Data = parser.Parse(json);
            }
            catch (JsonReaderException e)
            {
                string errorLine = string.Empty;
                throw new FormatException($"json 配置文本格式错误：行号：{e.LineNumber}。", e);
            }
        }
    }

}

using Schubert.Framework.Data;
using Schubert.Framework.Environment.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Schubert.Framework.Data.Services;
using Microsoft.Extensions.Options;

namespace Schubert.Framework.Environment.Modules
{
    public class ShellDescriptorManager : IShellDescriptorManager
    {
        private IRepository<ShellDescriptorRecord> _repository = null;
        private SchubertOptions _options = null;

        public ShellDescriptorManager(
            IRepository<ShellDescriptorRecord> repository, 
            IOptions<SchubertOptions> options)
        {
            Guard.ArgumentNotNull(options, nameof(options));
            Guard.ArgumentNotNull(repository, nameof(repository));

            _repository = repository;
            _options = options.Value;
        }

        public ShellDescriptor GetShellDescriptor()
        {
            ShellDescriptorRecord record = _repository.TableNoTracking.FirstOrDefault(r=>r.AppName == _options.AppSystemName);
            if (record == null)
            {
                return null;
            }
            return new ShellDescriptor()
            {
                ApplicationName = record.AppName,
                DisabledFeatures = record.DisabledFeatures.IsNullOrEmpty() ? Enumerable.Empty<String>() : 
                JsonConvert.DeserializeObject<IEnumerable<String>>(record.DisabledFeatures),

                Parameters = record.Parameters.IsNullOrEmpty() ? Enumerable.Empty<ShellParameter>() : 
                JsonConvert.DeserializeObject<IEnumerable<ShellParameter>>(record.Parameters)
            };
        }

        public void UpdateShellDescriptor(IEnumerable<string> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            string name = _options.AppSystemName;
            
            ShellDescriptorRecord record = _repository.Table.FirstOrDefault(d => d.AppName == name);
            bool exsiting = (record != null);
            if (!exsiting)
            {
                record = new ShellDescriptorRecord();
                record.AppName = name;
            }
            record.DisabledFeatures = enabledFeatures.IsNullOrEmpty() ? null : JsonConvert.SerializeObject(enabledFeatures);
            record.Parameters = parameters.IsNullOrEmpty() ? null : JsonConvert.SerializeObject(parameters);
            if (exsiting)
            {
                _repository.Update(record);
            }
            else
            {
                _repository.Insert(record);
            }
            _repository.CommitChanges();
        }
    }

    
}

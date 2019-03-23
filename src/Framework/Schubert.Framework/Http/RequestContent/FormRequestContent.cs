using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Http
{
    public class FormRequestContent : IRequestContent
    {
        private IEnumerable<KeyValuePair<String, String>> _formValues = null;
        public FormRequestContent(object anonymousObject)
        {
            Guard.ArgumentNotNull(anonymousObject, nameof(anonymousObject));

            _formValues = anonymousObject.ToDictionary().Where(kv => kv.Value != null)
                .Select(kv => new KeyValuePair<String, String>(kv.Key, kv.Value.ToString()));
        }

        public FormRequestContent(IDictionary<String, String> formValues)
        {
            this._formValues = formValues.ToArray();
        }

        public HttpContent GetContent()
        {
            return new FormUrlEncodedContent(_formValues);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Schubert.Linq
{
    public sealed class PropertyPathBuilder<TEntity>
    {
        private Dictionary<String, List<String>> m_pathArray = null;
        private string m_currentPrefix = null;
        private string m_currentKey = null;

        private PropertyPathBuilder(string currentPrefix, string currentKey, Dictionary<string, List<String>> paths)
	    {
            this.m_currentPrefix = currentPrefix ?? String.Empty;
            this.m_currentKey = currentKey ?? String.Empty;
            this.m_pathArray = paths ?? new Dictionary<string, List<String>>();
	    }

        public PropertyPathBuilder() : this(null, null, null)
        {

        }

        private void AddPath(string key, string path)
        {
            var existingPath = this.m_pathArray.GetOrAdd(key, k=> new List<String>());
            if (!existingPath.Contains(path))
            {
                existingPath.Add(path);
            }
        }

        public PropertyPathBuilder<TItem> IncludeCollection<TItem>(Expression<Func<TEntity, IEnumerable<TItem>>> collectionPropertyExpression)
            where TItem : class
        {
            Guard.ArgumentNotNull(collectionPropertyExpression, "collectionPropertyExpression");
            string propertyName = collectionPropertyExpression.GetMemberName();
            if (this.m_currentPrefix.IsNullOrWhiteSpace())
            {
                this.AddPath(propertyName, propertyName);
                return new PropertyPathBuilder<TItem>(propertyName, propertyName, this.m_pathArray);
            }
            else
            {
                string prefix = String.Format("{0}.{1}", this.m_currentPrefix, propertyName);
                this.AddPath(this.m_currentKey, prefix);
                return new PropertyPathBuilder<TItem>(prefix, this.m_currentKey, this.m_pathArray); 
            }
            
        }

        public PropertyPathBuilder<TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            where TProperty : class
        {
            Guard.ArgumentNotNull(propertyExpression, "propertyExpression");
           
            string propertyName = propertyExpression.GetMemberName();
            if (this.m_currentPrefix.IsNullOrWhiteSpace())
            {
                this.AddPath(propertyName, propertyName);
                return new PropertyPathBuilder<TProperty>(propertyName, propertyName, this.m_pathArray);
            }
            else
            {
                string prefix = String.Format("{0}.{1}", this.m_currentPrefix, propertyName);
                this.AddPath(this.m_currentKey, prefix);
                return new PropertyPathBuilder<TProperty>(prefix, this.m_currentKey, this.m_pathArray);
            }
        }

        public string[] GetPaths()
        {
            List<String> paths = new List<string>();
            foreach (var p in this.m_pathArray)
            {
                foreach (string child in p.Value)
                {
                    if (!child.IsNullOrWhiteSpace())
                    {
                        if (!paths.Contains(child))
                        {
                            paths.Add(child);
                        }
                    }
                }
            }
            var orderdPathList = paths.OrderByDescending(p => p).ToArray();
            paths.Clear();
            string current = String.Empty;
            foreach (var p in orderdPathList)
            {
                if (current.IndexOf(p) == -1)
                {
                    paths.Add(p);
                    current = p;
                }
            }
            return paths.ToArray();
        }
    }
}

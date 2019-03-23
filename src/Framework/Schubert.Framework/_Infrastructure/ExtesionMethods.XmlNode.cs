/*
===============================================================================
 The comments is generate by a tool.

 Author:Sharping      CreateTime:2011-01-27 09:31 
===============================================================================
 Copyright ? Sharping.  All rights reserved.
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
 OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
 LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 FITNESS FOR A PARTICULAR PURPOSE.
===============================================================================
*/
using System.Xml;

namespace System
{
    static partial class ExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">node 不是元素节点或属性节点</exception>
        public static string GetXPath(this XmlNode node, string separator = "/")
        {
            if(node.NodeType != XmlNodeType.Element && node.NodeType != XmlNodeType.Attribute)
            {
                throw new ArgumentException("the type of node is not correct.");
            }
            string nodeName = node.Name;
            if (node.NodeType == XmlNodeType.Attribute)
            {
                nodeName = String.Format("@{0}", nodeName);
            }
            while (node.ParentNode != null)
            {
                nodeName = String.Format("{0}{1}{0}{2}",separator.IfNullOrEmpty("/"), node.Name, node.ParentNode.Name);
                node = node.ParentNode;
            }
            return nodeName;
        }

        public static string GetAttribute(this XmlNode node, string attributeName)
        {
            return GetAttribute(node, attributeName, null);
        }

        public static string GetAttribute(this XmlNode node, string attributeName, string defaultValue)
        {
            var attribute = node.Attributes[attributeName];
            return (attribute != null ? attribute.InnerText : defaultValue);
        }

        public static void SetAttribute(this XmlNode node, string name, string value)
        {
            if (node != null)
            {
                var attribute = node.Attributes[name, node.NamespaceURI];
                if (attribute == null)
                {
                    attribute = node.OwnerDocument.CreateAttribute(name, node.OwnerDocument.NamespaceURI);
                    node.Attributes.Append(attribute);
                }
                attribute.InnerText = value;
            }
        }
    }
}

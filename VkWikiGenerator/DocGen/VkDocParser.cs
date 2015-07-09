﻿namespace VkWikiGenerator.DocGen
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using JetBrains.Annotations;

    /// <summary>
    /// Класс для разбора xml с информацией о классах и методах библиотеки, который генерируется компилятором при сборке.
    /// </summary>
    internal class VkDocParser
    {
        /// <summary>
        /// Типы сборки.
        /// </summary>
        public List<VkDocType> Types { get; private set; }
        
        /// <summary>
        /// Методы.
        /// </summary>
        public List<VkDocMethod> Methods { get; private set; }
        
        /// <summary>
        /// Свойства.
        /// </summary>
        public List<VkDocProperty> Properties { get; private set; }
        
        /// <summary>
        /// Элементы перечислений.
        /// </summary>
        public List<VkDocEnumItem> EnumItems { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="VkDocParser"/>.
        /// </summary>
        public VkDocParser()
        {
            Types = new List<VkDocType>();
            Methods = new List<VkDocMethod>();
            Properties = new List<VkDocProperty>();
            EnumItems = new List<VkDocEnumItem>();
        }
         
        /// <summary>
        /// Запускает разбор xml с документацией.
        /// </summary>
        /// <param name="content">xml с документацией.</param>
        /// <returns>Список типов сборки.</returns>
        [Pure]
        public IList<VkDocType> Parse(string content)
        {
            Clear();

            if (string.IsNullOrEmpty(content)) return Types;
          
            using (XmlReader xml = new XmlTextReader(new StringReader(content)))
            {
                while (xml.Read())
                {
                    if (xml.NodeType == XmlNodeType.Element && xml.Name == "member")
                    {
                        string name = xml.GetAttribute("name");
                        if (string.IsNullOrEmpty(name)) continue;

                        if (name.StartsWith("T:"))
                        {
                            var vkDocType = GetVkDocType(xml);
                            if (!vkDocType.FullName.StartsWith("JetBrains.Annotations"))
                                Types.Add(vkDocType);
                        }

                        if (name.StartsWith("M:"))
                            Methods.Add(GetVkDocMethod(xml));
                        
                        if (name.StartsWith("P:"))
                            Properties.Add(GetVkDocProperty(xml));

                        if (name.StartsWith("F:"))
                            EnumItems.Add(GetVkDocEnumItem(xml));

                    }
                }
            }

            foreach (VkDocType type in Types)
            {
                VkDocType t = type;
                List<VkDocMethod> methods = Methods.Where(m => m.FullName.StartsWith(t.FullName)).ToList();
                t.Methods = methods;
                methods.ForEach(x => x.Type = t);

                List<VkDocProperty> properties = Properties.Where(m => m.FullName.StartsWith(t.FullName)).ToList();
                t.Properties = properties;
                properties.ForEach(x => x.Type = t);

                List<VkDocEnumItem> enumItems = EnumItems.Where(m => m.FullName.StartsWith(t.FullName)).ToList();
                t.EnumItems = enumItems;
                enumItems.ForEach(x => x.Type = t);
            }

            return Types;
        }

        private void Clear()
        {
            Types.Clear();
            Methods.Clear();
            Properties.Clear();
            EnumItems.Clear();
        }

        [Pure]
        private VkDocEnumItem GetVkDocEnumItem(XmlReader xml)
        {
            var item = new VkDocEnumItem { FullName = GetMemberName(xml, "F:") };

            while (xml.Read())
            {
                if (xml.NodeType == XmlNodeType.EndElement && xml.Name == "member")
                    break;

                if (xml.NodeType == XmlNodeType.Element && xml.Name == "summary")
                    item.Summary = xml.ReadInnerXml().Trim();
            }

            return item;
        }

        [Pure]
        private VkDocProperty GetVkDocProperty(XmlReader xml)
        {
            var prop = new VkDocProperty{FullName = GetMemberName(xml, "P:")};

            while (xml.Read())
            {
                if (xml.NodeType == XmlNodeType.EndElement && xml.Name == "member")
                    break;

                if (xml.NodeType == XmlNodeType.Element && xml.Name == "summary")
                    prop.Summary = xml.ReadInnerXml().Trim();
            }

            return prop;
        }

        [Pure]
        private VkDocMethod GetVkDocMethod(XmlReader xml)
        {
            var method = new VkDocMethod {FullName = GetMemberName(xml, "M:")};

            while (xml.Read())
            {
                if (xml.NodeType == XmlNodeType.EndElement && xml.Name == "member")
                    break;

                if (xml.NodeType == XmlNodeType.Element && xml.Name == "summary")
                {
                    //method.Summary = xml.ReadElementContentAsString().Trim();
                    method.Summary = xml.ReadInnerXml().Trim();
                    continue;
                }

                if (xml.NodeType == XmlNodeType.Element && xml.Name == "remarks")
                {
                    method.Remarks = xml.ReadInnerXml().Trim();
                    continue;
                }

                if (xml.NodeType == XmlNodeType.Element && xml.Name == "returns")
                {
                    method.Returns = xml.ReadInnerXml().Trim();
                    continue;
                }

                if (xml.NodeType == XmlNodeType.Element && xml.Name == "param")
                {
                    method.Params.Add(GetParam(xml));
                    continue;
                }

                if (xml.NodeType == XmlNodeType.Element && xml.Name == "example")
                    method.Examples.Add(xml.ReadInnerXml().Trim());
            }

            return method;
        }

        [Pure]
        private VkDocType GetVkDocType(XmlReader xml)
        {
            var type = new VkDocType {FullName = GetMemberName(xml, "T:")};

            while (xml.Read())
            {
                if (xml.NodeType == XmlNodeType.EndElement && xml.Name == "member")
                    break;

                if (xml.NodeType == XmlNodeType.Element && xml.Name == "summary")
                {   
                    //type.Summary = xml.ReadElementContentAsString().Trim();
                    type.Summary = xml.ReadInnerXml().Trim();
                }
            }

            return type;
        }

        [Pure]
        private VkDocMethodParam GetParam(XmlReader xml)
        {
            var param = new VkDocMethodParam
                {
                    Name = GetMemberName(xml),
                    Description = xml.ReadInnerXml().Trim()
                };

            return param;
        }

        [Pure]
        private string GetMemberName(XmlReader xml)
        {
            return xml.GetAttribute("name");
        }

        [Pure]
        private string GetMemberName(XmlReader xml, string prefix)
        {
            string name = xml.GetAttribute("name");
            return !string.IsNullOrEmpty(name) ? name.Replace(prefix, "") : string.Empty;
        }

        [Pure]
        internal static string GetShortName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return string.Empty;

            int bracketPos = fullName.IndexOf('(');
            if (bracketPos != -1)
            {
                fullName = fullName.Substring(0, bracketPos);
            }

            int pos = fullName.LastIndexOf('.');
            if (pos == -1) return fullName;

            return fullName.Substring(pos + 1);
        }

        [Pure]
        internal static string GetNamespace(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return string.Empty;

            int position = fullName.LastIndexOf('.');
            if (position == -1)
                return string.Empty;

            return fullName.Substring(0, position);
        }
    }
}
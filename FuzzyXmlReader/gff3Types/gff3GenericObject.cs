using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FuzzyXmlReader.gff3Types
{
    /// <summary>
    /// 
    /// </summary>
    [XmlInclude(typeof(CGff3String))]
    public class CGff3GenericObject : CGff3Object
    {
        public CGff3GenericObject() { }
        public CGff3GenericObject(string type, string name, string value)
        {
            Type = type;
            Name = name;
            Value = value;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public class CGff3String : CGff3GenericObject
    {
        public CGff3String() { }
        public CGff3String(string type, string name, string value, string language)
        {
            Type = type;
            Name = name;
            Value = value;
            Language = language;
        }

        [XmlText]
        public new string Value { get; set; }

        [XmlAttribute]
        public string Language { get; set; }
    }

}

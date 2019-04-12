using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FuzzyXmlReader.gff3Types
{
    [XmlInclude(typeof(CGff3List))]
    [XmlInclude(typeof(CGff3Locstring))]
    public abstract class CGff3ListObject<T> : CGff3Object
    {
        public CGff3ListObject() { }
        public CGff3ListObject(string type, string name, List<T> children)
        {
            Type = type;
            Name = name;
            Value = children;
        }

        [XmlIgnore]
        public new List<T> Value { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class CGff3List : CGff3ListObject<gff3struct>
    {
        public CGff3List() { }
        public CGff3List(string type, string name, List<gff3struct> value) : base(type,name,value)
        {
            Type = type;
            Name = name;
            Value = value;
        }

        [XmlArray]
        public new List<gff3struct> Value { get; set; }

    }


    /// <summary>
    /// 
    /// </summary>
    public class CGff3Locstring : CGff3ListObject<CGff3String>
    {
        public CGff3Locstring() { }
        public CGff3Locstring(string type, string name, List<CGff3String> value) : base(type, name, value)
        {
            Type = type;
            Name = name;
            Value = value;
        }

        [XmlArray]
        public new List<CGff3String> Value { get; set; }

    }
}

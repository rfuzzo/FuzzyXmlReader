using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FuzzyXmlReader.gff3Types
{
    [XmlInclude(typeof(CGff3String))]
    [XmlInclude(typeof(CGff3GenericObject))]
    [XmlInclude(typeof(CGff3List))]
    [XmlInclude(typeof(CGff3Locstring))]
    public abstract class CGff3Object
    {
        [XmlIgnore]
        public string Type { get; set; }
        [XmlIgnore]
        public string Name { get; set; }

        [XmlIgnore]
        public object Value { get; set; }

       
        


    }



   


    



   
}

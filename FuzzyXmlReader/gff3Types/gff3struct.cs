using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FuzzyXmlReader.gff3Types
{



    [XmlRoot(Namespace = "http://www.FuzzyXmlReader.fantasy.uri")]
    [XmlInclude(typeof(CGff3String))]
    [XmlInclude(typeof(CGff3GenericObject))]
    [XmlInclude(typeof(CGff3List))]
    [XmlInclude(typeof(CGff3Locstring))]
    public class gff3struct
    {
        public gff3struct()
        {

        }
        public gff3struct(uint id)
        {
            Id = id;

            Data = new List<CGff3Object>();
        }

        [XmlAttribute]
        public uint Id { get; set; }

        [XmlArray]
        public List<CGff3Object> Data { get; set; }





        public List<CGff3Object> GetObjectsByName(string v)
        {
            return Data.FindAll(x => x.Name == v);
        }
        public CGff3GenericObject GetCommonObjectByName(string v)
        {
            return Data.Find(x => x.Name == v) as CGff3GenericObject;
        }
        public CGff3ListObject<T> GetListObjectByName<T>(string v)
        {
            return Data.Find(x => x.Name == v) as CGff3ListObject<T>;
        }



        public CGff3Object GetToplevelObjectByName(string v)
        {
            return Data.Find(x => x.Name == v);
        }


        public gff3struct GetEntryByIndex( int idx)
        {
            List<gff3struct> list = ((CGff3List)GetToplevelObjectByName("EntryList")).Value;
            return list.ElementAt(idx);
        }
        public gff3struct GetReplyByIndex( int idx)
        {
            List<gff3struct> list = ((CGff3List)GetToplevelObjectByName("ReplyList")).Value;
            return list.ElementAt(idx);
        }
       


    }

}

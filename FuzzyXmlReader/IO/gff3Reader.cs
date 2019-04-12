using FuzzyXmlReader.gff3Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FuzzyXmlReader.IO
{
    class gff3Reader
    {
        public gff3Reader()
        {
           
        }

        /// <summary>
        /// Reads an xml and returns a gff3 struct.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static gff3struct Read(string path)
        {
            var doc = XDocument.Load(path);


            XElement in_xstruct = doc.Element("gff3").Element("struct");
            gff3struct parentStruct = new gff3struct(ReadID(in_xstruct));

            ParseStruct(in_xstruct, parentStruct);

            return parentStruct;
        }

        /// <summary>
        /// Read Generic Node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static CGff3GenericObject ReadGeneric(XElement item)
        {
            return new CGff3GenericObject
            (
                ReadType(item),
                ReadLabel(item),
                ReadValue(item)
            );
        }

        /// <summary>
        /// Read String Node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static CGff3String ReadString(XElement item)
        {
            return new CGff3String
            (
                ReadType(item),
                ReadLabel(item),
                ReadValue(item),
                ReadAttribute(item, "language")
            );
        }

        /// <summary>
        /// pass the xstruct node, return a gff3 struct
        /// </summary>
        /// <param name="in_xstruct"></param>
        /// <param name="parentStruct"></param>
        private static void ParseStruct(XElement in_xstruct, gff3struct parentStruct)
        {
            string structID = in_xstruct.Attribute("id").Value;

            foreach (var node in in_xstruct.Elements())
            {
                // if node is a list
                if (ReadType(node) == "list")
                {
                    //get variables
                    var label = ReadLabel(node);
                    var type = ReadType(node);
                    var children = new List<gff3struct>();

                    var xchildren = node.Elements(); //nodes with name struct id=0, id=1 etc...
                    foreach (var childnode in xchildren)
                    {
                        var schild = new gff3struct(ReadID(childnode));
                        ParseStruct(childnode, schild); //passes the struct
                        children.Add(schild);
                    }

                    //create object and add to parent struct
                    var obj = new CGff3List(type, label, children);
                    parentStruct.Data.Add(obj);
                }
                // if node is a common type
                else
                {
                    if (ReadType(node) == "locstring")
                    {
                        var label = ReadLabel(node);
                        var type = ReadType(node);
                        var children = new List<CGff3String>();
                        //strref

                        var xchildren = node.Elements();
                        foreach (var childnode in xchildren)
                        {
                            CGff3String xstring = ReadString(childnode);
                            children.Add(xstring);
                        }

                        var obj = new CGff3Locstring(type, label, children);

                        parentStruct.Data.Add(obj);
                    }
                    // add common type to paremt struct
                    else
                    {
                        parentStruct.Data.Add(ReadGeneric(node));
                    }
                }

            }
        }






        /// <summary>
        /// Reads an Attribute from an XElement
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string ReadAttribute(XElement item, string name)
        {
            string v = "";
            if (item.Attribute(name) != null)
            {
                v = item.Attribute(name).Value;
            }
            return v;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static uint ReadID(XElement item)
        {
            return uint.Parse(ReadAttribute(item, "id"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string ReadLabel(XElement item)
        {
            return ReadAttribute(item, "label");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string ReadType(XElement item)
        {
            return item.Name.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string ReadValue(XElement item)
        {
            return item.Value.ToString();
        }




    }
}

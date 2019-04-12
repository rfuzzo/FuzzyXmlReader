using FuzzyXmlReader.gff3Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FuzzyXmlReader.IO
{
    class gff3Writer
    {
        public gff3Writer()
        {
            
        }

        public static XElement Doc { get; set; }

        /// <summary>
        /// Serialize a gff3struct to xml.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="gff3"></param>
        public static void Write(string path, gff3struct gff3)
        {
            string Path = path;
            gff3struct Gff3 = gff3;

            
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
            };
            XmlSerializer ser = new XmlSerializer(typeof(gff3struct));
            using (XmlWriter writer = XmlTextWriter.Create(path,settings))
            {
                ser.Serialize(writer, gff3, new XmlSerializerNamespaces());
                //XDocument doc = XDocument.Parse(writer);
            }
                

        }



        /// <summary>
        /// Outputs custom text.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="gff3"></param>
        public static void WriteCustomOutput_1(string path, gff3struct gff3)
        {
            gff3struct Gff3 = gff3;
            Doc = new XElement("gff3CustomOutput");

            // starting point
            CGff3ListObject<gff3struct> StartingList = (CGff3ListObject<gff3struct>)gff3.GetToplevelObjectByName("StartingList");
            foreach (gff3struct item in StartingList.Value)
            {
                CGff3GenericObject obj = item.GetCommonObjectByName("Index");
                int idx = int.Parse(obj.Value.ToString());

                // Write Tree
                WriteTree(gff3, Doc, idx, "entry");

            }

            Doc.Save(path);
        }

        /// <summary>
        /// Recursive writing dialogue tree.
        /// </summary>
        /// <param name="gff3"></param>
        /// <param name="idx"></param>
        /// <param name="lookIn"></param>
        private static void WriteTree(gff3struct gff3, XElement printparent, int idx, string lookIn)
        {
            if (lookIn == "entry")
            {
                gff3struct entry = gff3.GetEntryByIndex(idx);

                //PRINT DATA
                var output = PrintData(entry, printparent, idx, lookIn);

                //get replies
                CGff3ListObject<gff3struct> replies = entry.GetListObjectByName<gff3struct>("RepliesList");
                foreach (gff3struct reply in replies.Value)
                {
                    int newidx = int.Parse(reply.GetCommonObjectByName("Index").Value.ToString());
                    WriteTree(gff3, output,  newidx, "reply");
                }

            }
            else if (lookIn == "reply")
            {
                gff3struct reply = gff3.GetReplyByIndex(idx);

                //PRINT DATA
                var output = PrintData(reply, printparent, idx, lookIn);

                //get entries
                CGff3ListObject<gff3struct> entries = reply.GetListObjectByName<gff3struct>("EntriesList");
                foreach (gff3struct entry in entries.Value)
                {
                    int newidx = int.Parse(entry.GetCommonObjectByName("Index").Value.ToString());
                    WriteTree(gff3, output, newidx, "entry");
                }
            }
        }

        /// <summary>
        /// Prints custom data to xml element
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parent"></param>
        /// <param name="idx"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static XElement PrintData(gff3struct data, XElement parent, int idx, string type)
        {
            CGff3Locstring locstring = (CGff3Locstring)data.GetListObjectByName<CGff3String>("Text");
            string Text = "";
            if (locstring != null)
            {

                CGff3String englishText = locstring.Value.FirstOrDefault(x => x.Language == "6");
                if (englishText != null)
                {
                    Text = englishText.Value;
                }
            }
            string Id = data.GetCommonObjectByName("Id").Value.ToString();
            string Speaker = data.GetCommonObjectByName("Speaker").Value.ToString();

            XElement output = new XElement(type,
                new XAttribute("idx", idx),
                new XAttribute("Speaker", Speaker),
                new XAttribute("Text", Text)
                );
            parent.Add(output);

            return output;
        }
    }
}

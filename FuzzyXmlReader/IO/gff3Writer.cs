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
            

            var Doc = new XDocument( );
            var root = new XElement("gff3CustomOutput");
            Doc.AddFirst(root);

            // starting point
            CGff3ListObject<gff3struct> StartingList = (CGff3ListObject<gff3struct>)gff3.GetToplevelObjectByName("StartingList");
            foreach (gff3struct item in StartingList.Value)
            {
                CGff3GenericObject obj = item.GetCommonObjectByName("Index");
                int idx = int.Parse(obj.Value.ToString());

                // Write Tree
                WriteTree(gff3, Doc.Root, idx, "entry", true);

            }

            List<XElement> sections = Doc.Descendants().Where(x => x.Attribute("section") != null).ToList(); //get all sections
            List<XNode> list = sections.Distinct(new XNodeEqualityComparer()).ToList(); //make distinct
            //add to document
            var d = new XDocument();
            d.AddFirst(new XElement("root"));
            foreach (var item in list)
            {
                d.Root.Add(item);
            }
            d.Save(path);

            //Doc.Save(path);
        }

        /// <summary>
        /// Recursive writing dialogue tree.
        /// </summary>
        /// <param name="gff3"></param>
        /// <param name="idx"></param>
        /// <param name="lookIn"></param>
        private static void WriteTree(gff3struct gff3, XElement printparent, int idx, string lookIn, bool isSection)
        {
            if (lookIn == "entry")
            {
                gff3struct entry = gff3.GetEntryByIndex(idx);

                //PRINT DATA
                var output = PrintData(entry, printparent, idx, lookIn, isSection);

                //get replies
                // there can be more than one reply 
                // if there is more than one reply, this marks a CHOICE
                // and we must label all replys as goto points
                CGff3ListObject<gff3struct> replies = entry.GetListObjectByName<gff3struct>("RepliesList");

                foreach (gff3struct reply in replies.Value)
                {
                    int newidx = int.Parse(reply.GetCommonObjectByName("Index").Value.ToString());

                    //if there is a choice, flag the two replies
                    WriteTree(gff3, output, newidx, "reply", replies.Value.Count > 1);
                }



            }
            else if (lookIn == "reply")
            {
                gff3struct reply = gff3.GetReplyByIndex(idx);

                //PRINT DATA
                var output = PrintData(reply, printparent, idx, lookIn, isSection);

                //get entries
                // there can never be more than two entries
                // because nps can't choose
                CGff3ListObject<gff3struct> entries = reply.GetListObjectByName<gff3struct>("EntriesList");
                
                // DEBUG
                if (entries.Value.Count == 0)
                {

                }
                else if (entries.Value.Count == 1)
                {
                    int newidx = int.Parse(entries.Value.First().GetCommonObjectByName("Index").Value.ToString());
                    WriteTree(gff3, output, newidx, "entry", false);
                }
                else
                {
                    throw new NotImplementedException();
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
        private static XElement PrintData(gff3struct data, XElement parent, int idx, string type, bool isSection)
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
            string Interlocutor = data.GetCommonObjectByName("Interlocutor").Value.ToString();
            string NodeType = data.GetCommonObjectByName("NodeType").Value.ToString();

            XElement output = new XElement(type);
            
            output.Add(
                //new XAttribute("idx", idx),
                //new XAttribute("NodeType", NodeType),
                new XAttribute("Speaker", Speaker),
                //new XAttribute("Interlocutor", Interlocutor),
                new XAttribute("Text", Text)
                );
            if (isSection)
                output.Add(new XAttribute("section", $"section_{Text.Split(' ').First()}"));


            parent.Add(output);

            return output;
        }
    }
}

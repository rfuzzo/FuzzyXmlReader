using FuzzyXmlReader.gff3Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            //Generate YML
            XDocument sections = GenerateYML(Doc, path);
           
            Doc.Save(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static XDocument GenerateYML(XDocument doc, string path)
        {
            var d = new XDocument();
            d.AddFirst(new XElement("root"));

            List<XElement> sections = doc.Descendants().Where(x => x.Attribute("section") != null).ToList(); //get all sections
            List<XElement> list = sections.Distinct(new XNodeEqualityComparer()).ToList().Select(x => XElement.Parse(x.ToString())).ToList(); //make distinct
            List<string> sectionIDs = list.Select(x => x.Attribute("sectionRef").Value).ToList();

            var sectionDict = new Dictionary<string, string>();
            foreach (var item in list)
            {
                sectionDict.Add(item.Attribute("idx").Value, item.Attribute("section").Value);
            }

            
            
            foreach (var item in list)
            {
                //tag sectionRefs througout the file (needed because the initial recursion cannot catch references to later sections)


                // delete 
                var sdesc = item.Descendants().Where(x => sectionIDs.Contains(x.Attribute("idx")?.Value)).ToList();
                foreach (var n in sdesc)
                {
                    var refID = n.Attribute("idx").Value;
                    var refName = sectionDict[refID];

                    if (n.Parent.Attribute("CHOICE") != null)
                    {
                        if (n.Parent.Element("CHOICE") != null)
                        {
                            n.Parent.Element("CHOICE")?.Add(new XElement("REF", new XAttribute("NEXT", refName)));
                        }
                        else
                            n.Parent.Add(new XElement("CHOICE", new XElement("REF", new XAttribute("NEXT", refName))));
                    }
                    else
                    {
                        n.Parent.Add(new XElement("REF", new XAttribute("NEXT", refName)));
                    }
                    n.Remove();
                }
                   

                

                d.Root.Add(new XElement("section", item));
            }


            //save
            var fn = Path.GetFileNameWithoutExtension(path);
            var dn = Path.GetPathRoot(path);
            d.Save(Path.Combine(dn,$"{fn}_sections.xml"));
            

            return d;
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

                if (replies.Value.Count > 1)
                {
                    output.Add(new XAttribute("CHOICE", "true"));
                }
                    
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
                new XAttribute("idx", $"{type}_{idx}"),
                //new XAttribute("NodeType", NodeType),
                new XAttribute("Speaker", Speaker),
                //new XAttribute("Interlocutor", Interlocutor),
                new XAttribute("Text", Text)
                );
            if (isSection)
            {
                string sectionname = Text.Split(' ').First();
                //remove all 
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                sectionname = rgx.Replace(sectionname, "");

                output.Add(new XAttribute("section", $"section_{sectionname}"));
                output.Add(new XAttribute("sectionRef", $"{type}_{idx}"));
            }
                


            parent.Add(output);

            return output;
        }
    }
}

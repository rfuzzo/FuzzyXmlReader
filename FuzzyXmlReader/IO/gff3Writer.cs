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


        #region XML
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gff3"></param>
        /// <returns></returns>
        public static XDocument GenerateXML(gff3struct gff3)
        {
            gff3struct Gff3 = gff3;

            #region XML Structure
            var Doc = new XDocument();
            var root = new XElement("root");
            Doc.AddFirst(root);
            Doc.Root.Add(
                new XElement("Settings"),
                new XElement("Dialogscripts"));
            XElement Settings = Doc.Descendants("Settings").First();
            XElement Dialogscripts = Doc.Descendants("Dialogscripts").First();
            #endregion

            //general settings
            //section names
            Settings.Add(new XElement("SectionsList"));

            // Speaker settings
            List<gff3struct> SpeakerList = ((CGff3ListObject<gff3struct>)gff3.GetToplevelObjectByName("SpeakerList"))?.Value;
            List<string> Actors = SpeakerList.Select(x => x.GetCommonObjectByName("Speaker")?.Value.ToString() )?.ToList();
            Settings.Add(new XElement("Player"));
            Settings.Add(new XElement("Actors"));
            foreach (var actor in Actors)
                Settings.Element("Actors").Add(new XElement("Actor", actor));


            //Dialogscripts
            CGff3ListObject<gff3struct> StartingList = (CGff3ListObject<gff3struct>)gff3.GetToplevelObjectByName("StartingList");
            foreach (gff3struct item in StartingList.Value)
            {
                CGff3GenericObject obj = item.GetCommonObjectByName("Index");
                int idx = int.Parse(obj.Value.ToString());

                // Write Tree
                WriteTree(gff3, Dialogscripts, idx, "entry", true);

            }

            return Doc;
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


                if (replies.Value.Count == 0)
                {
                    output.Add(new XAttribute("END", "true"));
                }
                else if(replies.Value.Count > 1)
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
                
                if (entries.Value.Count == 0)
                {
                    //is end? //FIXME
                    output.Add(new XAttribute("END", "true"));
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
            //Document Settings
            var Doc = parent.Document;
            XElement Actors = Doc.Descendants("Settings").First()?.Element("Actors");
            XElement Player = Doc.Descendants("Settings").First()?.Element("Player");
            XElement SectionsList = Doc.Descendants("Settings").First()?.Element("SectionsList");

            //TEXT
            CGff3Locstring locstring = (CGff3Locstring)data.GetListObjectByName<CGff3String>("Text");
            string Text = "";
            if (locstring != null)
            {
                CGff3String englishText = locstring.Value.FirstOrDefault(x => x.Language == "6");
                if (englishText != null)
                    Text = englishText.Value;
            }

            //MISC
            string Interlocutor = data.GetCommonObjectByName("Interlocutor").Value.ToString();
            string NodeType = data.GetCommonObjectByName("NodeType").Value.ToString();
            string Id = data.GetCommonObjectByName("Id").Value.ToString();

            //SPEAKER
            string Speaker = data.GetCommonObjectByName("Speaker").Value.ToString();
            if (Speaker == "__player__")
                Speaker = "geralt";
            if (Speaker == "__owner__")
                Speaker = "npc";
            //write speakers to settings
            if (Actors == null)
                throw new NotImplementedException(); //should never go offbut keep it for testing
            List<string> ActorList = Actors.Elements().Select(x => x.Value.ToString()).ToList();
            if (!ActorList.Contains(Speaker))
                Actors.Add(new XElement("Actor", Speaker));
            if (Speaker == "geralt" && Player.Value != null)
                Player.Value = Speaker;

            //Add data
            XElement output = new XElement(type);
            
            output.Add(
                new XAttribute("idx", $"{type}_{idx}"),
                //new XAttribute("NodeType", NodeType),
                new XAttribute("Speaker", Speaker),
                //new XAttribute("Interlocutor", Interlocutor),
                new XAttribute("Text", Text)
                );

            //add section data
            if (isSection)
            {
                string sectionname = Text.Split(' ').First();
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                sectionname = rgx.Replace(sectionname, "");

                if (type == "entry")
                    sectionname = $"section_start_{idx}";
                else
                    sectionname = $"section_{sectionname}";

                List<string> sectionnames = SectionsList.Elements().Select(x => x.Value.ToString()).ToList();
                if (sectionnames.Contains(sectionname))
                    sectionname += $"_{idx}";
                SectionsList.Add(new XElement("Section", $"{type}_{idx}", new XAttribute("Name", sectionname)));

                output.Add(new XAttribute("section", sectionname));
                output.Add(new XAttribute("sectionRef", $"{type}_{idx}"));

                
               
            }

            parent.Add(output);
            return output;
        }

        #endregion

        #region SectionsXML
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_indoc"></param>
        /// <returns></returns>
        public static XDocument GenerateSectionsXML(XDocument _indoc)
        {
            #region XML Structure
            var Doc = new XDocument();
            var root = new XElement("root");
            Doc.AddFirst(root);
            Doc.Root.Add(_indoc.Descendants("Settings").First(),
                new XElement("Dialogscripts"));
            XElement Settings = Doc.Descendants("Settings").First();
            XElement Dialogscripts = Doc.Descendants("Dialogscripts").First();
            #endregion



            List<XElement> sections = _indoc.Descendants().Where(x => x.Attribute("section") != null).ToList(); //get all sections
            List<XElement> list = sections.Distinct(new XNodeEqualityComparer()).ToList().Select(x => XElement.Parse(x.ToString())).ToList(); //make distinct

            List<XElement> xSections = Settings.Element("SectionsList").Elements().ToList();
            List<string> SectionRefs = xSections.Select(x => x.Value.ToString()).ToList();
            



            // loop through all section nodes
            foreach (var item in list)
            {
                string sectionName = item.Attribute("section").Value;

                //delete sectionRefs througout the file (needed because the initial recursion cannot catch references to later sections)
                var sdesc = item.Descendants().Where(x => SectionRefs.Contains(x.Attribute("idx")?.Value)).ToList();
                foreach (var n in sdesc)
                {
                    string refID = n.Attribute("idx").Value;
                    string refName = xSections.Find(x => x.Value == refID).Attribute("Name").Value;
                    string Text = n.Attribute("Text")?.Value;

                    //if was choice
                    if (n.Parent.Attribute("CHOICE") != null)
                    {
                        if (n.Parent.Element("CHOICE") != null)
                        {
                            n.Parent.Element("CHOICE")?.Add(new XElement("CREF",
                                new XAttribute("NEXT", refName),
                                new XAttribute("Text", Text)
                                ));
                        }
                        else
                            n.Parent.Add(new XElement("CHOICE",
                                new XElement("CREF",
                                new XAttribute("NEXT", refName),
                                new XAttribute("Text", Text)
                                )));
                    }
                    else
                    {
                        n.Parent.Add(new XElement("REF",
                            new XAttribute("NEXT", refName),
                            new XAttribute("Text", Text)
                            ));
                    }
                    n.Remove();
                }

                //add end sections
                if (sdesc.Count == 0)
                {
                    var last = item.Descendants().Where(x => x.Attribute("END") != null).First();
                    last.Add(new XElement("REF", new XAttribute("NEXT", "section_exit")));
                }



                Dialogscripts.Add(new XElement("section", new XAttribute("Name", sectionName), item));
            }

            //add exit section
            Dialogscripts.Add(new XElement("section", 
                new XAttribute("Name", "section_exit"),
                new XElement("EXIT")));


            return Doc;
        }
        #endregion
    }
}

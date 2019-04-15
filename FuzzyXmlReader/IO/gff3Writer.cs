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
    public struct SDialogscriptdata
    {
        public string questfact { get; set; }
    }


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
            XElement Dialogscripts = Doc.Descendants("Dialogscripts").First();
            XElement Settings = Doc.Descendants("Settings").First();
            Settings.Add(new XElement("Actors"));
            Settings.Add(new XElement("Player"));
            Settings.Add(new XElement("SectionsList"));
            #endregion
            

            // Speaker settings
            List<gff3struct> SpeakerList = ((CGff3ListObject<gff3struct>)gff3.GetGenericObjectByName("SpeakerList"))?.Value;
            List<string> Actors = SpeakerList.Select(x => x.GetCommonObjectByName("Speaker")?.Value.ToString() )?.ToList();
            foreach (var actor in Actors)
                Settings.Element("Actors").Add(new XElement("Actor", actor));

            //Dialogscripts
            CGff3ListObject<gff3struct> StartingList = (CGff3ListObject<gff3struct>)gff3.GetGenericObjectByName("StartingList");
            for (int i = 0; i < StartingList.Value.Count; i++)
            {
                
                //add generic start sections to section list
                // FIXME pause?
                var startelement = new XElement("PAUSE",
                    new XAttribute("section", $"section_start_{i}"),
                    new XAttribute("ref", $"start_{i}")
                    );
                Dialogscripts.Add(startelement);
                Settings.Element("SectionsList").Add(new XElement("Section", $"start_{i}", new XAttribute("Name", $"section_start_{i}")));


                //
                gff3struct item = (gff3struct)StartingList.Value[i];
                CGff3GenericObject obj = item.GetCommonObjectByName("Index");
                int idx = int.Parse(obj.Value.ToString());

                // Write Tree
                WriteTree(gff3, startelement, idx, "entry", true);

                
            }

            //Modify Speaker Section
            string player = Settings.Element("Player").Value;
            List<string> actors = Settings.Element("Actors").Descendants()?.Select(x => x.Value).ToList();
            if (String.IsNullOrEmpty(player))
                Settings.Element("Player").Value = actors.First();

            return Doc;
        }

        /// <summary>
        /// Recursive writing dialogue tree.
        /// </summary>
        /// <param name="gff3"></param>
        /// <param name="idx"></param>
        /// <param name="type"></param>
        private static void WriteTree(gff3struct gff3, XElement printparent, int idx, string type, bool isSection)
        {
            XElement output = new XElement(type);
            if (type == "entry")
            {
                gff3struct entry = gff3.GetEntryByIndex(idx);

                //PRINT DATA
                
                bool isquest = AnnotateXML(entry, printparent, idx, type, isSection, ref output);

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
                    bool ischildSection = replies.Value.Count > 1 || isquest;
                    WriteTree(gff3, output, newidx, "reply", ischildSection);
                }
            }
            else if (type == "reply")
            {
                gff3struct reply = gff3.GetReplyByIndex(idx);

                //PRINT DATA
                var isquest = AnnotateXML(reply, printparent, idx, type, isSection, ref output);

                //get entries
                // there can never be more than two entries
                // because nps can't choose
                CGff3ListObject<gff3struct> entries = reply.GetListObjectByName<gff3struct>("EntriesList");
                
                if (entries.Value.Count == 0)
                {
                    output.Add(new XAttribute("END", "true"));
                }
                else if (entries.Value.Count == 1)
                {
                    int newidx = int.Parse(entries.Value.First().GetCommonObjectByName("Index").Value.ToString());
                    WriteTree(gff3, output, newidx, "entry", isquest);
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
        private static bool AnnotateXML(gff3struct data, XElement parent, int idx, string type, bool isSection, ref XElement output)
        {
            //Document Settings
            var Doc = parent.Document;
            XElement Actors = Doc.Descendants("Settings").First()?.Element("Actors");
            XElement Player = Doc.Descendants("Settings").First()?.Element("Player");

            List<XAttribute> attributes = new List<XAttribute>();
            //attributes.Add(new XAttribute("Idx", idx));
            output.Add(new XAttribute("ref", $"{type}_{idx}"));

            //TEXT
            CGff3Locstring locstring = (CGff3Locstring)data.GetListObjectByName<CGff3String>("Text");
            string Text = "";
            if (locstring != null)
            {
                CGff3String englishText = locstring.Value.FirstOrDefault(x => x.Language == "6");
                if (englishText != null)
                    Text = englishText.Value;
            }
            attributes.Add(new XAttribute("Text", Text));

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
            //Add actors to actorlist
            if (!ActorList.Contains(Speaker))
                Actors.Add(new XElement("Actor", Speaker));
            if (Speaker == "geralt" && Player.Value != null)
                Player.Value = Speaker;
            attributes.Add(new XAttribute("Speaker", Speaker));

            // Adding soundfile var
            string Sound = data.GetCommonObjectByName("Sound").Value.ToString();
            attributes.Add(new XAttribute("Sound", Sound));

            //OTHER 
            var dsdata = AttributeData(data, ref attributes);
            //QUESTS
           

            //ADD
            output.Add(attributes.ToArray());

            //add section data
            if (isSection)
            {
               
                //remove special charactersand to lower
                string sectionname = Text.Split(' ').First();
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                sectionname = $"section_{rgx.Replace(sectionname, "").ToLower()}_{idx}";

                //add to sections list
                AddToSectionsNoDuplicates(Doc, sectionname, $"{type}_{idx}");
                output.Add(new XAttribute("section", sectionname));
            }

            bool isQuestParent = !String.IsNullOrEmpty(dsdata.questfact);
            if (isQuestParent)
            {
                //add to quest list
                AddToSectionsNoDuplicates(Doc, $"script_setfact_{dsdata.questfact}", $"quest_{idx}");

                //add quest xml tags
               
                var scriptelement = new XElement("SCRIPT",
                    new XAttribute("function", "AddFact_S"),
                    new XElement("parameter", 
                        new XAttribute("factName", $"{dsdata.questfact}"),
                        new XAttribute("value", $"1"),
                        new XAttribute("validFor", $"0")
                        ));
                var questelement = new XElement("QUEST",
                   new XAttribute("Name", dsdata.questfact),
                   new XAttribute("section", $"script_setfact_{dsdata.questfact}"),
                   new XAttribute("ref", $"quest_{idx}"),
                   scriptelement
                   );

                // reshuffle nodes
                output.Add(questelement);
                parent.Add(output);
                output = output.Element("QUEST");
            }
            else
            {
                parent.Add(output);
            }

            return isQuestParent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        private static void AddToSectionsNoDuplicates(XDocument doc, string name, string id)
        {
            XElement SectionsList = doc.Descendants("Settings").First()?.Element("SectionsList");

            var sectionelement = new XElement("Section", id, new XAttribute("Name", name));
            var d = SectionsList.Elements().ToList();
            if (!d.Contains(sectionelement, new XNodeEqualityComparer()))   //does not contain exactlty that node (name and ref)
                SectionsList.Add(sectionelement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        private static SDialogscriptdata AttributeData(gff3struct data, ref List<XAttribute> ret)
        {
            var sdata = new SDialogscriptdata();

            //MISC
            //ret.Add(new XAttribute("Interlocutor", data.GetCommonObjectByName("Interlocutor").Value.ToString()));
            //ret.Add(new XAttribute("NodeType", data.GetCommonObjectByName("NodeType").Value.ToString()));
            //ret.Add(new XAttribute("Id", data.GetCommonObjectByName("Id").Value.ToString()));

            string qf = data.GetCommonObjectByName("Quest")?.Value?.ToString();
            bool isquest = !string.IsNullOrEmpty(qf);
            if (isquest)
            {
                //remove special characters
                qf = qf.Replace(' ', '_');
                Regex rgx = new Regex("[^a-zA-Z0-9_]");
                qf = rgx.Replace(qf, "").ToLower();
                
                ret.Add(new XAttribute("Quest", qf));
                sdata.questfact = qf;
            }



            return sdata;
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
            



            // loop through all start section nodes
            foreach (var item in list)
            {
                string sectionName = item.Attribute("section").Value;

                //check if any of the descendents have a reference to another section
                var refnode = item.Descendants().Where(x => SectionRefs.Contains(x.Attribute("ref")?.Value)).ToList();
                foreach (var n in refnode)
                {
                    string refID = n.Attribute("ref").Value;
                    string refName = xSections.Find(x => x.Value == refID).Attribute("Name").Value;
                    string Text = n.Attribute("Text")?.Value;

                    //if was choice
                    if (n.Parent.Attribute("CHOICE") != null)
                    {
                        // if there is already a choice node
                        if (n.Parent.Element("CHOICE") != null)
                        {
                            n.Parent.Element("CHOICE")?.Add(new XElement("CREF",
                                new XAttribute("NEXT", refName),
                                new XAttribute("Text", Text)
                                ));
                        }
                        //otherwise add a choice node
                        else
                            n.Parent.Add(new XElement("CHOICE",
                                new XElement("CREF",
                                new XAttribute("NEXT", refName),
                                new XAttribute("Text", Text)
                                )));
                    }
                    // no choice but found a reference
                    else
                    {
                        n.Parent.Add(new XElement("REF",
                            new XAttribute("NEXT", refName)
                            //new XAttribute("Text", Text)
                            ));
                    }
                    n.Remove();
                }

                //if it is a section but has no descendents that point to any other section
                //add EXIT
                if (refnode.Count == 0)
                {
                    if (item.Attribute("END")?.Value != null)
                    {
                        item.Add(new XElement("REF", new XAttribute("NEXT", "section_exit")));
                    }
                    else
                    {
                        var last = item.Descendants().Where(x => x.Attribute("END") != null).First();
                        last.Add(new XElement("REF", new XAttribute("NEXT", "section_exit")));
                    }
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

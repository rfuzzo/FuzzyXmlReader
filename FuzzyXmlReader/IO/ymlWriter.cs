using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FuzzyXmlReader.IO
{
    class ymlWriter
    {




        /// <summary>
        /// Convert XDocument to YML format.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="gff3"></param>
        public static void Write(string path, XDocument gff3)
        {
            XElement Settings = gff3.Descendants("Settings").First();
            XElement Dialogscripts = gff3.Descendants("Dialogscripts").First();
            

            using (StreamWriter sw = new StreamWriter(path))
            {
                using (IndentedTextWriter iw = new IndentedTextWriter(sw))
                {



                    // Dialogscripts
                    #region Dialogscripts
                    iw.Indent = 0;
                    iw.WriteLine("dialogScript:");


                    // Speakers
                    #region Speakers
                    List<string> Actors = Settings.Element("Actors").Elements().Select(x => x.Value.ToString()).ToList();
                    iw.Indent = 1;
                    iw.WriteLine($"player: geralt");
                    iw.WriteLine($"actors: [{String.Join(", ", Actors)}]");
                    iw.WriteLine();
                    #endregion

                    // sections
                    #region Sections
                    foreach (var section in Dialogscripts.Elements())
                    {
                        iw.Indent = 1;
                        string sectionName = $"{section.Attribute("Name").Value}:";
                        iw.WriteLine(sectionName);

                        //List<XNode> descedentNodes = section.DescendantNodes().ToList();
                        List<XElement> descedents = section.Descendants().ToList();
                        
                        foreach (XElement el in descedents)
                        {
                            iw.Indent = 2;
                            string Speaker = el.Attribute("Speaker")?.Value;
                            string Text = el.Attribute("Text")?.Value;

                            if (!(String.IsNullOrEmpty(Speaker) || String.IsNullOrEmpty(Text)))
                            {
                                iw.WriteLine($"- {Speaker}: \"{Text}\"");
                            }
                            else if (el.Name == "CHOICE")
                            {
                                foreach (var choice in el.Elements())
                                {
                                    string SectionName = choice.Attribute("NEXT")?.Value;
                                    string choiceText = choice.Attribute("Text")?.Value;
                                    iw.WriteLine($"- [\"{choiceText}\", {SectionName}]");
                                }
                            }
                            else if (el.Name == "REF")
                            {
                                string SectionName = el.Attribute("NEXT")?.Value;
                                iw.WriteLine($"- NEXT: {SectionName}");
                            }
                            else if (el.Name == "EXIT")
                            {
                                iw.WriteLine($"- EXIT");
                            }


                        }
                        iw.WriteLine();
                    }
                    #endregion
                    #endregion
                }
            }
        }
    }
}

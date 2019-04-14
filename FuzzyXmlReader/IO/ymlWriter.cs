using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                    List<string> Actors = Settings.Element("Actors").Elements().Select(x => x.Value.ToString()).ToList();


                    #region Repository
                    iw.Indent = 0;
                    iw.WriteLine("repository:");
                    iw.Indent = 1;
                    iw.WriteLine("actors:");                    
                    foreach (string actor in Actors)
                    {
                        iw.Indent = 2;
                        iw.WriteLine(actor+":");
                        if(actor == "geralt")
                        {
                            iw.Indent = 3;
                            iw.WriteLine("template: \"gameplay/templates/characters/player/player.w2ent\"");
                            iw.WriteLine();
                        }
                        else
                        {
                            iw.Indent = 3;
                            iw.WriteLine("template: \"dlc/dlcw1/data/characters/eskel.w2ent\"");
                            iw.WriteLine("appearance: [ \"default\" ]");
                            iw.WriteLine();
                        }
                    }

                    iw.Indent = 1;
                    iw.WriteLine("cameras:");
                    /*var dialogCameras = File.ReadLines("D:\\W1Files\\cameras_"+Actors.Count.ToString()+".txt");
                    foreach(string line in dialogCameras)
                    {
                        iw.Indent = 0;
                        iw.WriteLine(line);
                    }*/


                    iw.WriteLine();
                    #endregion

                    #region Production
                    iw.Indent = 0;
                    iw.WriteLine("production:");
                    iw.WriteLine();
                    #endregion

                    #region Storyboard
                    iw.Indent = 0;
                    iw.WriteLine("storyboard:");
                    iw.WriteLine();
                    #endregion


                    // Dialogscripts
                    #region Dialogscript
                    iw.Indent = 0;
                    iw.WriteLine("dialogscript:");


                    // Speakers
                    #region Speakers
                    
                    string Player = Settings.Element("Player").Value;
                    iw.Indent = 1;
                    iw.WriteLine($"player: {Player}");
                    iw.WriteLine($"actors: [{String.Join(", ", Actors)}]");
                    iw.WriteLine();
                    #endregion

                    // sections
                    #region Sections
                    foreach (var section in Dialogscripts.Elements())
                    {
                        iw.Indent = 1;
                        string sectionName = $"{section.Attribute("Name").Value}:";
                        int shotID = 1;
                        iw.WriteLine(sectionName);

                        List<XElement> descedents = section.Descendants().ToList();
                        
                        foreach (XElement el in descedents)
                        {
                            iw.Indent = 2;
                            string Speaker = el.Attribute("Speaker")?.Value;
                            string Text = el.Attribute("Text")?.Value;
                            string Sound = el.Attribute("Sound")?.Value;                            

                            if (el.Name == "CHOICE")
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
                                iw.WriteLine($"- OUTPUT: Out1");
                                iw.WriteLine($"- EXIT");
                            }
                            else if (el.Name == "PAUSE")
                            {
                                iw.WriteLine($"- PAUSE: 0");
                            }
                            else if (!(String.IsNullOrEmpty(Speaker) || String.IsNullOrEmpty(Text)))
                            {
                                var lines = File.ReadLines("D:\\W1Files\\audiolengths.txt");
                                string audioLength = "";
                                string StringID = "";
                                foreach (var line in lines)
                                {
                                    var arr = line.Split(';');
                                    if (arr[0] == Sound)
                                    {
                                        audioLength = arr[1];
                                        StringID = arr[2];
                                        break;
                                    }
                                }

                                string shotName = $"shot_{ shotID.ToString()}_{Speaker}";
                                shotID += 1;
                                iw.WriteLine($"- CUE: {shotName}");
                                iw.WriteLine($"- {Speaker}: \"[{audioLength}]{StringID}|{Text}\"");

                                File.AppendAllText(@"D:\W1Files\locale.en.csv", $"{StringID}|00000000||{Text}" + Environment.NewLine);
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

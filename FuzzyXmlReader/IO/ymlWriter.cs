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
            string ResourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

            using (StreamWriter sw = new StreamWriter(path))
            {
                using (IndentedTextWriter iw = new IndentedTextWriter(sw, "  "))
                {
                    List<string> Actors = Settings.Element("Actors").Elements().Select(x => x.Value.ToString()).ToList();

                    string filename = path.Substring(path.LastIndexOf("\\") + 1, path.Length - path.LastIndexOf("\\") - 1).Replace(".yml", "");
                    string newActorName = "";
                    if(Actors.Count == 2 && Actors[0] == "npc")
                    {
                        if (filename.Contains("zygfryd"))
                        {
                            Actors[0] = "siegfried";
                            newActorName = "siegfried";
                        }                            
                        if (filename.Contains("zygfryd"))
                        {
                            Actors[0] = "siegfried";
                            newActorName = "siegfried";
                        }
                            
                    }
                    else if (Actors.Count == 2 && Actors[1] == "npc")
                    {

                    }
                    // --------------------------------------------------------------------------------------
                    // Writing Repo
                    // --------------------------------------------------------------------------------------
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
                    var dialogCameras = File.ReadLines(Path.Combine(ResourceDir, "cameras.csv"));
                    foreach(string line in dialogCameras)
                    {
                        if(line.StartsWith("dialogset_1_vs_1_around_npc"))
                        {
                            string[] camSettings = line.Split(';');
                            iw.Indent = 2;
                            iw.WriteLine($"{camSettings[1]}:");
                            iw.Indent = 3;
                            iw.WriteLine($"fov: {camSettings[4]}");
                            iw.WriteLine("transform:");
                            iw.Indent = 4;
                            iw.WriteLine($"pos: {camSettings[2]}");
                            iw.WriteLine($"rot: {camSettings[3]}");
                            iw.Indent = 3;
                            iw.WriteLine($"#zoom: {camSettings[5]}");
                            iw.WriteLine("dof:");
                            iw.Indent = 4;
                            iw.WriteLine("aperture: [ 28.2495, 1.27007]");
                            iw.WriteLine($"blur: [ {camSettings[10]}, {camSettings[7]}]");
                            iw.WriteLine($"focus: [ {camSettings[9]}, {camSettings[6]}]");
                            iw.WriteLine($"intensity: {camSettings[8]}");
                            iw.Indent = 3;
                            iw.WriteLine("event_generator:");
                            iw.Indent = 4;
                            iw.WriteLine($"plane: \"{camSettings[12]}\"");
                            iw.WriteLine($"tags: {camSettings[13].Replace("[","[ \"").Replace("]", "\" ]")}");
                            iw.WriteLine();
                        }                        
                    }
                    iw.WriteLine();
                    #endregion
                    // --------------------------------------------------------------------------------------
                    // Writing Production
                    // --------------------------------------------------------------------------------------
                    #region Production
                    iw.Indent = 0;
                    iw.WriteLine("production:");

                    iw.Indent = 1;
                    iw.WriteLine("settings:");
                    iw.Indent = 2;
                    iw.WriteLine("sceneid: 1");
                    iw.WriteLine("strings-idspace: 0000");
                    iw.WriteLine("strings-idstart: 0");
                    iw.WriteLine();


                    iw.Indent = 1;
                    iw.WriteLine($"placement: \"{Actors[0]}\"");
                    iw.WriteLine();
                    iw.WriteLine("assets:");
                    iw.Indent = 2;
                    iw.WriteLine("actors:");
                    foreach (string actor in Actors)
                    {
                        iw.Indent = 3;
                        iw.WriteLine($"{actor}:");
                        iw.Indent = 4;
                        iw.WriteLine($"repo: \"{actor}\"");
                        iw.WriteLine("by_voicetag: true");
                        if(actor == "geralt")
                            iw.WriteLine($"tags: [ \"PLAYER\" ]");
                        else
                            iw.WriteLine($"tags: [ \"{actor}\" ]");
                        iw.WriteLine();
                    }

                    iw.Indent = 2;
                    iw.WriteLine("cameras:");
                    foreach (string line in dialogCameras)
                    {
                        if (line.StartsWith("dialogset_1_vs_1_around_npc"))
                        {
                            string[] camSettings = line.Split(';');
                            iw.Indent = 3;
                            iw.WriteLine($"{camSettings[1]}:");
                            iw.Indent = 4;
                            iw.WriteLine($"repo: \"{camSettings[1]}\"");
                            iw.WriteLine();
                        }
                    }


                    iw.WriteLine();
                    #endregion
                    // --------------------------------------------------------------------------------------
                    // Writing Storyboard
                    // --------------------------------------------------------------------------------------
                    #region Storyboard
                    iw.Indent = 0;
                    iw.WriteLine("storyboard:");
                    iw.Indent = 1;
                    iw.WriteLine("defaults:");                    
                    if(Actors.Count == 2)
                    {
                        iw.Indent = 2;
                        iw.WriteLine("placement:");
                        iw.Indent = 3;
                        iw.WriteLine($"{Actors[1]}: [[ 0.0, 1.6, 0.0 ], [ 0.0, 0.0, 180.0 ]]");
                        iw.WriteLine($"{Actors[0]}: [[ 0.0, 0.0, 0.0 ], [ 0.0, 0.0, 0.0 ]]");
                        iw.WriteLine();
                        iw.Indent = 2;
                        iw.WriteLine("camera:");
                        iw.Indent = 3;
                        iw.WriteLine($"{Actors[1]}: 1_2_medium_ext");
                        iw.WriteLine($"{Actors[0]}: 2_1_medium_ext");
                        iw.WriteLine();
                    }

                    iw.WriteLine();
                    #endregion


                    // --------------------------------------------------------------------------------------
                    // Writing Dialogscript
                    // --------------------------------------------------------------------------------------
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
                                iw.WriteLine("- CUE: shot_choice");
                                iw.WriteLine("- CHOICE:");
                                foreach (var choice in el.Elements())
                                {
                                    iw.Indent = 3;
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
                            else if (el.Name == "SCRIPT")
                            {
                                iw.WriteLine($"- SCRIPT:");
                                iw.Indent = 4;

                                iw.WriteLine($"function: {el.Attribute("function").Value}");
                                iw.WriteLine($"parameter:");
                                iw.Indent = 5;
                                var param = el.Element("parameter");
                                iw.WriteLine($"- factName: {param.Attribute("factName").Value}");
                                iw.WriteLine($"- value: {param.Attribute("value").Value}");
                                iw.WriteLine($"- validFor: {param.Attribute("validFor").Value}");
                            }
                            else if (el.Name == "reply" || el.Name == "entry")
                            {
                                if(String.IsNullOrEmpty(Speaker) || String.IsNullOrEmpty(Text)) //FIXME does that happen?
                                {
                                    continue;
                                }

                                var lines = File.ReadLines(Path.Combine(ResourceDir, "audiolengths.txt"));
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
                                
                                string shotName = $"shot_{ shotID.ToString()}";
                                shotID += 1;
                                iw.WriteLine($"- CUE: {shotName}");
                                if (newActorName != "" && Speaker == "npc")
                                    iw.WriteLine($"- {newActorName}: \"[{audioLength}]{StringID}|{Text}\"");
                                else
                                    iw.WriteLine($"- {Speaker}: \"[{audioLength}]{StringID}|{Text}\"");

                                // skip these
                                if(!Text.Contains("[continue]"))
                                    File.AppendAllText(Path.Combine(ResourceDir, "locale.en.csv"), $"{StringID}|00000000||{Text}" + Environment.NewLine);
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

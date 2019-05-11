using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using FuzzyXmlReader.gff3Types;
using FuzzyXmlReader.IO;

namespace FuzzyXmlReader
{


    class Program
    {
        static int Main(string[] args)
        {
            string stringsfie = ";meta[language=en]\n; id      |key(hex)|key(str)| text\n";
            string ResourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            File.WriteAllText(Path.Combine(ResourceDir, "locale.en.csv"), stringsfie);

            /*
            string[] Files = new string[]
            {
                //Path.Combine(ResourceDir, "cs0_06.xml"),
                //Path.Combine(ResourceDir, "cs0_02b.xml"),
                Path.Combine(ResourceDir, "cn_zygfryd01.xml"),
            };
            */

            string[] Files = Directory.GetFiles("D:\\Xoreos Decoder v1\\dlg_export\\");



            foreach (var file in Files)
            {
                Console.WriteLine(file.ToString());
                DumpFile(file);
            }
            


            

            return 1;
        }

        private static void DumpFile(string infile)
        {
            #region Save Settings
            var filename = Path.GetFileNameWithoutExtension(infile);
            var fileDirectory = Path.GetDirectoryName(infile);
            var newDirectory = Path.Combine(fileDirectory, $"out/{filename}");
            var ymlDirectory = Path.Combine(fileDirectory, $"out");

            if (!Directory.Exists(newDirectory))
                Directory.CreateDirectory(newDirectory);

            string outfile = Path.Combine(newDirectory, $"{filename}_out.xml");
            string outfile_sections = Path.Combine(newDirectory, $"{filename}_sections.xml");
            string outfile_yml = Path.Combine(ymlDirectory, $"{filename}.yml");

            #endregion 

            var parsedClass = gff3Reader.Read(infile);

            XDocument xml = gff3Writer.GenerateXML(parsedClass);
            XDocument xml_sections = gff3Writer.GenerateSectionsXML(xml);

            //debug
            //xml.Save(outfile);
            //xml_sections.Save(outfile_sections);

            ymlWriter.Write(outfile_yml, xml_sections);
        }
    }
}

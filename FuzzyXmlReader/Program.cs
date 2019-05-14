using FuzzyXmlReader.gff3Types;
using FuzzyXmlReader.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FuzzyXmlReader
{


    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            #region Info
            string stringsfile = ";meta[language=en]\n; id      |key(hex)|key(str)| text\n";
            string ResourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            File.WriteAllText(Path.Combine(ResourceDir, "locale.en.csv"), stringsfile);
            DirectoryInfo indir = new DirectoryInfo(@"D:\\Xoreos Decoder v1\\dlg_export\\");
            var files = indir.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
            var log = new List<string>();

            //int customexportlength = 100;
            int customexportlength = files.Length;
            #endregion

            #region Exporting
            Console.WriteLine($"Processing {customexportlength} out of {files.Length} Files.");
            for (int i = 0; i < files.Length; i++)
            {
                //custom export length
                if (i > customexportlength)
                    break;

                string path = files[i].FullName;

                try
                {
                    Xml2Yml(path);
                }
                catch (Exception ex)
                {
                    string logmessage = $"{path};{ ex.Message}";
                    log.Add(logmessage);
                    Console.WriteLine($"{i}/{files.Length}    {logmessage}");
                }

            }
            #endregion

            #region Logging
            if (log.Count > 0)
            {
                string logfilePath = Path.Combine(indir.Parent.FullName, "log.txt");

                using (StreamWriter sw = new StreamWriter(logfilePath))
                {
                    sw.WriteLine($"Exported {(customexportlength - log.Count)} out of {customexportlength} Files succesfully.");
                    sw.WriteLine($"Skipped {log.Count} Files.");
                    sw.WriteLine($"------------------------------------------------");

                    foreach (string s in log)
                    {
                        sw.WriteLine(s);
                    }
                }
            }
            

            Console.WriteLine($"Exported {(customexportlength - log.Count)} out of {customexportlength} Files succesfully.");
            Console.WriteLine($"Skipped {log.Count} Files.");
            #endregion


            return 1;
        }

        /// <summary>
        /// Exports a xoreos xml (export) to yml.
        /// </summary>
        /// <param name="infile"></param>
        private static void Xml2Yml(string infile)
        {
            #region Save Settings
            var filename = Path.GetFileNameWithoutExtension(infile);
            var fileDirectory = Path.GetDirectoryName(infile);
            //var newDirectory = Path.Combine(fileDirectory, $"out/{filename}");
            var ymlDirectory = Path.Combine(fileDirectory, $"out");

            if (!Directory.Exists(ymlDirectory))
                Directory.CreateDirectory(ymlDirectory);

            //string outfile = Path.Combine(newDirectory, $"{filename}_out.xml");
            //string outfile_sections = Path.Combine(newDirectory, $"{filename}_sections.xml");
            string outfile_yml = Path.Combine(ymlDirectory, $"{filename}.yml");

            #endregion 

            gff3struct parsedClass = gff3Reader.Read(infile);


            var gffWriter = new gff3Writer(parsedClass, infile);
            gffWriter.GenerateXML();
            //gffWriter.XDOC.Save(outfile); //dbg

            gffWriter.GenerateSectionsXML();
            //gffWriter.XDOC_SECTIONS.Save(outfile_sections); //dbg

            ymlWriter.Write(outfile_yml, gffWriter.XDOC_SECTIONS);
        }
    }
}

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
            string infile = @"E:\cn_zygfryd01.xml";
            string outfile = @"E:\out_cn_zygfryd01.xml";

            var parsedClass = gff3Reader.Read(infile);

            gff3Writer.WriteCustomOutput_1(outfile, parsedClass);
            //gff3Writer.Write(outfile, parsedClass);

            return 1;
        }
    }
}

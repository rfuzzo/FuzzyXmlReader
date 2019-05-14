using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyXmlReader
{
    class Gff3Exception : Exception
    {
        public string Message {get;set;}


        public Gff3Exception(string message) : base(message)
        {
            Message = message;
        }
    }
}

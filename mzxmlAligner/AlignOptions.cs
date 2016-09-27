using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace mzxmlAligner
{
    class AlignOptions
    {
        [Option('a', "alignmentMap", Required = true, HelpText = "path of alignment map for linear alignments")]
        public string alignmentMap { get; set; }

        [Option('f', "files", Required = true, HelpText = "path of directory containing mzXMLs")]
        public string files { get; set; }
    }
}

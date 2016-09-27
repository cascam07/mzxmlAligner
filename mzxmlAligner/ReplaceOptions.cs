using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace mzxmlAligner
{
    class ReplaceOptions
    {
        [Option('s', "scansFiles", Required = false, HelpText = "path of directory containing scans files for RT replacement")]
        public string scansFiles { get; set; }

        [Option('f', "files", Required = true, HelpText = "path of directory containing mzXMLs")]
        public string files { get; set; }
    }
}

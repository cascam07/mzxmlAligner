using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace mzxmlAligner
{
    class Options
    {
        [VerbOption("align", HelpText = "Use a list of alignment maps with headers \"Name\", \"Slope\", \"Intecept\" to linearly align mzXML files")]
        public AlignOptions AlignVerb { get; set; }

        [VerbOption("replace", HelpText = "Use a directory of *MS_scans.csv files with corrected retention time columns to correct RT fields in mzXML files")]
        public ReplaceOptions ReplaceVerb { get; set; }

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage()
        {
            var help = new StringBuilder();
            help.AppendLine("Options are:\n\talign -a [path of alignment map] -f [path of files to align]");
            return help.ToString();
        }
        



    }
}

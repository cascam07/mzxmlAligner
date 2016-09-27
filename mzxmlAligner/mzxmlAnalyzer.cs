using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace mzxmlAligner
{
    class mzxmlAnalyzer
    {

        [STAThread]

        //@"retentionTime=.PT[0-9]*.[0-9]*S."
        public static void GetStats(string rawFile)
        {
            string[] separator = { "," };
            StreamReader mzXML = new StreamReader(rawFile);
            StringBuilder scanSummary = new StringBuilder();
            Regex rtRegex1 = new Regex(@"\d+\.\d+");
            Regex rtRegex2 = new Regex(@"\d+");
            List<Scan> scanList = new List<Scan>();

            int scanCount = 1;
            int scanNum = 0;
            int msLevel = 0;
            double totalIonCurrent = 0.0;
            int peakCount = 0;
            int compressedLen = 0;
            while (true)
            {
                string line = mzXML.ReadLine();
                if (line == null) { break; }
                Match scanMatch = Regex.Match(line, @"scan num=.[0-9]*.");
                Match msLevelMatch = Regex.Match(line, @"msLevel=.[1-2].");
                Match totalIonCurrentMatch = Regex.Match(line, @"totIonCurrent=.*.");
                Match peaksCountMatch = Regex.Match(line, @"peaksCount=.[0-9]*.");
                Match compressedLenMatch = Regex.Match(line, @"compressedLen=.[0-9]*.");

                if (scanMatch.Success)
                {
                    scanNum = Int32.Parse(Regex.Match(line, @"\d+").Value);
                }
                if (msLevelMatch.Success)
                {
                    msLevel = Int32.Parse(Regex.Match(line, @"\d+").Value);
                }
                if (totalIonCurrentMatch.Success)
                {
                    totalIonCurrent = Int32.Parse(Regex.Match(line, @"\d+\.\d+").Value);
                }
                if (peaksCountMatch.Success)
                {
                    peakCount = Int32.Parse(Regex.Match(line, @"\d+").Value);
                }
                if (compressedLenMatch.Success)
                {
                    compressedLen = Int32.Parse(Regex.Match(line, @"\d+").Value);
                    Scan scan = new Scan(scanNum, msLevel,totalIonCurrent, peakCount, compressedLen);
                    scanList.Add(scan);
                }
            }


        }
    }
}

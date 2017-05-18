using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CommandLine;

namespace mzxmlAligner
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {

            string invokedVerb = "Nothing";
            object invokedVerbInstance = null;
            var options = new Options();

            if (args.Length == 0)
            {
                Console.Write(options.GetUsage());
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            if (!Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
            {
                invokedVerb = verb;
                invokedVerbInstance = subOptions;
            })
                )
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            if (invokedVerb == "align")
            {
                ExecuteAligner((AlignOptions) invokedVerbInstance);
            }

            else
            {
                Console.Write(options.GetUsage());
            }
        }


        private static void ExecuteAligner(AlignOptions options)
        {
            try
            {
                string mapPath = options.alignmentMap;
                var alignmentFile = ReadLines(mapPath);
                string dirPath = options.files;
                string[] rawFiles = Directory.GetFiles(dirPath, "*.mzXML");

                Dictionary<string, Tuple<double, double>> alignmentMaps = BuildAlignmentMaps(alignmentFile);
                foreach (var key in alignmentMaps.Keys)
                {
                    AlignFile(dirPath + "\\" + key, alignmentMaps[key].Item1, alignmentMaps[key].Item2);
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }

        private static void ExecuteReplacer(ReplaceOptions options)
        {
            try
            {
                string scansPath = options.scansFiles;
                string dirPath = options.files;
                string[] scansFiles = Directory.GetFiles(scansPath, "*_MS_scans.csv");
                string[] rawFiles = Directory.GetFiles(dirPath, "*.mzXML");

                foreach (var scanfile in scansFiles)
                {
                    string rawname = scanfile.Split(new string[] {"\\"}, StringSplitOptions.None).Last();
                
                    rawname = rawname.Replace("_MS_scans.csv", ".mzXML");
                    var match = from f in rawFiles where f.Contains(rawname) select f;
                    if (match.Any())
                    {
                        var rawFile = match.FirstOrDefault();
                        ReplaceTimes(rawFile, scanfile);
                    }
                    
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }


        private static string[] ReadLines(string path)
        {
            string[] file = File.ReadAllLines(path);
            return file;
        }

        private static Dictionary<string, Tuple<double, double>> BuildAlignmentMaps(string[] alignmentFile)
        {
            Dictionary<string, Tuple<double, double>> alignmentMaps = new Dictionary<string, Tuple<double, double>>();
            Dictionary<string, int> columnHeaders = new Dictionary<string, int>();
            var columnTitles = alignmentFile[0].Split(',');
            var numOfColumns = columnTitles.Length;
            int name = 0;
            int shift = 0;
            int slope = 0;
            int intercept = 0;
            for (int i = 0; i < numOfColumns; i++)
            {
                var header = columnTitles[i].ToLower().Trim();
                switch (header)
                {
                    case "name":
                        name = i;
                        break;
                    case "slope":
                        slope = i;
                        break;
                    case "intercept":
                        intercept = i;
                        break;
                }
            }
            for (int i = 1; i < alignmentFile.Length; i++)
            {
                var currentLine = alignmentFile[i].Split(',');
                var currentSlope = double.Parse(currentLine[slope]);
                var currentIntercept = double.Parse(currentLine[intercept]);
                alignmentMaps.Add(currentLine[name], new Tuple<double, double>(currentSlope, currentIntercept));
            }
            return alignmentMaps;
        }

        private static void AlignFile(string rawFile, double slope, double intercept)
        {
            string[] separator = {","};
            StreamReader mzXML = new StreamReader(rawFile);
            StringBuilder correctedFile = new StringBuilder();
            Regex rtRegex1 = new Regex(@"\d+\.\d+");
            Regex rtRegex2 = new Regex(@"\d+");

            using (StreamWriter AlignedFile = new StreamWriter(rawFile.Replace(".mzXML", "_Aligned.mzXML")))
            {
                int scanCount = 1;
                while (true)
                {
                    string line = mzXML.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    Match match1 = Regex.Match(line, @"retentionTime=.PT[0-9]*.[0-9]*S.");
                    Match match2 = Regex.Match(line, @"retentionTime=.PT[0-9]*S.");
                    if (match1.Success || match2.Success)
                    {
                        string newLine;
                        if (match2.Success)
                        {
                            var oldTime = double.Parse(Regex.Match(line, @"\d+").Value);
                            var correctTime = (((oldTime/60) - intercept)/slope)*60;
                            if (correctTime < 0)
                            {
                                correctTime = 0;
                            }
                            newLine = rtRegex2.Replace(line, correctTime.ToString());
                        }
                        else
                        {
                            var oldTime = double.Parse(Regex.Match(line, @"\d+\.\d+").Value);
                            var correctTime = (((oldTime/60) - intercept)/slope)*60;
                            if (correctTime < 0)
                            {
                                correctTime = 0;
                            }
                            newLine = rtRegex1.Replace(line, correctTime.ToString());
                        }
                        AlignedFile.WriteLine(newLine);
                        scanCount++;
                    }
                    else
                    {
                        AlignedFile.WriteLine(line);
                    }
                }
            }
            Console.WriteLine("Successfully aligned " + rawFile);
        }


        private static void ReplaceTimes(string rawFile, string scanFile)
        {
            char[] delim = new[] {','};
            StringBuilder correctedFile = new StringBuilder();
            var scans = File.ReadLines(scanFile).ToList();
            var splitLine = scans[0].Replace("\"","").Split(delim).ToList();
            var timeIndex = splitLine.IndexOf("corrected_time");
            var scanIndex = splitLine.IndexOf("scan_num");
            /*
            using (XmlReader reader = XmlReader.Create(rawFile))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(rawFile.Replace(".mzXML", "_Aligned.mzXML"), settings))
                {
                    while (reader.Read())
                    {
                        reader.ReadToFollowing("scan");
                        reader.ReadAttributeValue();
                        writer.WriteNode(reader,false);
                        writer.Flush();
                    }
                    writer.Close();
                }
                reader.Close();   
            }*/

            XDocument doc = XDocument.Load(rawFile);

            var nodes = doc.Descendants();
            var scanNodes = from node in nodes where node.Name.LocalName == "scan" select node;
            for (int line = 1; line < scans.Count; line++)
            {
                var linesplit = scans[line].Split(delim);
                var scan = linesplit[scanIndex];
                var newTime = linesplit[timeIndex];
                var desiredScan = scanNodes.FirstOrDefault(element => element.FirstAttribute.Value.Equals(scan));
                //var rt = desiredScan.Attribute("retentionTime").Value;
                //rt = String.Format("PT{0}S", Double.Parse(newTime) * 60)
                desiredScan.Attribute("retentionTime").Value = String.Format("PT{0}S", Double.Parse(newTime) * 60);
            }
            doc.Save(rawFile.Replace(".mzXML", "_Aligned.mzXML"));



            /*
            string[] separator = {","};
            StreamReader mzXML = new StreamReader(rawFile);
            StringBuilder correctedFile = new StringBuilder();
            Regex rtRegex1 = new Regex(@"\d+\.\d+");
            Regex rtRegex2 = new Regex(@"\d+");
            Regex scanNumRegex = new Regex(@"");
             
            using (StreamWriter AlignedFile = new StreamWriter(rawFile.Replace(".mzXML", "_Aligned.mzXML")))
            {
                int scanCount = 1;
                while (true)
                {
                    string line = mzXML.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    Match match1 = Regex.Match(line, @"retentionTime=.PT[0-9]*.[0-9]*S.");
                    Match match2 = Regex.Match(line, @"retentionTime=.PT[0-9]*S.");
                    if (match1.Success || match2.Success)
                    {
                        string newLine;
                        if (match2.Success)
                        {
                            var oldTime = double.Parse(Regex.Match(line, @"\d+").Value);
                            var correctTime = newTime*60;
                            if (correctTime < 0)
                            {
                                correctTime = 0;
                            }
                            newLine = rtRegex2.Replace(line, correctTime.ToString());
                        }
                        else
                        {
                            var oldTime = double.Parse(Regex.Match(line, @"\d+\.\d+").Value);
                            var correctTime = newTime * 60;
                            if (correctTime < 0)
                            {
                                correctTime = 0;
                            }
                            newLine = rtRegex1.Replace(line, correctTime.ToString());
                        }
                        AlignedFile.WriteLine(newLine);
                        scanCount++;
                    }
                    else
                    {
                        AlignedFile.WriteLine(line);
                    }
                }
            }
            Console.WriteLine("Successfully aligned " + rawFile);
        }*/

        }
    }
}

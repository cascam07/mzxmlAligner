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
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string path;
                Console.WriteLine("Select alignment map with headers \"Name\", \"Slope\", \"Intecept\"");
                var alignmentFile = ReadLines(out path);
                Console.WriteLine("Select folder where mzXML files are located.");
                FolderBrowserDialog fbd = new FolderBrowserDialog { SelectedPath = path };
                //FolderBrowserDialog fbd = new FolderBrowserDialog();
                DialogResult result = fbd.ShowDialog();
                var dirPath = fbd.SelectedPath;
                string[] rawFiles = Directory.GetFiles(dirPath, "*.mzXML");
             
                Dictionary<string, Tuple<double, double>> alignmentMaps = BuildAlignmentMaps(alignmentFile);
                foreach (var key in alignmentMaps.Keys)
                {
                    AlignFile(dirPath+"\\"+key, alignmentMaps[key].Item1, alignmentMaps[key].Item2);
                }

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }

        private static string[] ReadLines(out string path)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = "Libraries\\Documents",
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            openFileDialog1.ShowDialog();
            string[] file = File.ReadAllLines(openFileDialog1.FileName);
            path = Path.GetDirectoryName(openFileDialog1.FileName);
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
            string[] separator = { "," };
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
    }
}

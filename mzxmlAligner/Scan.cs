using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxmlAligner
{
    class Scan
    {
        public int ScanNum { get; set; }
        public int MsLevel { get; set; }
        public double TotalIonCurrent { get; set; }
        public int PeakCount { get; set; }
        public int CompressedLen { get; set; }

        public Scan(int scanNum, int msLevel, double totalIonCurrent, int peakCount, int compressedLen)
        {
            this.ScanNum = scanNum;
            this.MsLevel = msLevel;
            this.TotalIonCurrent = totalIonCurrent;
            this.PeakCount = peakCount;
            this.CompressedLen = compressedLen;
        }

    }
}

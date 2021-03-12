using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpuInfoClient.BE
{
    public class HardDiskBE
    {
        string name;
        string driveFormat;
        long totalFreeSpace;
        long totalSize;

        public string Name { get => name; set => name = value; }
        public string DriveFormat { get => driveFormat; set => driveFormat = value; }
        public long TotalFreeSpace { get => totalFreeSpace; set => totalFreeSpace = value; }
        public long TotalSize { get => totalSize; set => totalSize = value; }
    }
}

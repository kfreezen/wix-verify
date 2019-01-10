using System;
using System.Collections.Generic;
using System.Text;

namespace wix_verify.CheckOutput.Models
{
    public class WixFile
    {
        public string Id { get; set; }
        public string DiskId { get; set; }
        public bool Hidden { get; set; }
        public bool ReadOnly { get; set; }
        public bool System { get; set; }
        public bool Compressed { get; set; }

        public string Name { get; set; }
        public string Source { get; set; }
    }
}

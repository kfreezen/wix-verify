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
        public bool Vital { get; set; }

        /// <summary>
        /// Set by <!-- ignore-file:path\to\ignored-file -->
        /// </summary>
        public bool Ignored { get; set; }

        public string Name { get; set; }
        public string Source { get; set; }

        /// <summary>
        /// The file path that this file element was loaded from.
        /// </summary>
        public string WxsFilePath { get; set; }
    }
}

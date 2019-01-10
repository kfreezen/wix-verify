using System;
using System.Collections.Generic;
using System.Text;

namespace wix_verify.CheckOutput.Models
{
    public class WixComponent
    {
        public WixComponent()
        {
            Elements = new List<WixElement>();
        }

        public List<WixElement> Elements { get; set; }
    }
}

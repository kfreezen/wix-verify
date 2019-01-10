using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace wix_verify.Subcommands
{
    public static class CheckOutput
    {
        public static int Run(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Usage: wix-verify check-output <application-output> <wxs-file-1> ... <wxs-file-n>");
                return 1;
            }

            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace wix_verify.Subcommands
{
    static class Help
    {
        public static int Run(Span<string> args)
        {
            Console.WriteLine("Subcommands:");
            Console.WriteLine("\tcheck-output");

            return 0;
        }
    }
}

using System;
using wix_verify.Subcommands;

namespace wix_verify
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                PrintUsage();
                return;
            }
            else
            {
                string subcommand = args[0];
                Span<string> sub_args = args.AsSpan(1);

                switch(args[0])
                {
                    case "help":
                        Environment.Exit(Help.Run(sub_args));
                        break;

                    case "check-output":
                        Environment.Exit(Subcommands.CheckOutput.Run(sub_args));
                        break;
                }
            }

            Console.WriteLine("Hello World!");
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: wix-verify [subcommand] [arguments...]");
            Console.WriteLine("\tView help by running `wix-verify help'");
        }
    }
}

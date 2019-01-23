using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using wix_verify.CheckOutput.Models;
using wix_verify.CheckOutput;
using System.Linq;

namespace wix_verify.Subcommands
{
    public static class CheckOutput
    {
        private class OutputFileWithTags
        {
            public string Filename { get; set; }

            public WixFile LocatedWixFile { get; set; }
        }

        public static int Run(Span<string> args)
        {
            int returnVal = 0;

            if(args.Length < 2)
            {
                Console.WriteLine("Usage: wix-verify check-output <application-output> <wxs-file-1> ... <wxs-file-n>");
                return 1;
            }

            string applicationOutput = args[0];
            Span<string> wxsFiles = args.Slice(1);

            if (!Directory.Exists(applicationOutput))
            {
                Console.WriteLine("Please specify a valid application output directory.");
                return 1;
            }

            List<WixFile> wxsFileElements = new List<WixFile>();
            var listReader = new FileListReader();

            foreach(var filename in wxsFiles)
            {
                try
                {
                    string fullWxs = Path.GetFullPath(filename);

                    List<WixFile> files = listReader.GetFiles(fullWxs);
                    if (files != null) wxsFileElements.AddRange(files);
                }
                catch(Exception ex)
                {
                    using (var stderr = new StreamWriter(Console.OpenStandardError()))
                    {
                        stderr.WriteLine("Error occurred while reading files from XML");
                        stderr.WriteLine(ex);
                    }
                }
            }

            // Now that we've obtained all of our file information from the wix modules, get all of our file names from the application output.

            // A couple of rules
            // 1. Relative paths should resolve from WXS file root path.
            // 2. 

            var applicationFiles = Directory.EnumerateFileSystemEntries(applicationOutput, "*", new EnumerationOptions() { RecurseSubdirectories = true })
                .Select(s => new OutputFileWithTags() { Filename = Path.GetFullPath(s) });

            foreach(var wixFile in wxsFileElements)
            {
                string wxsDirectory = Path.GetDirectoryName(wixFile.WxsFilePath);

                // We want to make sure Source filename and Name match.
                string relativeSource = wixFile.Source;
                string absoluteSource = Path.GetFullPath(relativeSource, wxsDirectory);

                if(!wixFile.Ignored && Path.GetFileName(absoluteSource).ToLowerInvariant() != wixFile.Name.ToLowerInvariant())
                {
                    Console.WriteLine("ERROR: Source Filename != Name. Source = '{0}', Name = '{1}'", relativeSource, wixFile.Name);
                    returnVal = 1;
                }

                // We want to make sure that Source exists in application output.
                if(!wixFile.Ignored && !absoluteSource.IsSubPathOf(applicationOutput))
                {
                    Console.WriteLine("WARN: File source '{0}' outside of specified application output folder. Was this intended?", wixFile.Source);
                }
                else
                {
                    OutputFileWithTags outputFile = findOutputFile(applicationFiles, absoluteSource);

                    if (!wixFile.Ignored && outputFile == null)
                    {
                        Console.WriteLine("ERROR: File specified in WXS does not have corresponding file in application output. Source='{0}'", wixFile.Source);
                        returnVal = 1;
                    }
                    else
                    {
                        outputFile.LocatedWixFile = wixFile;
                    }
                }
            }

            foreach(var outputFile in applicationFiles)
            {
                if(outputFile.LocatedWixFile == null)
                {
                    Console.WriteLine("ERROR: File '{0}' in application output does not have corresponding WiX file entry. If this is intentional, please add an ignore-file comment.", outputFile.Filename);
                    returnVal = 1;
                }
            }

            return returnVal;
        }

        private static OutputFileWithTags findOutputFile(IEnumerable<OutputFileWithTags> files, string absoluteSource)
        {
            absoluteSource = absoluteSource.ToLowerInvariant();

            return files.FirstOrDefault(f => f.Filename.ToLowerInvariant() == absoluteSource);
        }
    }
}

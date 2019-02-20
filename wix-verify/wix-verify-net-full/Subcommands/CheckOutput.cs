using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using wix_verify.CheckOutput.Models;
using wix_verify.CheckOutput;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
            List<string> ignores = new List<string>();

            var listReader = new FileListReader();

            foreach(var filename in wxsFiles)
            {
                try
                {
                    string fullWxs = Path.GetFullPath(filename);

                    List<WixFile> files = listReader.GetFiles(fullWxs, ignores);
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

            var applicationFiles = Directory.EnumerateFileSystemEntries(applicationOutput, "*", SearchOption.AllDirectories)
                .Where(s => !new FileInfo(s).Attributes.HasFlag(FileAttributes.Directory))
                .Select(s => new OutputFileWithTags() { Filename = Path.GetFullPath(s) })
                .ToList();

            List<Regex> regexIgnores = ignores.Select(s => new Regex(Regex.Escape(s.ToLowerInvariant()).Replace(@"\*", ".*").Replace(@"\?", "."))).ToList();

            foreach (var wixFile in wxsFileElements)
            {
                string wxsDirectory = Path.GetDirectoryName(wixFile.WxsFilePath);

                // We want to make sure Source filename and Name match.
                string relativeSource = wixFile.Source;
                if(relativeSource == null || wxsDirectory == null)
                {
                    System.Diagnostics.Debugger.Break();
                }

                string absoluteSource = Path.GetFullPath(Path.Combine(wxsDirectory, relativeSource));

                if(Path.GetFileName(absoluteSource).ToLowerInvariant() != wixFile.Name.ToLowerInvariant())
                {
                    Console.WriteLine("ERROR: Source Filename != Name. Source = '{0}', Name = '{1}'", relativeSource, wixFile.Name);
                    returnVal = 1;
                }

                // We want to make sure that Source exists in application output.
                if(!absoluteSource.IsSubPathOf(applicationOutput))
                {
                    Console.WriteLine("WARN: File source '{0}' outside of specified application output folder. Was this intended?", wixFile.Source);
                }
                else
                {
                    OutputFileWithTags outputFile = findOutputFile(applicationFiles, absoluteSource);

                    if (outputFile == null)
                    {
                        Console.WriteLine("ERROR: File specified in {0} does not have corresponding file in application output. Source='{1}'", Path.GetFileName(wixFile.WxsFilePath), wixFile.Source);
                        returnVal = 1;
                    }
                    else if(wixFile == null)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        outputFile.LocatedWixFile = wixFile;
                    }
                }
            }

            foreach(var outputFile in applicationFiles)
            {
                bool ignored = matchIgnore(regexIgnores, outputFile.Filename);
                if(outputFile.LocatedWixFile == null && !ignored)
                {
                    Console.WriteLine("ERROR: File '{0}' in application output does not have corresponding WiX file entry. If this is intentional, please add an ignore-file comment.", outputFile.Filename);
                    returnVal = 1;
                }
            }

            return returnVal;
        }

        private static bool matchFiles(string name, string absoluteSource)
        {
            return name.ToLowerInvariant() == absoluteSource;
        }

        private static bool matchIgnore(List<Regex> ignores, string absoluteSource)
        {
            absoluteSource = absoluteSource.ToLowerInvariant();
            return ignores.Any(i => i.IsMatch(absoluteSource));
        }

        private static OutputFileWithTags findOutputFile(IEnumerable<OutputFileWithTags> files, string absoluteSource)
        {
            absoluteSource = absoluteSource.ToLowerInvariant();

            return files.FirstOrDefault(f => f.Filename.ToLowerInvariant() == absoluteSource);
        }
    }
}

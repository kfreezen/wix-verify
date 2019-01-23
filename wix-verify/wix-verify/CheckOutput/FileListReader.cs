using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using wix_verify.CheckOutput.Models;
using System.Xml;

namespace wix_verify.CheckOutput
{
    public class FileListReader
    {
        public List<WixFile> GetFiles(string wxsFile)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings()
            {
                IgnoreComments = false
            };

            List<WixFile> files = new List<WixFile>();

            using(FileStream stream = new FileStream(wxsFile, FileMode.Open))
            using (XmlReader reader = XmlReader.Create(stream, readerSettings))
            {
                reader.MoveToContent();
                while(reader.Read())
                {
                    switch(reader.NodeType)
                    {
                        case XmlNodeType.Comment:
                            {
                                string commentString = reader.ReadContentAsString();

                                string[] commentParts = commentString.Trim().ToLowerInvariant().Split(new char[] { ':' }, 2);
                                if (commentParts.Length > 1 && commentParts[0].Trim() == "ignore-file")
                                {
                                    files.Add(new WixFile()
                                    {
                                        Source = commentParts[1],
                                        Ignored = true
                                    });
                                }
                            }
                            break;
                    }

                    if(reader.IsStartElement())
                    {
                        if (reader.Name.ToLowerInvariant() == "file")
                        {
                            var file = ReadFile(reader);
                            file.WxsFilePath = wxsFile;

                            if (file != null) files.Add(file);
                        }
                    }
                }
            }

            return files;
        }

        private WixFile ReadFile(XmlReader reader)
        {
            if(reader.Name.ToLowerInvariant() != "file")
            {
                return null;
            }

            if(!reader.HasAttributes)
            {
                return null;
            }

            WixFile file = new WixFile()
            {
                Id = reader.GetAttribute("Id"),
                DiskId = reader.GetAttribute("DiskId"),
                Hidden = parseBoolean(reader.GetAttribute("Hidden")),
                ReadOnly = parseBoolean(reader.GetAttribute("ReadOnly")),
                System = parseBoolean(reader.GetAttribute("System")),
                Vital = parseBoolean(reader.GetAttribute("Vital")),
                Compressed = parseBoolean(reader.GetAttribute("Compressed")),
                Name = reader.GetAttribute("Name"),
                Source = reader.GetAttribute("Source")
            };

            return file;
        }

        private bool parseBoolean(string boolean)
        {
            if(boolean == null)
            {
                return false;
            }

            switch(boolean.ToLowerInvariant())
            {
                case "no":
                case "false":
                    return false;

                case "yes":
                case "true":
                    return true;

                default:
                    return false;
            }
        }
    }
}

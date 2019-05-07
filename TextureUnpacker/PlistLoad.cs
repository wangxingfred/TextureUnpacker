using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;

namespace TextureUnpacker
{
	public class PlistFile
	{
		public PlistMetadata metadata = new PlistMetadata();
		public List<PlistFrame> frames = new List<PlistFrame>();
	}

	public struct PlistMetadata
	{
		public int format;
		public String realTextureFileName;
		public Size size;
		public String smartupdate;
		public String textureFileName;
	}

	public struct PlistFrame
	{
		public String name;
		public Rectangle frame;
		public Point offset;
		public bool rotated;
		public Rectangle sourceColorRect;
		public Size sourceSize;
	}


	class PlistLoad
	{
		public PlistFile plistFile = new PlistFile();

		public PlistLoad() { }
		public PlistLoad(String filepath) { load(filepath); }

		public void load(String path)
		{
			System.IO.StreamReader file = new System.IO.StreamReader(path);

			// Create an XmlReaderSettings object.  
			XmlReaderSettings settings = new XmlReaderSettings();

			// Set XmlResolver to null, and ProhibitDtd to false. 
			settings.XmlResolver = null;
			settings.DtdProcessing = DtdProcessing.Ignore;

			// Now, create an XmlReader.  This is a forward-only text-reader based
			// reader of Xml.  Passing in the settings will ensure that validation
			// is not performed. 
			XmlReader reader = XmlTextReader.Create(file, settings);

			// Create your document, and load the reader. 
			XmlDocument doc = new XmlDocument();
			doc.Load(reader);

			XmlNode root = doc.DocumentElement.FirstChild;

            PlistDictionary dict = new PlistDictionary(root);

			plistFile = dict_to_plist(dict);

			file.Close();
		}

		private PlistFile dict_to_plist(Dictionary<string, XmlNode> dict)
		{
			PlistFile plistFile = new PlistFile();

			//metadata
			plistFile.metadata.format = 0;
			if(dict.ContainsKey("metadata"))
			{
                PlistDictionary metadata = new PlistDictionary(dict["metadata"]);
                plistFile.metadata.format = metadata.GetInt("format");
				plistFile.metadata.realTextureFileName = metadata.GetString("realTextureFileName");
				plistFile.metadata.smartupdate = metadata.GetString("smartupdate");
				plistFile.metadata.textureFileName = metadata.GetString("textureFileName");

				plistFile.metadata.size = metadata.GetSize("size");
			}

			//frames
            PlistDictionary frames = new PlistDictionary(dict["frames"]);
            int format = plistFile.metadata.format;
			foreach (KeyValuePair<string, XmlNode> node in frames)
			{
                PlistDictionary d = new PlistDictionary(node.Value);
                PlistFrame frame = new PlistFrame
                {
                    name = node.Key
                };

                if (format==0)
				{
					frame.frame = d.ToRectangle();
                    frame.offset = d.ToPoint("offsetX", "offsetY");

                    frame.sourceSize = d.ToSize("originalWidth", "originalWidth");
                    frame.sourceSize.Width = Math.Abs(frame.sourceSize.Width);
                    frame.sourceSize.Height = Math.Abs(frame.sourceSize.Height);

					frame.rotated = false;
				}
				else if (format == 1 || format == 2)
				{
					frame.rotated = format == 2 ? d.GetBool("rotated") : false;

                    frame.frame = d.GetRectangle("frame");

					frame.offset = d.GetPoint("offset");

                    frame.sourceSize = d.GetSize("sourceSize");
				}
				else if (format == 3)
				{
					frame.rotated = d.GetBool("textureRotated");

                    frame.sourceSize = d.GetSize("spriteSourceSize");

                    frame.offset = d.GetPoint("spriteOffset");

                    Size s = d.GetSize("spriteSize");
                    Rectangle b = d.GetRectangle("textureRect");
                    frame.frame = new Rectangle(b.X, b.Y, s.Width, s.Height);

				}
				plistFile.frames.Add(frame);
			}
			return plistFile;
		}
	}
}













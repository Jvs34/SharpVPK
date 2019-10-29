using System.IO;

namespace SharpVPK
{
	internal class ArchivePart
	{
		public uint Size { get; set; }
		public int Index { get; set; }
		public string Filename { get; set; }
		public Stream PartStream { get; set; }

		public ArchivePart( uint size , int index , string filename , Stream filestream )
		{
			Size = size;
			Index = index;
			Filename = filename;
			PartStream = filestream;
		}

		public ArchivePart( int index , string filename , Stream filestream )
		{
			Index = index;
			Filename = filename;
			PartStream = filestream;
		}
	}
}

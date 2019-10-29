using System.IO;
using System.Linq;

namespace SharpVPK
{
	public class VpkEntry
	{
		public string Extension { get; set; }
		public string Path { get; set; }
		public string Filename { get; set; }
		public bool HasPreloadData { get; set; }
		public uint Length => EntryLength;

		internal uint CRC;
		internal ushort PreloadBytes;
		internal uint PreloadDataOffset;
		internal ushort ArchiveIndex;
		internal uint EntryOffset;
		internal uint EntryLength;
		internal VpkArchive ParentArchive;

		internal VpkEntry( VpkArchive parentArchive , uint crc , ushort preloadBytes , uint preloadDataOffset , ushort archiveIndex , uint entryOffset ,
			uint entryLength , string extension , string path , string filename )
		{
			ParentArchive = parentArchive;
			CRC = crc;
			PreloadBytes = preloadBytes;
			PreloadDataOffset = preloadDataOffset;
			ArchiveIndex = archiveIndex;
			EntryOffset = entryOffset;
			EntryLength = entryLength;
			Extension = extension;
			Path = path;
			Filename = filename;
			HasPreloadData = preloadBytes > 0;
		}

		public override string ToString()
		{
			return string.Concat( Path , "/" , Filename , "." , Extension );
		}

		public byte [] ReadPreloadData()
		{
			if( HasPreloadData )
			{
				var buff = new byte [PreloadBytes];
				var fs = ParentArchive.Parts [-1].PartStream;
				fs.Seek( PreloadDataOffset , SeekOrigin.Begin );
				fs.Read( buff , 0 , buff.Length );
				return buff;
			}
			return null;
		}

		public Stream ReadPreloadDataStream()
		{
			Stream memStream = null;
			if( HasPreloadData )
			{
				var fs = ParentArchive.Parts [-1].PartStream;
				memStream = new MemoryStream();
				fs.Seek( PreloadDataOffset , SeekOrigin.Begin );
				fs.CopyTo( memStream , PreloadBytes );

			}
			return memStream;
		}

		public byte [] ReadData()
		{
			var partFile = ParentArchive.Parts [ArchiveIndex];
			if( partFile != null && !HasPreloadData )
			{
				var buff = new byte [EntryLength];
				var fs = partFile.PartStream;
				fs.Seek( EntryOffset , SeekOrigin.Begin );
				fs.Read( buff , 0 , buff.Length );
				return buff;
			}
			return null;
		}

		public Stream ReadDataStream()
		{
			Stream memStream = null;

			var partFile = ParentArchive.Parts [ArchiveIndex];
			if( partFile != null && !HasPreloadData )
			{
				var fs = partFile.PartStream;
				memStream = new MemoryStream();
				fs.Seek( EntryOffset , SeekOrigin.Begin );
				fs.CopyTo( memStream , (int) EntryLength );
			}

			return memStream;
		}

		public byte [] ReadAnyData() => HasPreloadData ? ReadPreloadData() : ReadData();

		public Stream ReadAnyDataStream()
		{
			if( HasPreloadData )
			{
				return ReadPreloadDataStream();
			}
			else
			{
				return ReadDataStream();
			}
		}

	}
}

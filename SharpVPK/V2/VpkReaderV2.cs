﻿using System.IO;
using System.Runtime.InteropServices;

namespace SharpVPK.V2
{
	internal class VpkReaderV2 : VpkReaderBase
	{
		public VpkReaderV2( string filename )
			: base( filename )
		{
		}

		public VpkReaderV2( byte[] file )
			: base( file )
		{
		}

		public VpkReaderV2( Stream stream )
			: base( stream )
		{

		}

		public override IVpkArchiveHeader ReadArchiveHeader()
		{
			var hdrStructSize = Marshal.SizeOf( typeof( VpkArchiveHeaderV2 ) );
			var hdrBuff = Reader.ReadBytes( hdrStructSize );
			// skip unknown values
			Reader.ReadInt32();
			var hdr = BytesToStructure<VpkArchiveHeaderV2>( hdrBuff );
			hdr.FooterLength = ( uint ) Reader.ReadInt32();
			Reader.ReadInt32();
			return hdr;
		}

		public override uint CalculateEntryOffset( uint offset )
		{
			throw new System.NotImplementedException();
		}
	}
}

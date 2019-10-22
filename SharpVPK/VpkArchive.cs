using SharpVPK.Exceptions;
using SharpVPK.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpVPK
{
	public class VpkArchive : IDisposable
	{
		public List<VpkDirectory> Directories { get; set; }
		public bool IsMultiPart { get; set; }
		private VpkReaderBase _reader;
		internal List<ArchivePart> Parts { get; set; }
		internal string ArchivePath { get; set; }

		private bool _disposedValue; // To detect redundant calls


		public VpkArchive()
		{
			Directories = new List<VpkDirectory>();
		}

		public void Load( string filename , VpkVersions.Versions version = VpkVersions.Versions.V1 )
		{
			ArchivePath = filename;
			IsMultiPart = filename.EndsWith( "_dir.vpk" );
			if( IsMultiPart )
			{
				LoadParts( filename );
			}

			if( version == VpkVersions.Versions.V1 )
			{
				_reader = new VpkReaderV1( filename );
			}
			else if( version == VpkVersions.Versions.V2 )
			{
				_reader = new V2.VpkReaderV2( filename );
			}

			var hdr = _reader.ReadArchiveHeader();
			if( !hdr.Verify() )
			{
				throw new ArchiveParsingException( "Invalid archive header" );
			}

			Directories.AddRange( _reader.ReadDirectories( this ) );
		}

		public void Load( byte [] bytes , VpkVersions.Versions version = VpkVersions.Versions.V1 )
		{
			if( version == VpkVersions.Versions.V1 )
			{
				_reader = new VpkReaderV1( bytes );
			}
			else if( version == VpkVersions.Versions.V2 )
			{
				_reader = new V2.VpkReaderV2( bytes );
			}

			var hdr = _reader.ReadArchiveHeader();
			if( !hdr.Verify() )
			{
				throw new ArchiveParsingException( "Invalid archive header" );
			}

			Directories.AddRange( _reader.ReadDirectories( this ) );
		}

		public void Load( Stream stream , VpkVersions.Versions version = VpkVersions.Versions.V1 )
		{
			if( version == VpkVersions.Versions.V1 )
			{
				_reader = new VpkReaderV1( stream );
			}
			else if( version == VpkVersions.Versions.V2 )
			{
				_reader = new V2.VpkReaderV2( stream );
			}

			var hdr = _reader.ReadArchiveHeader();
			if( !hdr.Verify() )
			{
				throw new ArchiveParsingException( "Invalid archive header" );
			}

			Directories.AddRange( _reader.ReadDirectories( this ) );
		}

		private void LoadParts( string filename )
		{
			Parts = new List<ArchivePart>();
			var fileBaseName = filename.Split( '_' ) [0];
			foreach( var file in Directory.GetFiles( Path.GetDirectoryName( filename ) ) )
			{
				if( file.Split( '_' ) [0] != fileBaseName || file == filename )
				{
					continue;
				}

				var fi = new FileInfo( file );
				string [] spl = file.Split( '_' );
				var partIdx = int.Parse( spl [spl.Length - 1].Split( '.' ) [0] );
				Parts.Add( new ArchivePart( (uint) fi.Length , partIdx , file ) );
			}
			Parts.Add( new ArchivePart( (uint) new FileInfo( filename ).Length , -1 , filename ) );
			Parts = Parts.OrderBy( p => p.Index ).ToList();
		}

		#region IDisposable Support

		protected virtual void Dispose( bool disposing )
		{
			if( !_disposedValue )
			{
				if( disposing )
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~VpkArchive()
		// {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose( true );
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}

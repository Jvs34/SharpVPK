using SharpVPK.Exceptions;
using SharpVPK.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpVPK
{
	public sealed class VpkArchive : IDisposable
	{
		public List<VpkDirectory> Directories { get; internal set; } = new List<VpkDirectory>();
		public bool IsMultiPart => Parts.Count > 1;
		private VpkReaderBase _reader;
		internal Dictionary<int , ArchivePart> Parts { get; set; } = new Dictionary<int , ArchivePart>();
		private bool _disposedValue; // To detect redundant calls

		public const int MainPartIndex = -1;

		public void Load( string filename , VpkVersions.Versions version = VpkVersions.Versions.V1 )
		{
			Parts.Clear();

			Load( new FileStream( filename , FileMode.Open , FileAccess.Read ) , filename , version , LoadFileParts( filename ) );
		}

		public void Load( byte[] bytes , string filename = "" , VpkVersions.Versions version = VpkVersions.Versions.V1 , List<byte[]> byteParts = null )
		{
			//create a new memorystream for each bytes archive
			Dictionary<Stream , string> streamParts = null;

			if( byteParts?.Count > 0 )
			{
				streamParts = new Dictionary<Stream , string>();

				int index = 0;
				foreach( byte[] archivePart in byteParts )
				{
					streamParts.Add( new MemoryStream( archivePart ) , $"stream_{index}.vpk" );
					index++;
				}
			}

			Load( new MemoryStream( bytes ) , filename , version , streamParts );
		}

		public void Load( Stream stream , string filename = "" , VpkVersions.Versions version = VpkVersions.Versions.V1 , Dictionary<Stream , string> parts = null )
		{
			Directories.Clear();
			Parts.Clear();

			if( string.IsNullOrEmpty( filename ) )
			{
				filename = "stream_dir.vpk";
			}

			AddMainPart( filename , stream );

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

			//Jvs: moved this down here as we want the header error as soon as possible
			if( parts?.Count > 0 )
			{
				LoadParts( parts );
			}

			Directories.AddRange( _reader.ReadDirectories( this ) );
		}

		private Dictionary<Stream , string> LoadFileParts( string filename )
		{
			Dictionary<Stream , string> streamParts = new Dictionary<Stream , string>();

			var fileBaseName = filename.Split( '_' )[0];
			foreach( var file in Directory.GetFiles( Path.GetDirectoryName( filename ) ) )
			{
				if( file.Split( '_' )[0] != fileBaseName || file == filename )
				{
					continue;
				}

				streamParts.Add( new FileStream( file , FileMode.Open , FileAccess.Read ) , file );
			}

			return streamParts;
		}

		private void LoadParts( Dictionary<Stream , string> streamParts )
		{
			foreach( var kv in streamParts )
			{
				string[] spl = kv.Value.Split( '_' );
				var partIdx = int.Parse( spl[spl.Length - 1].Split( '.' )[0] );
				AddPart( kv.Value , kv.Key , partIdx );
			}
		}

		private void AddMainPart( string filename , Stream stream = null )
		{
			if( stream is null )
			{
				FileInfo info = new FileInfo( filename );
				stream = info.Open( FileMode.Open , FileAccess.Read );
			}
			AddPart( filename , stream , MainPartIndex );
		}

		private void AddPart( string filename , Stream stream , int index )
		{
			Parts.Add( index , new ArchivePart( index , filename , stream ) );
		}

		#region IDisposable Support

		private void Dispose( bool disposing )
		{
			if( !_disposedValue )
			{
				if( disposing )
				{
					foreach( var partkv in Parts )
					{
						partkv.Value.PartStream?.Dispose();
					}
					Parts.Clear();
				}
				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

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

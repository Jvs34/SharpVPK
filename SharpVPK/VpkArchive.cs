using SharpVPK.Exceptions;
using SharpVPK.V1;
using SharpVPK.V2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpVPK
{
	public sealed class VpkArchive : IDisposable
	{
		public bool Loaded { get; private set; }
		public bool IsMultiPart => Parts.Count > 1;
		public IReadOnlyList<VpkDirectory> Directories => InternalDirectories.AsReadOnly();
		private VpkReaderBase Reader { get; set; }
		private bool Disposed { get; set; } // To detect redundant calls
		internal List<VpkDirectory> InternalDirectories { get; } = new List<VpkDirectory>();
		internal Dictionary<int , ArchivePart> Parts { get; } = new Dictionary<int , ArchivePart>();
		internal ArchivePart MainPart => Parts[MainPartIndex];

		internal const int MainPartIndex = -1;

		/// <summary>
		/// Loads the specified vpk archive by filename, if it's a _dir.vpk file it'll load related numbered vpks automatically
		/// </summary>
		/// <param name="filename">A vpk archive ending in _dir.vpk</param>
		/// <param name="version"></param>
		public void Load( string filename , VpkVersions.Versions version = VpkVersions.Versions.V1 )
		{
			Load( new FileStream( filename , FileMode.Open , FileAccess.Read ) , filename , version , LoadFileParts( filename ) );
		}

		/// <summary>
		/// Loads a vpk archive by stream, the related parts need to be ordered correctly in the list
		/// </summary>
		/// <param name="mainPartStream"></param>
		/// <param name="parts"></param>
		/// <param name="filename"></param>
		/// <param name="version"></param>
		public void Load( Stream mainPartStream , List<Stream> parts , string filename = "" , VpkVersions.Versions version = VpkVersions.Versions.V1 )
		{
			Dictionary<Stream , string> streamParts = null;

			if( parts?.Count > 0 )
			{
				streamParts = new Dictionary<Stream , string>();

				int index = 0;
				foreach( var streamPart in parts )
				{
					streamParts.Add( streamPart , $"stream_{index}.vpk" );
					index++;
				}
			}

			Load( mainPartStream , filename , version , streamParts );
		}

		/// <summary>
		/// The main Load function, the related parts need to be numbered correctly as "archivename_01.vpk" and so forth
		/// </summary>
		/// <param name="mainPartStream"></param>
		/// <param name="filename"></param>
		/// <param name="version"></param>
		/// <param name="parts"></param>
		public void Load( Stream mainPartStream , string filename = "" , VpkVersions.Versions version = VpkVersions.Versions.V1 , Dictionary<Stream , string> parts = null )
		{
			if( Loaded )
			{
				throw new NotSupportedException( "Tried to call Load on a VpkArchive that is already loaded, dispose and create a new one instead" );
			}

			if( string.IsNullOrEmpty( filename ) )
			{
				filename = "stream_dir.vpk";
			}

			if( version == VpkVersions.Versions.V1 )
			{
				Reader = new VpkReaderV1( mainPartStream );
			}
			else if( version == VpkVersions.Versions.V2 )
			{
				Reader = new VpkReaderV2( mainPartStream );
			}

			var hdr = Reader.ReadArchiveHeader();
			if( !hdr.Verify() )
			{
				throw new ArchiveParsingException( "Invalid archive header" );
			}

			//Jvs: moved these down here as we want the header error as soon as possible
			AddMainPart( filename , mainPartStream );

			if( parts?.Count > 0 )
			{
				LoadParts( parts );
			}

			InternalDirectories.AddRange( Reader.ReadDirectories( this ) );

			Loaded = true;
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
			if( !Disposed )
			{
				if( disposing )
				{
					foreach( var partkv in Parts )
					{
						partkv.Value.PartStream?.Dispose();
					}
					Parts.Clear();
					InternalDirectories.Clear();
				}
				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				Disposed = true;
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

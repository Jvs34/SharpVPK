using System;
using System.IO;
using System.Linq;

namespace SharpVPK.Test
{
	class Program
	{
		static void Main( string[] args )
		{

			using( var testVpkArchive = new VpkArchive() )
			{
				testVpkArchive.Load( @"E:\Games\Steam\steamapps\common\dota 2 beta\game\dota\pak01_dir.vpk" , VpkVersions.Versions.V2 );

				foreach( var dir in testVpkArchive.Directories )
				{
					foreach( var entry in dir.Entries )
					{
						//Console.WriteLine( Path.Combine( entry.Path , Path.ChangeExtension( entry.Filename , entry.Extension ) ) );
						//var stream = entry.ReadAnyDataStream();
					}
				}

				//try to find items_game.txt


				var itemsEntry = testVpkArchive.Directories
					.Where( x => x.Path.StartsWith( "scripts" ) && x.Entries.Find( y => y.Filename.Contains( "items_game" ) ) != null )
					.Select( x => x.Entries.Find( y => y.Filename.Contains( "items_game" ) ) ).First();

				using( var stream = itemsEntry.ReadAnyDataStream() )
				using( var fileStream = File.OpenWrite( itemsEntry.Filename + "." + itemsEntry.Extension ) )
				{
					stream.Position = 0;
					stream.CopyTo( fileStream );
				}
			}



			Console.WriteLine( "Done" );
			Console.ReadLine();
		}
	}
}

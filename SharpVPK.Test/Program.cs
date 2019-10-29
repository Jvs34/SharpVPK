using System;
using System.IO;

namespace SharpVPK.Test
{
	class Program
	{
		static void Main( string [] args )
		{

			using( var testVpkArchive = new VpkArchive() )

			/*using( var fileStream = File.OpenRead( @"E:\Games\Steam\steamapps\common\dota 2 beta\game\dota\pak01_dir.vpk" ) )
			{
			testVpkArchive.Load( fileStream , fileStream.Name , VpkVersions.Versions.V2 );
			*/

			{
				testVpkArchive.Load( @"E:\Games\Steam\steamapps\common\dota 2 beta\game\dota\pak01_dir.vpk" , VpkVersions.Versions.V2 );

				foreach( var dir in testVpkArchive.Directories )
				{
					foreach( var entry in dir.Entries )
					{
						Console.WriteLine( Path.Combine( entry.Path , Path.ChangeExtension( entry.Filename , entry.Extension ) ) );
						var stream = entry.ReadAnyDataStream();
					}
				}

				var scriptFolder = testVpkArchive.Directories.Find( x => x.Path == "scripts/" );
			}



			Console.WriteLine( "Done" );
			Console.ReadLine();
		}
	}
}

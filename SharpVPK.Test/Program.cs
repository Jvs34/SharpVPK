using System;
using System.IO;

namespace SharpVPK.Test
{
	class Program
	{
		static void Main( string [] args )
		{
			var archive = new VpkArchive();

			using( var fileStream = File.OpenRead( @"E:\Games\Steam\steamapps\common\dota 2 beta\game\dota\pak01_dir.vpk" ) )
			{
				archive.Load( fileStream , VpkVersions.Versions.V2 );
			}


			//foreach(var dir in archive.Directories)
			//    foreach(var entry in dir.Entries)
			//        if (entry.HasPreloadData)
			//        {

			//        }
			Console.WriteLine( "Done" );
			Console.ReadLine();
		}
	}
}

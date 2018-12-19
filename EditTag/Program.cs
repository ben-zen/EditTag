using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EditTag
{
    class Program
    {
        static void Main(string[] args)
        {
            // Confirm that the first argument (only?) is a folder path.
            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("Enter a directory that exists!");
                return;
            }
            // Loop over files in a supplied directory.

            // Check for argument someday...
            EditAlbumTags(args[0]);

            foreach (var file in Directory.GetFiles(args[0]))
            {
                EditTrackTags(file);
            }

            // Rename files to match their tags.
            foreach (var file in Directory.GetFiles(args[0]))
            {
                RenameFile(file);
            }
        }

        static bool GetYesNoResponse()
        {
            switch (Console.ReadLine().ToLowerInvariant())
            {
                case "":
                case "y":
                    return true;
                case "n":
                default:
                    return false;
            }
        }

        static void EditAlbumTags(string folderPath)
        {
            string albumArtist = null;
            Console.WriteLine("Set an album artist? (Y/n)");
            if (GetYesNoResponse())
            {
                albumArtist = Console.ReadLine();
            }

            Console.WriteLine("Set album title:");
            var albumTitle = Console.ReadLine();

            Console.WriteLine("Set album genre:");
            var albumGenre = Console.ReadLine();

            Console.WriteLine("Set album year:");

            var albumYear = Console.ReadLine();

            foreach (var file in Directory.GetFiles(folderPath))
            {
                var fileTags = TagLib.File.Create(file);
                if (albumArtist != null)
                {
                    fileTags.Tag.AlbumArtists = new string[] { albumArtist };
                }

                fileTags.Tag.Album = albumTitle;
                fileTags.Tag.Genres = new string[] { albumGenre };
                fileTags.Tag.Year = uint.Parse(albumYear);
                fileTags.Save();
            }
        }

        static void EditTrackTags(string filePath)
        {
            var fileTags = TagLib.File.Create(filePath);

            if (fileTags.Tag.Performers.Length > 0)
            {
                Console.WriteLine($"Current track artist{(fileTags.Tag.Performers.Length > 1 ? "s" : "")}: "
                    + string.Join(", ", fileTags.Tag.Performers) + ". Replace artists? (Y/n)");
            }
            else
            {
                Console.WriteLine("Set track artist(s)? (Y/n)");
            }

            if (GetYesNoResponse())
            {
                List<string> artists = new List<string>();
                var inputArtistName = "";
                do
                {
                    Console.WriteLine("Enter artist name then press enter, or just hit return to stop adding artists.");
                    inputArtistName = Console.ReadLine();
                    if (inputArtistName.Length > 0)
                    {
                        artists.Add(inputArtistName);
                    }
                } while (inputArtistName.Length > 0);

                if (artists.Count > 0)
                {
                    fileTags.Tag.Performers = artists.ToArray();
                }
            }

            Console.WriteLine($"Current track title: {fileTags.Tag.Title}. Set a new title? (Y/n)");
            if (GetYesNoResponse())
            {
                Console.WriteLine("Enter a new track title, then press enter.");
                fileTags.Tag.Title = Console.ReadLine();
            }

            fileTags.Save();
        }

        static void RenameFile(string file)
        {
            var fileInfo = new FileInfo(file);
            // Build a new file name; eventually this can include a format string, but for now
            // it's a "get what you get" situation, oriented towards Plex's layout.

            var fileTags = TagLib.File.Create(file);

            fileInfo.MoveTo(System.IO.Path.Combine(fileInfo.DirectoryName,
                $"{fileTags.Tag.Track} - {fileTags.Tag.Title}.{fileInfo.Extension}"));
        }
    }
}

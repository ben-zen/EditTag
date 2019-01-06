// Copyright (C) 2018 Studio Perigee.
// Licensed under the MIT license.

using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace EditTag
{
    class Program
    {
        public enum AlbumProperty
        {
            AlbumArtists,
            AlbumTitle,
            DiskCount, // Setting this value implies TrackProperties.DiskNumber will also be set.
            Genre,
            Publisher,
            TrackCount,
            Year
        }

        public enum TrackProperty
        {
            Artists,
            DiskNumber,
            Title,
            TrackNumber
        }

        public class Options
        {
            [Option(Required = false, HelpText = "Declares the folder that EditTags will update tags for, treating the folder as a single album.")]
            public string AlbumDirectory { get; set; }

            [Option(Required = true, Separator = ',')]
            public List<AlbumProperty> AlbumProperties { get; set; }

            [Option(Default = false)]
            public bool Rename { get; set; }

            [Option(Required = true, Separator = ',')]
            public List<TrackProperty> TrackProperties { get; set; }

            [Option(Required = false, Default = false)]
            public bool Verbose { get; set; }
        }

        static void WriteHelp()
        {
            var writer = Console.Out;
            writer.WriteLine("EditTag v1.0");
            writer.WriteLine("Copyright (c) 2019 Studio Perigee.");
            writer.WriteLine("----------");
            writer.WriteLine();

            // Write all of the option strings.


            writer.WriteLine("--help \t Present this help screen.");
            writer.WriteLine("--version \t Get version information.");
        }

        static void HandleErrors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                switch (error.Tag)
                {
                    case ErrorType.HelpRequestedError:
                        WriteHelp();
                        break;
                    default:
                        Console.WriteLine($"Unexpected error {Enum.GetName(typeof(ErrorType), error.Tag)} encountered.");
                        break;
                }
            }
        }

        static void HandleOperation(Options options)
        {
            if (options.Verbose)
            {
                
            }

            var albumDirectory = (options.AlbumDirectory != null) ? options.AlbumDirectory : Directory.GetCurrentDirectory();
            // Confirm that the first argument (only?) is a folder path.
            if (!Directory.Exists(albumDirectory))
            {
                Console.WriteLine("Enter a directory that exists!");
                return;
            }

            // Loop over files in a supplied directory.

            // Check for argument someday...
            EditAlbumTags(albumDirectory);

            foreach (var file in Directory.GetFiles(albumDirectory))
            {
                EditTrackTags(file);
            }

            if (options.Rename)
            {
                // Rename files to match their tags.
                foreach (var file in Directory.GetFiles(albumDirectory))
                {
                    RenameFile(file);
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Run `EditTag.exe --help` for help.");
            }
            else
            {
                var parser = new Parser(opts => { opts.HelpWriter = null; opts.CaseInsensitiveEnumValues = true; });
                parser.ParseArguments<Options>(args)
                    .WithNotParsed(HandleErrors)
                    .WithParsed(HandleOperation);
            }
        }

        static bool GetYesNoResponse()
        {
            while (true)
            {
                switch (Console.ReadLine().ToLowerInvariant())
                {
                    case "":
                    case "y":
                        return true;
                    case "n":
                        return false;
                    default:
                        Console.WriteLine("Enter n for no, or y for yes; or just hit enter for yes.");
                        break;
                }
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

            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                var fileTags = TagLib.File.Create(file);
                if (albumArtist != null)
                {
                    fileTags.Tag.AlbumArtists = new string[] { albumArtist };
                }

                fileTags.Tag.Album = albumTitle;
                fileTags.Tag.Genres = new string[] { albumGenre };
                fileTags.Tag.Year = uint.Parse(albumYear);

                if (fileTags.Tag.TrackCount != files.Count())
                {
                    fileTags.Tag.TrackCount = (uint) files.Count(); // Count is an int(!?) so we need to convert it.
                }
                fileTags.Save();
            }
        }

        static void EditTrackTags(string filePath)
        {
            var fileTags = TagLib.File.Create(filePath);

            Console.WriteLine($"Editing information for track {fileTags.Tag.Track}.");

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
                $"{fileTags.Tag.Track} - {fileTags.Tag.Title}{fileInfo.Extension}"));
        }
    }
}

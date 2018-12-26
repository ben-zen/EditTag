﻿// Copyright (C) 2018 Studio Perigee.
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
        public enum AlbumProperties
        {
            AlbumArtists,
            AlbumTitle,
            DiskCount, // Setting this value implies TrackProperties.DiskNumber will also be set.
            Genre,
            Publisher,
            TrackCount,
            Year
        }

        public enum TrackProperties
        {
            Artists,
            DiskNumber,
            Title,
            TrackNumber
        }

        public class Options
        {
            [Option(HelpText = "Declares the folder that EditTags will update tags for, treating the folder as a single album.")]
            public string AlbumDirectory { get; set; }

            [Option(Separator = ',', HelpText = "If this parameter is set, only the album properties supplied will be edited.")]
            public List<string> AlbumProperties { get; set; }

            [Option(Separator = ',', HelpText = "If set, declares the track properties that EditTags will offer to edit for each track.")]
            public List<string> TrackProperties { get; set; }
        }

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
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

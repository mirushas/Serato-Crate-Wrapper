using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SeratoCrateWrapper
{
    public class SeratoCrate
    {
        // V1.0 Support for reading/writing crate files with all Songs saved on the same Drive.

        // ToDo V1.1 Support for songs from multiple drives in one crate.

        // Columns writing (not yet supported)
        //static readonly byte[] _osrt = Encoding.UTF8.GetBytes("osrt");
        //static readonly byte[] _columnSortEnd = new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00 };
        //static readonly byte[] _tvcn = Encoding.UTF8.GetBytes("tvcn");
        //static readonly byte[] _brev = Encoding.UTF8.GetBytes("brev");

        //Crate identifier
        static readonly byte[] _crateBeginning = Encoding.BigEndianUnicode.GetBytes("Serato ScratchLive");

        // Columns reading
        static readonly byte[] _columnBeginning = Encoding.UTF8.GetBytes("ovct");
        static readonly byte[] _columnNameEnd = Encoding.UTF8.GetBytes("tvcw");

        // Songs
        static readonly byte[] _songBeginning = Encoding.UTF8.GetBytes("otrk");
        static readonly byte[] _songFilePathEnd = Encoding.UTF8.GetBytes("ptrk");

        public List<string> Songs;
        private List<byte> _crateDataExcludingSongs;

        private SeratoCrate(
            List<string> songs,
            List<byte> crateDataExcludingSongs) 
        {
            Songs = songs;
            _crateDataExcludingSongs = crateDataExcludingSongs;
        }

        private static void ValidatePath(
            string filePath)
        {
            var folderPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folderPath))
                throw new ArgumentOutOfRangeException(nameof(filePath), "Directory does not exist.");

            if (Path.GetExtension(filePath) != ".crate")
                throw new ArgumentOutOfRangeException(nameof(filePath), "File is not a .crate file");
        }

        /// <summary>
        /// Loads an existing Serato ScratchLive Crate.
        /// </summary>
        /// <param name="filePath">The full path to the .crate file.</param>
        /// <returns>An Instance of the SeratoCrate Class containing the song data read from the .crate file</returns>
        public static SeratoCrate Load(
            string filePath)
        {
            if (!File.Exists(filePath))
                throw new ArgumentOutOfRangeException(nameof(filePath), "File does not exist.");

            var fileBytes = File.ReadAllBytes(filePath);

            if (fileBytes.IndexOf(_crateBeginning) == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "File is not a serato crate.");
            }

            var songstart = fileBytes.IndexOf(_songFilePathEnd) + 8; // Advance to the start of the song info

            return new SeratoCrate(
                ReadSongs(fileBytes, songstart),
                fileBytes.Slice(0, songstart - 16).ToList());
        }

        /// <summary>
        /// Creates a new Instance of the SeratoCrate class with Serato's default settings
        /// </summary>
        /// <returns>A new Instance of the SeratoCrate class with Serato's default settings</returns>
        public static SeratoCrate CreateNew()
        {
            return new SeratoCrate(
                new List<string>(),
                Properties.Resources.DefaultCrateWithoutSongs.ToList());
        }

        /// <summary>
        /// Adds a song to the crate
        /// </summary>
        /// <param name="filePathWithoutDrivePart">The full path to the songfile, leaving out the drive identifier (C:\Music\test.mp3 -> Music\test.mp3) !!All songs have to be from the same drive as the crate!!</param>
        public void AddSong(
            string filePathWithoutDrivePart)
        {
            Songs.Add(filePathWithoutDrivePart);
        }

        /// <summary>
        /// Removes a song from the crate
        /// </summary>
        /// <param name="filePathWithoutDrivePart">The full path to the songfile, leaving out the drive identifier (C:\Music\test.mp3 -> Music\test.mp3) !!All songs have to be from the same drive as the crate!!</param>
        public void RemoveSong(
            string filePathWithoutDrivePart)
        {
            if (!Songs.Contains(filePathWithoutDrivePart))
                throw new ArgumentOutOfRangeException(nameof(filePathWithoutDrivePart), "This song is currently not in the crate.");
            
            Songs.Remove(filePathWithoutDrivePart);
        }

        /// <summary>
        /// Saves the state of the Instance into a .crate file
        /// </summary>
        /// <param name="filePath"></param>
        public void WriteFile(
            string filePath)
        {
            ValidatePath(filePath);

            _crateDataExcludingSongs.AddRange(GetSongDataForWrite());

            File.WriteAllBytes(filePath, _crateDataExcludingSongs.ToArray());
        }

        private static List<string> ReadSongs(byte[] crateFile, int pointer)
        {
            var songs = new List<string>();

            var nextSongStart = crateFile.IndexOf(_songBeginning, pointer); // Get beginning of the next song
            if (nextSongStart < 0)
                nextSongStart = crateFile.Length;

            while (true)
            {
                // Get path to songfile
                var songData = crateFile.Slice(pointer, nextSongStart);
                songs.Add(Encoding.BigEndianUnicode.GetString(songData));

                // Advance to the beginning of the next song
                pointer = crateFile.IndexOf(_songFilePathEnd, nextSongStart) + 8;
                nextSongStart = crateFile.IndexOf(_songBeginning, pointer);

                if (nextSongStart == -1)
                    break;
                 
                nextSongStart = crateFile.IndexOf(_songBeginning, pointer);
            }
            return songs;
        }

        /// <summary>
        /// Converts the songdata into the serato format
        /// </summary>
        /// <param name="songs">List of paths to the songfiles</param>
        /// <returns>Bytes of the songdata.</returns>
        private List<byte> GetSongDataForWrite()
        {
            var returnBytes = new List<byte>();

            for (int i = 0; i < Songs.Count; i++)
            {
                var songFilePathBytes = Encoding.BigEndianUnicode.GetBytes(Songs[i]);

                if (songFilePathBytes.Last() == 0) //Remove trailing byte
                {
                    songFilePathBytes = songFilePathBytes.Slice(0, songFilePathBytes.Length - 1);
                }

                var doubleLength = Songs[i].Length * 2;

                //Conversion into the SSL Format
                returnBytes.AddRange(_songBeginning); //Starttag for a song "otrk"
                returnBytes.AddRange(BitConverter.GetBytes(doubleLength + 8).Reverse()); //Double length +8 of the songFile in BigEndianUnicode
                returnBytes.AddRange(_songFilePathEnd); //"ptrk"
                returnBytes.AddRange(BitConverter.GetBytes(doubleLength).Reverse()); //Double length of the songFile in BigEndianUnicode
                returnBytes.AddRange(songFilePathBytes); //Path to the songFile in BigEndianUnicode
            }

            return returnBytes;
        }
    }
}

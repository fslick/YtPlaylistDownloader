using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using System.Linq;
using System.IO;

namespace PlaylistDownloader
{
    internal class PlaylistDownloader
    {
        private static readonly YoutubeClient client = new YoutubeClient();

        private static void Main(string[] args)
        {
            Main().GetAwaiter().GetResult();

            Console.WriteLine("done");
            Console.ReadKey();
        }

        private static async Task Main()
        {
            var video = await client.GetVideoAsync("");
            string playlistName = "";
            var tuple = await DownloadVideo(video, playlistName);
            ApplyTags(tuple.filePath, tuple.video, playlistName);
        }

        private static async Task<(string filePath, Video video)> DownloadVideo(Video video, string folderName)
        {
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(video.Id);
            var streamInfo = streamInfoSet.Audio
                .Where(a => a.Container.GetFileExtension().ToLower() != "webm")
                .OrderByDescending(a => a.Bitrate)
                .FirstOrDefault()
                ?? streamInfoSet.Audio.WithHighestBitrate();
            string fileName = SanitizeFileName($"{video.Title} - {video.Author}.{streamInfo.Container.GetFileExtension()}");
            string filePath = Path.Combine(folderName, fileName);
            string extension = streamInfo.Container.GetFileExtension();

            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);

            if (!File.Exists(filePath))
                await client.DownloadMediaStreamAsync(streamInfo, filePath);

            return (filePath, video);
        }

        private static void ApplyTags(string filePath, Video video, string albumName)
        {
            var file = TagLib.File.Create(filePath);
            file.Tag.Album = albumName;
            file.Tag.Performers = new string[] { video.Author };
            file.Tag.Title = video.Title;
            file.Tag.Year = (uint)video.UploadDate.Year;
            file.Save();
        }

        private static string SanitizeFileName(string filename)
        {
            var invalids = Path.GetInvalidFileNameChars();
            return String.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
    }
}
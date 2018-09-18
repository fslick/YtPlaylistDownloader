using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace PlaylistDownloader
{
    //a
    class PlaylistDownloader
    {
        private static readonly YoutubeClient client = new YoutubeClient();

        static void Main(string[] args)
        {
            Main().GetAwaiter().GetResult();
        }

        static async Task Main()
        {
            string playlistId = "";
            var playlist = await client.GetPlaylistAsync(playlistId);
            await DownloadVideo(playlist.Videos[0]);
        }

        static async Task DownloadVideo(Video video)
        {
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(video.Id);

            var streamInfo = streamInfoSet.Audio.WithHighestBitrate();
            string fileName = SanitizeFileName(video.Title);
            string extension = streamInfo.Container.GetFileExtension();
            //await client.DownloadMediaStreamAsync(streamInfo, $"{fileName}.{extension}");
            await Task.Delay(5000);
        }

        static string SanitizeFileName(string filename)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            return String.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
    }
}
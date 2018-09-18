open System
open YoutubeExplode
open YoutubeExplode.Models
open YoutubeExplode.Models.MediaStreams

let client = YoutubeClient()

let getPlaylist playlistId = client.GetPlaylistAsync playlistId |> Async.AwaitTask

let getVideos (playlist: Playlist) = playlist.Videos

let sanitizeFilename (filename: string) = 
    let invalids = System.IO.Path.GetInvalidFileNameChars()
    String.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.')

let downloadVideo (video: Video) = async {
    let! streamInfoSet = client.GetVideoMediaStreamInfosAsync video.Id |> Async.AwaitTask
    let streamInfo = streamInfoSet.Audio.WithHighestBitrate()
    let filename = sprintf "%s - %s.%s" (video.Title |> sanitizeFilename) video.Author (streamInfo.Container.GetFileExtension())
    do! client.DownloadMediaStreamAsync(streamInfo, filename) |> Async.AwaitTask
    //let random = System.Random()
    //do! random.Next(500, 5000) |> System.Threading.Tasks.Task.Delay |> Async.AwaitTask
    filename |> Console.WriteLine
}

let playlistId = ""

let playlist = 
    playlistId
    |> getPlaylist
    |> Async.RunSynchronously
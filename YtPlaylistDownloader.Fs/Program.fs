open System
open System.IO
open YoutubeExplode
open YoutubeExplode.Models
open YoutubeExplode.Models.MediaStreams
open System.Diagnostics

let client = YoutubeClient()

let getPlaylist playlistId = client.GetPlaylistAsync playlistId |> Async.AwaitTask

let getVideos (playlist: Playlist) = playlist.Videos

let sanitizeFilename (filename: string) = 
    let invalids = System.IO.Path.GetInvalidFileNameChars()
    String.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.')

let log (text: string) = sprintf "%s >> %s" (DateTime.Now.ToString("HH:mm:ss.fff")) text |> Console.WriteLine 

let downloadVideo path (video: Video) = 
    async {
        if Directory.Exists path |> not then path |> Directory.CreateDirectory |> ignore else
        let! streamInfoSet = client.GetVideoMediaStreamInfosAsync video.Id |> Async.AwaitTask
        let streamInfo = streamInfoSet.Audio.WithHighestBitrate()
        let filename = sprintf "%s - %s.%s" (video.Title |> sanitizeFilename) video.Author (streamInfo.Container.GetFileExtension())
        let filepath = sprintf "%s\%s" path filename
        if File.Exists filepath |> not then
            do! client.DownloadMediaStreamAsync(streamInfo, filepath) |> Async.AwaitTask
            //let random = System.Random()
            //do! random.Next(500, 5000) |> System.Threading.Tasks.Task.Delay |> Async.AwaitTask
        else 
            sprintf "%s exists, skipped" filename |> log
            
    }

let runWithLogging (id: string) (func: Async<unit>) = 
    async {
        sprintf "%s started" id |> log
        let sw = Stopwatch.StartNew()
        do! func
        sw.Stop()
        sprintf "%s finished (%d ms)" id sw.ElapsedMilliseconds |> log

    }

let shuffle seq =
    let array = seq |> Seq.toArray
    let random = Random()
    for i in 0 .. array.Length - 1 do
        let j = random.Next(i, array.Length)
        let pom = array.[i]
        array.[i] <- array.[j]
        array.[j] <- pom
    array |> Array.toSeq

let playlistId = ""

let playlist = 
    playlistId
    |> getPlaylist
    |> Async.RunSynchronously

let task = 
    playlist
    |> getVideos
    |> shuffle
    |> Seq.map(fun v -> (v.Title, downloadVideo playlist.Title v))
    |> Seq.map(fun t -> runWithLogging (fst t) (snd t))
    |> Async.Parallel

task |> Async.RunSynchronously |> ignore

"done" |> log
Console.ReadKey() |> ignore
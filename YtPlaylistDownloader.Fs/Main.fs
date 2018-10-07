open System
open System.Diagnostics
open Downloader
open Tagger

let log (text: string) = sprintf "%s >> %s" (DateTime.Now.ToString("HH:mm:ss.fff")) text |> Console.WriteLine

let runWithLogging (id: string, func: Async<'a>) =
    async {
        sprintf "%s started" id |> log
        let sw = Stopwatch.StartNew()
        let! result = func
        sw.Stop()
        sprintf "%s finished (%d ms)" id sw.ElapsedMilliseconds |> log
        return result
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
    playlist.Videos
    |> shuffle
    |> Seq.map(fun v -> (v.Title, downloadVideoFromPlaylist playlist v))
    |> Seq.map(fun t -> runWithLogging t)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Seq.iter applyTags

"done" |> log
Console.ReadKey() |> ignore
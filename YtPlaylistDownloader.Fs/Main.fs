open System
open System.Diagnostics
open Downloader
open Tagger

let log (text: string) = sprintf "%s >> %s" (DateTime.Now.ToString("HH:mm:ss.fff")) text |> Console.WriteLine

let shuffle seq =
    let array = seq |> Seq.toArray
    let random = Random()
    for i in 0 .. array.Length - 1 do
        let j = random.Next(i, array.Length)
        let pom = array.[i]
        array.[i] <- array.[j]
        array.[j] <- pom
    array |> Array.toSeq

type AsyncTask<'a> = { Id: string; Func: Async<'a>}

let runWithLogging task =
    async {
        sprintf "%s started" task.Id |> log
        let sw = Stopwatch.StartNew()
        let! result = task.Func
        sw.Stop()
        sprintf "%s finished (%d ms)" task.Id sw.ElapsedMilliseconds |> log
        return result
    }

let playlistId = ""

let playlist =
    playlistId
    |> getPlaylist
    |> Async.RunSynchronously

let task =
    playlist.Videos
    |> shuffle
    |> Seq.map(fun v -> 
        let func = 
            async {
                let! metadata = downloadVideoFromPlaylist playlist v
                metadata |> applyTags
                return metadata 
            }
        { Id = v.Title; Func = func })
    |> Seq.map runWithLogging
    |> Async.Parallel
    |> Async.RunSynchronously

"done" |> log
Console.ReadKey() |> ignore
module Downloader

open System
open System.IO
open YoutubeExplode
open YoutubeExplode.Models.MediaStreams
open Types

let private client = YoutubeClient()

let getPlaylist playlistId =
    async {
        let! p = client.GetPlaylistAsync playlistId |> Async.AwaitTask
        return {
            Id = p.Id
            Title = p.Title
            Videos = p.Videos |> Seq.map(fun v -> { Id = v.Id; Title = v.Title; Author = v.Author })
        }
    }

let private sanitizeFilename (filename: string) =
    let invalids = Path.GetInvalidFileNameChars()
    String.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.')

let downloadVideoFromPlaylist (playlist: Types.Playlist) (video: Types.Video) =
    let retrieveAudioStreamInfo (streamInfoSet: MediaStreamInfoSet) =
        let audio =
            streamInfoSet.Audio
            |> Seq.filter (fun a -> a.Container.GetFileExtension().ToLower() = "webm" |> not)
            |> Seq.sortByDescending (fun a -> a.Bitrate)
            |> Seq.tryHead
        match audio with
        | Some asi -> asi
        | None -> streamInfoSet.Audio.WithHighestBitrate()

    async {
        let path = playlist.Title
        if Directory.Exists path |> not then path |> Directory.CreateDirectory |> ignore
        let! streamInfoSet = client.GetVideoMediaStreamInfosAsync video.Id |> Async.AwaitTask
        let streamInfo = streamInfoSet |> retrieveAudioStreamInfo
        let filename = sprintf "%s - %s.%s" (video.Title |> sanitizeFilename) video.Author (streamInfo.Container.GetFileExtension())
        let filepath = sprintf "%s\%s" path filename
        if File.Exists filepath |> not then
            do! client.DownloadMediaStreamAsync(streamInfo, filepath) |> Async.AwaitTask
        return {
            Video = video
            FileName = filename
            FilePath = filepath
            Playlist = Some playlist
        }
    }
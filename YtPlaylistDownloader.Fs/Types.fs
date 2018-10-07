module Types

open System

type Video = {
    Id: string
    Title: string
    Author: string
    UploadDate: DateTimeOffset
}

type Playlist = {
    Id: string
    Title: string
    Videos: seq<Video>
}

type DownloadedVideo = {
    Video: Video
    Playlist: option<Playlist>
    FileName: string
    FilePath: string
}
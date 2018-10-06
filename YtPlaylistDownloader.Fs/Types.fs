module Types

type Video = {
    Id: string
    Title: string
    Author: string
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
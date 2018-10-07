module Tagger

open Types

let applyTags (metadata: DownloadedVideo) =
    let file = TagLib.File.Create(metadata.FilePath)
    file.Tag.Album <- 
        match metadata.Playlist with
        | Some playlist -> playlist.Title
        | None -> null
    file.Tag.Performers <- [| metadata.Video.Author |]
    file.Tag.Title <- metadata.Video.Title
    file.Tag.Year <- metadata.Video.UploadDate.Year |> uint32
    file.Save()
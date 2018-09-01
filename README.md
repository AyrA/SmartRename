# SmartRename

Converts Stupid names into normal ones

## Download

Check the [Releases Section](https://github.com/AyrA/SmartRename/releases) for precompiled binaries.

## How To Use

	SmartRename.exe C:\Path\To\Unnecessary\Directory.With.Long.And.Stupid.Name.2000.Oh.God.Why

You can specify multiple directories.
This application does not supports wildcards.

## Limitations

As of now, the application is designed to be run on directories that primarily are the result of downloading Video files off various sources.

## Action Confirmation

The application will show a list of all pending actions and will ask for confirmation before doing so.

## List of actions

### Directory Name Conversion

Convert Stupid name of main directory into normal name.

This converts
`C:\Path\To\Some.Downloaded.Video.File.2000.Xvid.How.Long.Can.This.Get` into
`C:\Path\To\Some Downloaded Video File`

### Rename Video files to match normal name

Regardless of what the video file name is, it's renamed to the Normalized name from the main directory.
This application will rename `MKV`, `AVI` and `MP4`

### Rename Subtitles to match video file name

If there are subtitles they will be renamed to match the video file name.
This application will rename `IDX`, `SUB` and `SRT` files.

This rename operation is special. Subtitle files usually end in a 2 or 3 letter language code or `forced`.
The application will keep that suffix intact.

### Delete unnecessary files and directories

- Deleted Directories: `Proof` and `Sample`
- Deleted Files by extension: `NFO`, `LNK`, `URL`, `SFV`, `TXT` and `DIZ`

## `WARNING`

Only use if the download was for a single video file.
Don't use for video collections, games or audio albums.
Carefully review the pending changes because they can't be undone.

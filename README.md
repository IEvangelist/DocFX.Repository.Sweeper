# DocFX.Repository.Sweeper :metal:

The `DocFX.Repository.Sweeper` (**sweeper**) is a tool for sweeping **DocFX** repositories clean of files that are not being used.

> This project was directly inspired by [Genevieve Warren - @gewarren](https://github.com/gewarren) and her amazing [CleanRepo](https://github.com/gewarren/cleanrepo) tool.

## Let's :eyes: at the tool

This command-line tool helps to identify (and optionally delete) files within a **DocFX** repository that are not referenced by any other file.

  :heavy_check_mark: Find (and delete) markdown files not referenced in TOC, index, or other markdown files
  :heavy_check_mark: Find (and delete) image files not referenced in TOC, index, or other markdown files

## Getting started :clipboard:

The **sweeper** executable is a `.NET Core` project and can be executed from the command-line with the `dotnet` CLI. There are several options avialble:

| Option | Description | Default |
|--:|:--|:--|
| `-s` (required) | The source directory to act on (can be subdirectory or top-level) | `null` | 
| `-t` | If true, finds orphaned topic (markdown) files | `true` |
| `-i` | If true, finds orphaned image files (.png, .jpg, .jpeg, .gif, .svg) | `true` |
| `-d` | If true, deletes orphaned markdown or image files | `false` |

Executing the following command will find all the files within the `cognitive-serivces` directory that are not referenced anywhere else in the entire repository (relevant to the `docfx.json` file).

```
dotnet sweeper.dll -s "C:\repo\azure-docs-pr\articles\cognitive-services"
```

## Toubleshooting :poop:

```
// TODO: add details about using Git to undo issues that may arrise
```
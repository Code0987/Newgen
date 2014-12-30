## Newgen

A simple package manager and metro gui replacement for Windows.

## Packages

Package format

    - Package files (applications, docs, ...) *

    - Package.Install (script for post installation) ?
    - Package.UnInstall (script for pre un-installation) ?

    - Package.Metadata (metadata)
    - Package.Settings (settings made by user after installation)

## ToDo

    Package Manager
        App Link
        Launcher for Apps, Files, Urls
        Internal integrated package

    Packages

    Help package
        HTML based help slideshow

    Social package
        Facebook
        G+
        Twitter

    Internet package
        Firefox
        I.E.
        Chrome

    UI themes package
        Include settings modification
        Background images

_Bug fixes_
    Hide Start Screen on opening any external app
    Add a FAQ page
    Change bg/icon/gfx
    Fix bugs
    Fix user tile
    Prepare for v13
    Reduce features not needed
    Update Webkit

## Ideas

Use node-webkit as HTML apps engine with edge.js as bridge b/w HTML apps and C# core app. 

## Runtime/Working Structure

App
|---- GUI Thread 
    |---- StartScreen, StartBar, Other UI ...

|---- Package Server for HTML apps Thread
    |---- node+edge

## Resources Structure

App Root Directory
|---- F: Main executable 
|---- F: #r .Net Libraries
|---- D: Packages
    |---- *: All packages are installed here
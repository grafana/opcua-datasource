#!/usr/bin/env sh

dotnet publish ./backend/.linux.build.csproj -r linux-x64 --self-contained true
#dotnet publish ./backend/.windows.build.csproj -r win-x64 --self-contained true
#dotnet publish ./backend/.osx.build.csproj -r osx-x64 --self-contained true

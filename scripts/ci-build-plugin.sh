#!/usr/bin/env sh

[ ! -d ci/jobs/build_backend ] && mkdir -pv ci/jobs/build_backend

# Build Linux
dotnet publish ./backend/.linux.build.csproj -r linux-x64 --self-contained true
mv -v dist ci/jobs/build_backend/linux

# Build Windows
dotnet publish ./backend/.windows.build.csproj -r win-x64 --self-contained true
mv -v dist ci/jobs/build_backend/windows

# Build OSX
dotnet publish ./backend/.osx.build.csproj -r osx-x64 --self-contained true
mv -v dist ci/jobs/build_backend/darwin

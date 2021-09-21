#!/usr/bin/env sh

[ ! -d ci/jobs/build_backend ] && mkdir -pv ci/jobs/build_backend

# Build Linux
dotnet publish ./backend/.linux.build.csproj -r linux-x64 --self-contained true
# Lint
mv -v dist ci/jobs/build_backend/linux

# add dist back, plugin:ci-build --finish moved it
mkdir dist
# Build Windows
dotnet publish ./backend/.windows.build.csproj -r win-x64 --self-contained true
mv -v cdist ci/jobs/build_backend/windows

# add dist back, plugin:ci-build --finish moved it
mkdir dist
# Build OSX
dotnet publish ./backend/.osx.build.csproj -r osx-x64 --self-contained true
mv -v dist ci/jobs/build_backend/darwin

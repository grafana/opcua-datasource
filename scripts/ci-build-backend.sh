#!/usr/bin/env sh

set -e

[ ! -d ci/jobs/build_backend ] && mkdir -pv ci/jobs/build_backend || echo "build backend already created"

[ -d dist ] && /bin/rm -rf dist || echo "no dist directory exist"
[ ! -d dist ] && mkdir -pv dist || echo "dist directory already created"


# Build Linux
[ ! -d dist ] && mkdir -pv dist || echo "Linux build: dist already created"
dotnet publish ./backend/.linux.build.csproj -r linux-x64 --self-contained true
mv -v dist ci/jobs/build_backend/linux

# Build Windows
[ ! -d dist ] && mkdir -pv dist || echo "Windows build: dist already created"
dotnet publish ./backend/.win.build.csproj -r win-x64 --self-contained true
mv -v dist ci/jobs/build_backend/windows


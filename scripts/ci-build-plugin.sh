#!/usr/bin/env sh

# Build Linux
dotnet publish ./backend/.linux.build.csproj -r linux-x64 --self-contained true
# Lint
./node_modules/.bin/grafana-toolkit plugin:ci-build --finish
mv -v ci/jobs/build_backend/dist ci/jobs/build_backend_linux

# add dist back, plugin:ci-build --finish moved it
mkdir dist
# Build Windows
dotnet publish ./backend/.windows.build.csproj -r win-x64 --self-contained true
./node_modules/.bin/grafana-toolkit plugin:ci-build --finish
mv -v ci/jobs/build_backend/dist ci/jobs/build_backend_windows

# add dist back, plugin:ci-build --finish moved it
mkdir dist
# Build OSX
dotnet publish ./backend/.osx.build.csproj -r osx-x64 --self-contained true
./node_modules/.bin/grafana-toolkit plugin:ci-build --finish
mv -v ci/jobs/build_backend/dist ci/jobs/build_backend_darwin

[ ! -d ci/jobs/build_backend ] && mkdir -pv ci/jobs/build_backend
# Organize results
mv -v ci/jobs/build_backend_linux ci/jobs/build_backend/linux
mv -v ci/jobs/build_backend_windows ci/jobs/build_backend/windows
mv -v ci/jobs/build_backend_darwin ci/jobs/build_backend/darwin    
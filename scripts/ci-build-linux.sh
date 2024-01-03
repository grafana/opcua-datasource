#!/usr/bin/env sh

set -e

dotnet publish ./backend/.linux.build.csproj -r linux-x64 --self-contained true

# upload step needs grabpl
mkdir -p build-pipeline/bin
curl -fL -o build-pipeline/bin/grabpl https://grafana-downloads.storage.googleapis.com/grafana-build-pipeline/v2.9.51/grabpl
chmod +x build-pipeline/bin/grabpl

DSNAME=grafana-opcua-datasource
ifeq ($(OS),Windows_NT) 
    BUILD_OS := windows
else
    BUILD_OS := $(shell sh -c '(uname 2>/dev/null | tr A-Z a-z) || echo linux')
endif

all: build

restore: 
	dotnet restore ./pkg/dotnet/plugin-dotnet/plugin-dotnet.csproj -r linux-x64 

build:
ifeq (${BUILD_OS},windows)
	dotnet publish ./backend/.windows.build.csproj -o ./dist --self-contained true
endif
ifeq (${BUILD_OS},darwin)
	pidof gpx_opcua && kill $$(pidof gpx_opcua | tr -d '\n') || true
	dotnet publish ./backend/.osx.build.csproj -o ./dist --self-contained true
endif
ifeq (${BUILD_OS},linux)
	pidof gpx_opcua && kill $$(pidof gpx_opcua | tr -d '\n') || true
	dotnet publish ./backend/.linux.build.csproj -o ./dist --self-contained true
endif

watch:
ifeq (${BUILD_OS},windows)
	dotnet watch -p ./backend/.windows.build.csproj publish .windows.build.csproj -o ./dist --self-contained true
endif
ifeq (${BUILD_OS},darwin)
	pidof gpx_opcua && kill $$(pidof gpx_opcua | tr -d '\n') || true
	dotnet watch -p ./backend/.osx.build.csproj publish .osx.build.csproj -o ./dist --self-contained true
endif
ifeq (${BUILD_OS},linux)
	pidof gpx_opcua && kill $$(pidof gpx_opcua | tr -d '\n') || true
	dotnet watch -p ./backend/.linux.build.csproj publish .linux.build.csproj -o ./dist --self-contained true
endif
 
.PHONY: vendor

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
	dotnet publish ./backend/.win.build.csproj -r win-x64 --self-contained true
endif
ifeq (${BUILD_OS},darwin)
	dotnet publish ./backend/.osx.build.csproj -r osx-x64 --self-contained true
endif
ifeq (${BUILD_OS},linux)
	dotnet publish ./backend/.linux.build.csproj -r linux-x64 --self-contained true
endif

linux:
	dotnet publish ./backend/.linux.build.csproj -r linux-x64 --self-contained true

windows:
	dotnet publish ./backend/.win.build.csproj -r win-x64 --self-contained true

darwin:
	dotnet publish ./backend/.osx.build.csproj -r osx-x64 --self-contained true

watch:
ifeq (${BUILD_OS},windows)
	dotnet watch -p ./backend/.win.build.csproj publish .windows.build.csproj -r win-x64 --self-contained true
endif
ifeq (${BUILD_OS},darwin)
	dotnet watch -p ./backend/.osx.build.csproj publish .osx.build.csproj -r osx-x64 --self-contained true
endif
ifeq (${BUILD_OS},linux)
	dotnet watch -p ./backend/.linux.build.csproj publish .linux.build.csproj -r linux-x64 --self-contained true
endif
 
.PHONY: vendor

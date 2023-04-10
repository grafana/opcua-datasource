#!/bin/bash

# To compile all protobuf files in this repository, run
# "make protobuf" at the top-level.

set -eu

DST_DIR_CSHARP=../backend/Proto
DST_DIR_GOLANG=../pkg/proto

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ] ; do SOURCE="$(readlink "$SOURCE")"; done
DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"

cd "$DIR"

protoc -I ./ ./backend.proto --csharp_out=${DST_DIR_CSHARP} --grpc_out=${DST_DIR_CSHARP} --plugin=protoc-gen-grpc=/usr/bin/grpc_csharp_plugin
protoc -I ./ ./backend.proto --go_out=${DST_DIR_GOLANG} --plugin=protoc-gen-grpc=/usr/local/bin/protoc-gen-go



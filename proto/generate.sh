#!/bin/bash

# To compile all protobuf files in this repository, run
# "make protobuf" at the top-level.

set -eu

DST_DIR=../backend/Proto

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ] ; do SOURCE="$(readlink "$SOURCE")"; done
DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"

cd "$DIR"

protoc -I ./ backend.proto --csharp_out=${DST_DIR}

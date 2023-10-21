#!/usr/bin/env sh

set -e

[ -d ci/jobs/build-and-test-frontend ] && /bin/rm -rf ci/jobs/build-and-test-frontend || echo "build-and-test-frontend already deleted"

[ -d dist ] && /bin/rm -rf dist || echo "no dist directory exist"
[ ! -d dist ] && mkdir -pv dist || echo "dist directory already created"

# Build Frontend
yarn build
mv dist ci/jobs/build-and-test-frontend


#!/usr/bin/env sh

set -e


PLUGIN_NAME=`cat ci/dist/plugin.json|jq '.id'| sed s/\"//g`
VERSION=`cat ci/dist/plugin.json|jq '.info.version'| sed s/\"//g`
echo "Plugin Name: ${PLUGIN_NAME}"
echo "Plugin Version: ${VERSION}"

#
# ci-package will create the zip file
# move to the dist folder that package uses
mv -v ci/jobs/build_backend/linux ci/jobs/build_backend/dist
./node_modules/.bin/grafana-toolkit plugin:ci-package
# created during ci-package
ls -lR ci/dist/${PLUGIN_NAME}
#
# Building separate linux and windows zip files
#
# 1. rename to linux package
#
mv ci/packages/${PLUGIN_NAME}-${VERSION}.zip \
  ci/packages/${PLUGIN_NAME}-${VERSION}.linux_amd64.zip
mv ci/packages/${PLUGIN_NAME}-${VERSION}.zip.sha1 \
  ci/packages/${PLUGIN_NAME}-${VERSION}.linux_amd64.zip.sha1
#
# 2. update info.json with new zip file name
#
sed -i 's/zip/linux_amd64\.zip/g' ci/packages/info.json
#
# 3. move into linux subdir
#
mkdir -p temp_ci/packages/linux
cp -p ci/packages/info.json temp_ci/packages/linux
cp -p ci/packages/info.json temp_ci/packages/linux/info-linux_amd64.json
mv ci/packages/${PLUGIN_NAME}* temp_ci/packages/linux
mv -v ci/jobs/build_backend/dist ci/jobs/build_backend/linux
#
# now create the windows package
#
# 1. remove dist
#
mv -v ci/jobs/build_backend/windows ci/jobs/build_backend/dist
echo "Windows dist"
ls -lR ci/jobs/build_backend/dist
#
# 2. extract instantclient for windows into dist
#
/bin/rm -rf ci/jobs/build_backend/dist/lib # Remove the original linux instantclient
#
# 3. re-run ci-package to create the windows-only zip
#
./node_modules/.bin/grafana-toolkit plugin:ci-package
#
# 4. rename zip
#
mv ci/packages/${PLUGIN_NAME}-${VERSION}.zip \
  ci/packages/${PLUGIN_NAME}-${VERSION}.windows_amd64.zip
mv ci/packages/${PLUGIN_NAME}-${VERSION}.zip.sha1 \
  ci/packages/${PLUGIN_NAME}-${VERSION}.windows_amd64.zip.sha1
#
# update info.json with new zip file name
#
sed -i 's/zip/windows_amd64\.zip/g' ci/packages/info.json
cp ci/packages/info.json ci/packages/info-windows_amd64.json
#
# 6. put the linux builds back into place
#
cp temp_ci/packages/linux/* ci/packages

# create a package job folder (referenced by other steps) and copy dist
mkdir -p ci/jobs/package
cp -r -v ci/jobs/build_backend/* ci/jobs/package

# DONE
ls -lR ci/packages
#!/usr/bin/env sh

set -e

ci_package() {
  build="$1"
  arch="$2"

  [ -d ci/dist/grafana-opcua-datasource ] && /bin/rm -rf ci/dist/grafana-opcua-datasource || echo "no dist dir found. Good."
  [ ! -d ci/dist ] && mkdir -pv ci/dist || echo "dist dir alread created. Good."

  mv -v ci/jobs/build_backend/$build/dist ci/dist/grafana-opcua-datasource
  cp -rfv ci/jobs/build-and-test-frontend/dist/ ci/dist/grafana-opcua-datasource

  PLUGIN_NAME=`cat ci/dist/plugin.json|jq '.id'| sed s/\"//g`
  VERSION=`cat ci/dist/plugin.json|jq '.info.version'| sed s/\"//g`
  echo "Plugin Name: ${PLUGIN_NAME}"
  echo "Plugin Version: ${VERSION}"
  
  cd ci/dist/grafana-opcua-datasource
  npx @grafana/sign-plugin@latest --rootUrls="http://localhost:3000"
  zip -r grafana-opcua-datasource_${build}_${arch}.zip grafana-opcua-datasource
  cd ../../.. 
  mv ci/dist/grafana-opcua-datasource_${build}_${arch}.zip ci/packages
}




ci_package "linux" "amd64"
ci_package "windows" "amd64"

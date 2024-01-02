#!/usr/bin/env sh

set -e

ci_package() {
  build="$1"
  arch="$2"

  [ -d ci/dist/grafana-opcua-datasource ] && /bin/rm -rf ci/dist/grafana-opcua-datasource || echo "no dist dir found. Good."
  [ ! -d ci/dist ] && mkdir -pv ci/dist || echo "dist dir alread created. Good."

  mv -v ci/jobs/build_backend/$build/dist ci/dist/grafana-opcua-datasource
  cp -rfv ci/jobs/build-and-test-frontend/dist/ ci/dist/grafana-opcua-datasource

  PLUGIN_NAME=$(cat ci/dist/grafana-opcua-datasource/plugin.json | jq '.id' | sed s/\"//g)
  VERSION=$(cat ci/dist/grafana-opcua-datasource/plugin.json | jq '.info.version' | sed s/\"//g)
  echo "Plugin Name: ${PLUGIN_NAME}"
  echo "Plugin Version: ${VERSION}"

  cd ci/dist/grafana-opcua-datasource
  npx @grafana/sign-plugin@latest
  zip -r grafana-opcua-datasource_${build}_${arch}.zip grafana-opcua-datasource
  cd ../../..
  mv ci/dist/grafana-opcua-datasource_${build}_${arch}.zip ci/packages

}

# ci_package "linux" "amd64"
# ci_package "windows" "amd64"

[ -d ci/dist/grafana-opcua-datasource ] && /bin/rm -rf ci/dist/grafana-opcua-datasource || echo "no dist dir found. Good."
[ ! -d ci/dist ] && mkdir -pv ci/dist || echo "dist dir alread created. Good."

mv -v ci/jobs/build_backend/linux/dist ci/dist/grafana-opcua-datasource
mv -v ci/jobs/build_backend/windows/dist ci/dist/grafana-opcua-datasource
cp -rfv ci/jobs/build-and-test-frontend/dist/ ci/dist/grafana-opcua-datasource

if [ -d \"/build/dist\" ]; then
  mkdir -p ci/jobs/package
  mv /build/dist ci/jobs/package/
else
  ./node_modules/.bin/grafana-toolkit plugin:ci-build --finish
fi

./bin/grabpl plugin package

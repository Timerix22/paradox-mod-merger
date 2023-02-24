#!/usr/bin/bash

set -e

function publish_aot() {
    echo "---------[$1]---------"
    cd "$1"
    rm -rf bin/publish
    dotnet publish -c Release -o bin/publish -p:PublishAot=true
    mkdir -p ../publish
    cp -r bin/publish/* ../publish/
    cd ..
}

rm -rf publish
publish_aot paradox-mod-merger
publish_aot diff-text
ls -lh publish

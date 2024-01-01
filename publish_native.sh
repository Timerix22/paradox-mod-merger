#!/usr/bin/bash

set -e

function publish_aot() {
    echo "---------[$1]---------"
    cd "$1"
    rm -rf bin/publish
    dotnet publish -c Release -o bin/publish -p:PublishAot=true --self-contained
    sleep 0.1
    rm bin/publish/*.pdb
    mkdir -p ../publish
    cp -r bin/publish/* ../publish/
    cd ..
}

rm -rf publish
# paradox-mod-merger publishes diff-text as dotnet executable
#publish_aot diff-text
publish_aot paradox-mod-merger
ls -lh publish

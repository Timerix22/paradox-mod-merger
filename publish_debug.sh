#!/bin/sh
rm -rf publish
mkdir publish
dotnet publish -c debug -o publish

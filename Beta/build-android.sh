#!/bin/bash

cd ..
dotnet clean

cd Beta
dotnet build -r android-arm64 -f net8.0-android
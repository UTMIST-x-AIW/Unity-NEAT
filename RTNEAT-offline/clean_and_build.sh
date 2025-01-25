#!/bin/bash

echo "Cleaning solution..."
dotnet clean

echo "Removing bin and obj directories..."
rm -rf bin/ obj/

echo "Restoring packages..."
dotnet restore

echo "Building solution..."
dotnet build

echo "Done!"
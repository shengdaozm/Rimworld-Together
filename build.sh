#!/bin/bash
ROOT="$(cd "$(dirname "$0")" && pwd)"

echo "=== Building RTShared ==="
dotnet build "$ROOT/Source/RTShared/RTShared.csproj" > /dev/null

echo "=== Building RTNetwork ==="
dotnet build "$ROOT/Source/RTNetwork/RTNetwork.csproj" > /dev/null

cp -f "$ROOT/Source/RTShared/bin/Debug/netstandard2.0/RTShared.dll" \
      "$ROOT/Source/RTNetwork/bin/Debug/netstandard2.0/RTNetwork.dll" \
      "$ROOT/Source/Assemblies/"

echo "=== Building Server ==="
rm -rf "$ROOT/Source/Server/obj" "$ROOT/Source/Server/bin"
dotnet build "$ROOT/Source/Server/GameServer.csproj" > /dev/null

echo "=== Building RTClient ==="
dotnet build "$ROOT/Source/RTClient/RTClient.csproj" > /dev/null

echo "=== Deploying DLLs ==="
cp -f "$ROOT/Source/RTShared/bin/Debug/netstandard2.0/RTShared.dll" \
      "$ROOT/Source/RTNetwork/bin/Debug/netstandard2.0/RTNetwork.dll" \
      "$ROOT/Source/RTClient/bin/Debug/net48/RTClient.dll" \
      "$ROOT/Source/RTShared/bin/Debug/netstandard2.0/Newtonsoft.Json.dll" \
      "$ROOT/Source/Assemblies/"

for ver in 1.5 1.6; do
    mkdir -p "$ROOT/$ver/Assemblies"
    cp -f "$ROOT/Source/Assemblies/"*.dll "$ROOT/$ver/Assemblies/"
done

echo "=== Done ==="
ls -lh "$ROOT/Source/Assemblies/RT"*.dll

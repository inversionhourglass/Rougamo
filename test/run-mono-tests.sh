#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
echo "Building Issues to ensure weaving..."
dotnet build $DIR/TestAssemblies/Issues/Issues.csproj -c Debug
echo "Building Fody Tests..."
dotnet build $DIR/Rougamo.Fody.Tests/Rougamo.Fody.Tests.csproj -c Debug -f net48
echo "Running xUnit via Mono..."
mono ~/.nuget/packages/xunit.runner.console/2.4.2/tools/net472/xunit.console.exe $DIR/Rougamo.Fody.Tests/bin/Debug/net48/Rougamo.Fody.Tests.dll

dotnet add package coverlet.msbuild
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/coverage.opencover.xml
reportgenerator -reports:TestResults/coverage.opencover.xml -targetdir:TestResults/CoverageReport -reporttypes:Html
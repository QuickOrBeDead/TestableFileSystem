@echo off
rd /s /q coverage 2>nul
md coverage

set opencover="%USERPROFILE%\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe"
set reportgenerator="%USERPROFILE%\.nuget\packages\ReportGenerator\2.5.10\tools\ReportGenerator.exe"
set testrunner="%USERPROFILE%\.nuget\packages\xunit.runner.console\2.2.0\tools\xunit.console.x86.exe"
set targets=".\Fakes.Tests\bin\Debug\net452\TestableFileSystem.Fakes.Tests.dll .\Analyzer.Tests\bin\Debug\net452\TestableFileSystem.Analyzer.Tests.dll -noshadow"
set coveragefile=".\coverage\TestableFileSystem.xml"

%opencover% -register:user -target:%testrunner% -targetargs:%targets% -excludebyattribute:*.ExcludeFromCodeCoverage* -hideskipped:All -output:%coveragefile%
%reportgenerator% -targetdir:.\coverage -reports:%coveragefile%

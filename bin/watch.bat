@echo %~dp0
@echo %CD%
@rem dotnet watch script --project "%~dp0watch.csproj" "%~dp0Program.cs" %CD%
dotnet watch run --no-hot-reload --project "%~dp0watch\\watch.csproj" -- %CD%
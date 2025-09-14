@echo off
REM -----------------------------------------------------
REM Script to install EF Core tools, add design package,
REM scaffold initial migration and update the database.
REM -----------------------------------------------------

:: 1. Install the EF CLI globally
dotnet tool install --global dotnet-ef
if ERRORLEVEL 1 (
    echo Failed to install dotnet-ef tool
    exit /b 1
)

:: 2. Add the EF Core Design package to the current project
dotnet add package Microsoft.EntityFrameworkCore.Design
if ERRORLEVEL 1 (
    echo Failed to add EF Core Design package
    exit /b 1
)

:: 3. Scaffold an initial migration
dotnet ef migrations add InitialCreate
if ERRORLEVEL 1 (
    echo Failed to create InitialCreate migration
    exit /b 1
)

:: 4. Apply the migration to the database
dotnet ef database update
if ERRORLEVEL 1 (
    echo Failed to update the database
    exit /b 1
)

echo.
echo ✔ All steps completed successfully.
pause

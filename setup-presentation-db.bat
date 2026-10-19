@echo off
echo ===================================================
echo  CommunityLink Presentation Database Setup
echo ===================================================
echo Applying presentation showcase seed data to CommunityLink database...
sqlcmd -S . -d CommunityLink -E -C -i "%~dp0CommunityLink.Database\Scripts\Seed_Presentation_Showcase_Data.sql"
if %ERRORLEVEL% equ 0 (
    echo.
    echo ===================================================
    echo  SUCCESS: Database seeded successfully!
    echo  All user and admin passwords: '123456789'
    echo ===================================================
) else (
    echo.
    echo ===================================================
    echo  ERROR: Failed to run script. Check your SQL Server connection.
    echo ===================================================
)
pause

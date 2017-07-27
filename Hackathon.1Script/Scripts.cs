namespace Hackathon._1Script
{
    public class Scripts
    {
        public static string WindowsScript = @"@echo off

setlocal enabledelayedexpansion
@echo Installing VSCode...
if not exist %temp%\vscodeSetup.exe (
    powershell -ExecutionPolicy Bypass -Command ""$wc = New-Object net.webclient; $wc.Downloadfile(\""https://go.microsoft.com/fwlink/?LinkID=623230\"", \""$env:temp\vscodeSetup.exe\"")""
    if errorlevel 1 GOTO ERROR

    :: install non-interactively & don't launch
    %temp%\vscodeSetup.exe /verysilent /mergetasks= !runcode
    if errorlevel 1 GOTO ERROR
)


:: dotnet cli download & install
@echo Installing dotnet cli...
if not exist %temp%\netcore20preview2.exe (
   @if not exist ""%programfiles(x86)%"" (
       powershell -ExecutionPolicy Bypass -Command ""$wc = New-Object net.webclient; $wc.Downloadfile(\""https://aka.ms/dotnet-sdk-2.0.0-preview2-win-x86\"", \""$env:temp\netcore20preview2.exe\"")""
    ) else (
        powershell -ExecutionPolicy Bypass -Command ""$wc = New-Object net.webclient; $wc.Downloadfile(\""https://aka.ms/dotnet-sdk-2.0.0-preview2-win-x64\"", \""$env:temp\netcore20preview2.exe\"")""
    )

    %temp%\netcore20preview2.exe
    if errorlevel 1 GOTO ERROR
)

:: Invoke the template
@echo Creating the mvc template...
dotnet new mvc -o ScriptDemo\MyFirstWebApp
if errorlevel 1 GOTO ERROR
cd ScriptDemo\MyFirstWebApp
dotnet restore
if errorlevel 1 GOTO ERROR
dotnet build
if errorlevel 1 GOTO ERROR

IF NOT EXIST Properties\PublishProfiles mkdir Properties\PublishProfiles
if errorlevel 1 GOTO ERROR

IF NOT EXIST Properties\PublishProfiles\Azure.pubxml (echo ^<Project^>
    echo    ^<PropertyGroup^>
    echo        ^<PublishProtocol^>Kudu^</PublishProtocol^>
    echo        ^<PublishSiteName^>$SiteName$^</PublishSiteName^>
    echo        ^<UserName^>$UserName$^</UserName^>
    echo        ^<Password^>$Password$^</Password^>
    echo    ^</PropertyGroup^>
    echo ^</Project^>
)>Properties\PublishProfiles\Azure.pubxml
if errorlevel 1 GOTO ERROR

dotnet publish /p:PublishProfile=Azure /p:Configuration=Release
if errorlevel 1 GOTO ERROR

start http://$SiteName$.azurewebsites.net
if errorlevel 1 GOTO ERROR

code .
@if not exist ""%programfiles(x86)%"" (
    ""%programfiles%\Microsoft VS Code\code.exe"" .
) else (
    ""%programfiles(x86)%\Microsoft VS Code\code.exe"" .
)

:ERROR
	endlocal
	exit /b 1
";

        public static string UnixScript = @"
# vscode download & install
echo Installing vsCode...
curl -L https://go.microsoft.com/fwlink/?LinkID=760868 > /tmp/vsCode-install.deb
if [[ $? -ne 0 ]]; then
	exit_on_error()
fi

sudo apt-get install /tmp/vsCode-install.deb
if [[ $? -ne 0 ]]; then
	exit_on_error()
fi


# dotnet cli download & install
echo ""***""
echo ""*** Installing dotnet cli""
echo ""***""
curl https://raw.githubusercontent.com/dotnet/cli/release/2.0.0-preview2/scripts/obtain/dotnet-install.sh > /tmp/dotnet-install.sh
if [[ $? -ne 0 ]]; then
	exit_on_error()
fi

chmod a+x /tmp/dotnet-install.sh
sudo /bin/bash /tmp/dotnet-install.sh
if [[ $? -ne 0 ]]; then
	exit_on_error()
fi

# Invoke the template
echo ""***""
echo ""Invoking the template""
echo ""***""
~/.dotnet/dotnet new mvc -o ~/ScriptDemo/MyFirstWebApp
if [[ $? -ne 0 ]]; then
	exit_on_error()
fi

cd ~/ScriptDemo/MyFirstWebApp
~/.dotnet/dotnet restore
if [[ $? -ne 0 ]]; then
	exit_on_error()
fi

~/.dotnet/dotnet build
if [[ $? -ne 0 ]]; then
	exit_on_error()
fi


if [ ! -e ""Properties/PublishProfiles"" ]; then
	mkdir -p Properties/PublishProfiles
fi

if [ ! -e ""Properties/PublishProfiles/Azure.pubxml"" ]; then
	(echo ""<Project>""
    	echo ""   <PropertyGroup>""
    	echo ""       <PublishProtocol>Kudu</PublishProtocol>""
    	echo ""       <PublishSiteName>$SiteName$</PublishSiteName>""
    	echo ""       <UserName>$UserName$</UserName>""
    	echo ""       <Password>$Password$</Password>""
    	echo ""   </PropertyGroup>""
    	echo ""</Project>""
	) > Properties/PublishProfiles/Azure.pubxml
fi
if [[ $? -ne 0 ]]; then
	exit_on_error()
fi

echo ""Publishing the project""
~/.dotnet/dotnet publish /p:PublishProfile=Azure /p:Configuration=Release

xdg-open http://$SiteName$.azurewebsites.net

code .

function exit_on_error
{
	local message = ""$2""
	local error_code = ""${3:-1}""
	if [[ -n ""$message"" ]]; then
		echo ""Error: ${message}; exiting with status ${code}""
	else
		echo ""Error: exiting with status ${code}""
	if
	exit ""${code}""
}
";
    }
}

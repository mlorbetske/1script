﻿namespace Hackathon._1Script
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

    SET ""PATH=%programfiles%\dotnet;%PATH%""
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

(echo ^<Project^>
    echo    ^<PropertyGroup^>
    echo        ^<PublishProtocol^>Kudu^</PublishProtocol^>
    echo        ^<PublishSiteName^>$SiteName$^</PublishSiteName^>
    echo        ^<UserName^>$UserName$^</UserName^>
    echo        ^<Password^>$Password$^</Password^>
    echo    ^</PropertyGroup^>
    echo ^</Project^>
)>Properties\PublishProfiles\Azure.pubxml
if errorlevel 1 GOTO ERROR

rem echo ""Making sure the site is not running...""
rem powershell -ExecutionPolicy Bypass -Command ""$headers = @{Authorization = \""Basic \"" + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes('$UserName$:$Password$'))}; $bodyString = @{ command=\""touch web.config\""; dir=\""site\\wwwroot\"" }; Invoke-RestMethod -Method POST -Uri https://$SiteName$.scm.azurewebsites.net/api/command -Body $bodyJson -Headers $headers -ContentType \""application/json\"" -UserAgent \""1Script\""""

echo Publishing the project...
dotnet publish /p:PublishProfile=Azure /p:Configuration=Release
if errorlevel 1 GOTO ERROR

start http://$SiteName$.azurewebsites.net
if errorlevel 1 GOTO ERROR

@if not exist ""%programfiles(x86)%"" (
    ""%programfiles%\Microsoft VS Code\code.exe"" .
) else (
    ""%programfiles(x86)%\Microsoft VS Code\code.exe"" .
)

:ERROR
	endlocal
	exit /b 1
";

        public static string MacOsScript = @"#!/bin/bash
# vscode download & install

function exit_on_error
{
	local message = """"$2""""
	local error_code = """"${3:-1}""""
	if [[ -n """"$message"""" ]]; then
		echo """"Error: ${message}; exiting with status ${error_code}""""
	else
		echo """"Error: exiting with status ${error_code}""""
	fi
	exit """"${error_code}"""";
}

echo Installing vsCode...
if [ ! -e ~/Downloads/Visual\ Studio\ Code.app ]; then
open https://go.microsoft.com/fwlink/?LinkID=620882
if [[ $? -ne 0 ]]; then
	exit_on_error
fi

fi

# dotnet cli download & install
echo Installing dotnet cli...
if [ ! -e ~/Downloads/dotnet-sdk-2.0.0-preview2-006497-osx-x64.pkg ]; then
curl -L -o ~/Downloads/dotnet-sdk-2.0.0-preview2-006497-osx-x64.pkg https://aka.ms/dotnet-sdk-2.0.0-preview2-osx-x64
sudo installer -allowUntrusted -verboseR -pkg ~/Downloads/dotnet-sdk-2.0.0-preview2-006497-osx-x64.pkg -target /
fi

# Invoke the template
echo Invoking the template...
dotnet new mvc -o ~/ScriptDemo/MyFirstWebApp

cd ~/ScriptDemo/MyFirstWebApp
dotnet restore
if [[ $? -ne 0 ]]; then
	exit_on_error
fi

dotnet build
if [[ $? -ne 0 ]]; then
	exit_on_error
fi


if [ ! -e Properties/PublishProfiles ]; then
	mkdir -p Properties/PublishProfiles
fi

(echo \<Project\>
    echo    \<PropertyGroup\>
    echo        \<PublishProtocol\>Kudu\</PublishProtocol\>
    echo        \<PublishSiteName\>$SiteName$\</PublishSiteName\>
    echo        \<UserName\>\$UserName$\</UserName\>
    echo        \<Password\>$Password$\</Password\>
    echo    \</PropertyGroup\>
    echo \</Project\>
) > Properties/PublishProfiles/Azure.pubxml

if [[ $? -ne 0 ]]; then
	exit_on_error
fi

#echo Making sure the site is not running...
#curl -X POST -H ""Content-Type=application/json"" --data ""{command: 'touch web.config', dir: 'site\\\\wwwroot'}"" -u \$UserName$:$Password$ https://$SiteName$.scm.azurewebsites.net/api/command

echo Publishing the project...
dotnet publish /p:PublishProfile=Azure /p:Configuration=Release

open http://$SiteName$.azurewebsites.net

open ~/Downloads/Visual\ Studio\ Code.app/ .
";

        public static string UnixScript = @"#!/bin/bash

function exit_on_error
{
	local message=""$2""
	local error_code=""${3:-1}""
	if [[ -n ""$message"" ]]; then
		echo ""Error: ${message}; exiting with status ${error_code}""
	else
		echo ""Error: exiting with status ${error_code}""
	fi
	exit ""${error_code}"";
}

# vscode download & install
echo ""Installing vsCode...""
xdg-open https://go.microsoft.com/fwlink/?LinkID=760868
if [[ $? -ne 0 ]]; then
	exit_on_error
fi

# dotnet cli download & install
echo ""Installing dotnet cli...""
curl https://raw.githubusercontent.com/dotnet/cli/release/2.0.0-preview2/scripts/obtain/dotnet-install.sh > /tmp/dotnet-install.sh
if [[ $? -ne 0 ]]; then
	exit_on_error
fi

chmod a+x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh -i ~/bin/ -v 2.0.0-preview2-006497
if [[ $? -ne 0 ]]; then
	exit_on_error
fi

# Invoke the template
echo ""Invoking the template...""
~/bin/dotnet new mvc -o ~/ScriptDemo/MyFirstWebApp
if [[ $? -ne 0 ]]; then
	exit_on_error
fi

cd ~/ScriptDemo/MyFirstWebApp
~/bin/dotnet restore
if [[ $? -ne 0 ]]; then
	exit_on_error
fi

~/bin/dotnet build
if [[ $? -ne 0 ]]; then
	exit_on_error
fi

if [ ! -e ""Properties/PublishProfiles"" ]; then
	mkdir -p Properties/PublishProfiles
fi

(echo ""<Project>""
    echo ""   <PropertyGroup>""
    echo ""       <PublishProtocol>Kudu</PublishProtocol>""
    echo ""       <Configuration>Release</Configuration>""
    echo ""       <PublishSiteName>$SiteName$</PublishSiteName>""
    echo ""       <UserName>\$UserName$</UserName>""
    echo ""       <Password>$Password$</Password>""
    echo ""   </PropertyGroup>""
    echo ""</Project>""
) > Properties/PublishProfiles/Azure.pubxml

if [[ $? -ne 0 ]]; then
	exit_on_error
fi

#echo ""Making sure the site is not running...""
#curl -X POST -H ""Content-Type=application/json"" --data ""{command: 'touch web.config', dir: 'site\\\\wwwroot'}"" -u \$UserName$:$Password$ https://$SiteName$.scm.azurewebsites.net/api/command

echo ""Publishing the project...""
~/bin/dotnet publish /p:PublishProfile=Azure /p:Configuration=Release

xdg-open http://$SiteName$.azurewebsites.net
code ~/ScriptDemo/MyFirstWebApp/
";
    }
}

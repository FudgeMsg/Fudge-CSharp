@echo off

rem Copyright 2009 by OpenGamma Inc and other contributors.
rem
rem Licensed under the Apache License, Version 2.0 (the "License");
rem you may not use this file except in compliance with the License.
rem You may obtain a copy of the License at
rem 
rem     http://www.apache.org/licenses/LICENSE-2.0
rem     
rem Unless required by applicable law or agreed to in writing, software
rem distributed under the License is distributed on an "AS IS" BASIS,
rem WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
rem See the License for the specific language governing permissions and
rem limitations under the License.

rem ---------------------------------------------------------------------------
rem Library documentation builder. You will need Sandcastle and the
rem "HTML Help Workshop" installed to run this.
rem ---------------------------------------------------------------------------

setlocal

rem Set default build below. Debug at the moment but maybe Release would be a
rem more sensible default?

set CONFIGURATION=Debug
set PROJECT=Fudge
set SANDCASTLE=%DXROOT%
set HHC="%DXROOT%\..\HTML Help Workshop\hhc.exe"
set PATH=%DXROOT%\ProductionTools;%PATH%

rem Customise a run by giving parameters

if "%1" == "" goto skip1
set CONFIGURATION=%1
:skip1

rem Here we go

set WORKINGDIR=..\..\obj\%CONFIGURATION%
cd bin\%CONFIGURATION%
mrefbuilder %PROJECT%.DLL /out:%WORKINGDIR%\reflection.org
xsltransform /xsl:"%SANDCASTLE%ProductionTransforms\ApplyVSDocModel.xsl" %WORKINGDIR%\reflection.org /xsl:"%SANDCASTLE%ProductionTransforms\AddFriendlyFilenames.xsl" /out:reflection.xml
xslTransform /xsl:"%SANDCASTLE%ProductionTransforms\ReflectionToManifest.xsl" reflection.xml /out:manifest.xml
call "%SANDCASTLE%Presentation\vs2005\copyOutput.bat"
buildassembler /config:"..\..\docs\sandcastle.config" manifest.xml > %WORKINGDIR%\sandcastle.log
find "Warn: ShowMissingComponent:" %WORKINGDIR%\sandcastle.log > %WORKINGDIR%\missing.log
xsltransform /xsl:"%SANDCASTLE%ProductionTransforms\ReflectionToChmProject.xsl" /arg:project=%PROJECT% reflection.xml /out:Output\%PROJECT%.hhp
xsltransform /xsl:"%SANDCASTLE%ProductionTransforms\createvstoc.xsl" reflection.xml /out:%WORKINGDIR%\toc.xml
xsltransform /xsl:"%SANDCASTLE%ProductionTransforms\TocToChmContents.xsl" %WORKINGDIR%\toc.xml /out:Output\%PROJECT%.hhc
xsltransform /xsl:"%SANDCASTLE%ProductionTransforms\ReflectionToChmIndex.xsl" reflection.xml /out:Output\%PROJECT%.hhk
cd Output
%HHC% %PROJECT%.hhp
copy /Y %PROJECT%.chm ..\..\..\docs

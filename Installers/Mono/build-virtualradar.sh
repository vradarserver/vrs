#!/bin/bash
VER=$1
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
WORK=$DIR/work-virtual-radar
ROOT=$DIR/../..
BUILD=$ROOT/VirtualRadar/bin/x86/Release
OUTPUT=$DIR/output

if [ -d $WORK ]; then
  rm -r $WORK;
fi
if [ ! -d $OUTPUT ]; then
  mkdir $OUTPUT;
fi
mkdir $WORK
mkdir $WORK/de-DE
mkdir $WORK/fr-FR
mkdir $WORK/pt-BR
mkdir $WORK/ru-RU
mkdir $WORK/zh-CN

cp    $BUILD/AWhewell.Owin.dll $WORK
cp    $BUILD/AWhewell.Owin.Host.HttpListener.dll $WORK
cp    $BUILD/AWhewell.Owin.Interface.dll $WORK
cp    $BUILD/AWhewell.Owin.Interface.Host.HttpListener.dll $WORK
cp    $BUILD/AWhewell.Owin.Interface.WebApi.dll $WORK
cp    $BUILD/AWhewell.Owin.Utility.dll $WORK
cp    $BUILD/AWhewell.Owin.WebApi.dll $WORK
cp    $BUILD/InterfaceFactory.dll $WORK
cp    $BUILD/Interop.NATUPNPLib.dll $WORK
cp    $BUILD/Microsoft.FlightSimulator.SimConnect.dll $WORK
cp    $BUILD/VirtualRadar.Database.dll $WORK
cp    $BUILD/VirtualRadar.exe $WORK
cp    $BUILD/VirtualRadar.Headless.dll $WORK
cp    $BUILD/VirtualRadar.Interface.dll $WORK
cp    $BUILD/VirtualRadar.Interop.dll $WORK
cp    $BUILD/VirtualRadar.Library.dll $WORK
cp    $BUILD/VirtualRadar.Localisation.dll $WORK
cp    $BUILD/VirtualRadar.Resources.dll $WORK
cp    $BUILD/VirtualRadar.WebServer.dll $WORK
cp    $BUILD/VirtualRadar.WebServer.HttpListener.dll $WORK
cp    $BUILD/VirtualRadar.WebSite.dll $WORK
cp    $BUILD/VirtualRadar.WinForms.dll $WORK
cp    $BUILD/AjaxMin.dll $WORK
cp    $BUILD/Dapper.dll $WORK
cp    $BUILD/HtmlAgilityPack.dll $WORK
cp    $BUILD/KdTreeLib.dll $WORK
cp    $BUILD/NewtonSoft.Json.dll $WORK
cp    $BUILD/BaseStationImport.exe $WORK
cp    $BUILD/BaseStationImport.exe.config $WORK
cp    $BUILD/Checksums.txt $WORK
cp -r $BUILD/Web $WORK
cp    $ROOT/SQLiteWrapper.Mono/bin/Release/VirtualRadar.SQLiteWrapper.dll $WORK
cp -t $WORK/de-DE $BUILD/de-DE/VirtualRadar.WebSite.resources.dll
cp -t $WORK/fr-FR $BUILD/fr-FR/VirtualRadar.WebSite.resources.dll
cp -t $WORK/pt-BR $BUILD/pt-BR/VirtualRadar.WebSite.resources.dll
cp -t $WORK/ru-RU $BUILD/ru-RU/VirtualRadar.WebSite.resources.dll
cp -t $WORK/zh-CN $BUILD/zh-CN/VirtualRadar.WebSite.resources.dll
rm $WORK/Web/zz-norel-*
cd $WORK
tar -czf     $OUTPUT/VirtualRadar-mono-$VER.tar.gz *
echo Created $OUTPUT/VirtualRadar-mono-$VER.tar.gz


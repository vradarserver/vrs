#!/bin/bash
VER=$1
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
WORK=$DIR/work-language-pack
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

cp -t $WORK/de-DE $BUILD/de-DE/*
cp -t $WORK/fr-FR $BUILD/fr-FR/*
cp -t $WORK/pt-BR $BUILD/pt-BR/*
cp -t $WORK/ru-RU $BUILD/ru-RU/*
cp -t $WORK/zh-CN $BUILD/zh-CN/*
cd $WORK
tar -czf     $OUTPUT/LanguagePack-Linux-$VER.tar.gz *
echo Created $OUTPUT/LanguagePack-Linux-$VER.tar.gz


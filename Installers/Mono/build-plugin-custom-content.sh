#!/bin/bash
VER=$1
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
ROOT=$DIR/../..
OUTPUT=$DIR/output

WORKROOT=$DIR/work-plugin-custom-content
WORK=$WORKROOT/Plugins/CustomContent
BUILD=$ROOT/VirtualRadar/bin/x86/Release/Plugins/CustomContent

if [ -d $WORKROOT ]; then
  rm -r $WORKROOT;
fi
if [ ! -d $OUTPUT ]; then
  mkdir $OUTPUT;
fi
mkdir -p $WORK

cp -r $BUILD/* $WORK
cd $WORKROOT
tar -czf     $OUTPUT/Plugin-Linux-CustomContent-$VER.tar.gz *
echo Created $OUTPUT/Plugin-Linux-CustomContent-$VER.tar.gz


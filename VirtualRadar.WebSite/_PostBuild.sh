#!/bin/bash
exit 0

export SOURCE=$1
export DEST=$2
echo "Source is $SOURCE"
echo "Dest is $DEST"

if [ ! -d "$SOURCE" ]; then
    echo "FAILED: The source folder '$SOURCE' does not exist."
    exit 1
else
    if [ -d "$DEST" ]; then
        echo "Erasing '$DEST'"
        rm -rf "$DEST"
        if [ $? -ne 0 ]; then
            echo "Could not remove the '$DEST' folder, error level is $?"
            exit 1
        fi
    fi

    echo "Copying '$SOURCE' to '$DEST'"
    cp -fR "$SOURCE" "$DEST"
    if [ $? -ne 0 ]; then
        echo "Could not copy from '$SOURCE' to '$DEST', error level is $?"
        exit 1
    fi

    echo "Copied web site output to VirtualRadar build folder"
fi

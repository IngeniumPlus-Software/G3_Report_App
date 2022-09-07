#!/bin/bash
  
TARGET_FOLDER="/var/www/api"

if [ -e "$TARGET_FOLDER" ]; then
    rm -rf ${TARGET_FOLDER}.bak
    mv -v ${TARGET_FOLDER} ${TARGET_FOLDER}.bak
fi

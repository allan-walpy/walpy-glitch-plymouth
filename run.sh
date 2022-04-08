#!/bin/bash

Color='\033[0;33m'
NoColor='\033[0m'

echo -e "${Color}plymouth://process/start${NoColor}"
plymouthd --no-daemon --debug >daemon.log 2>&1 &
sleep 1
echo -e "${Color}plymouth://process/show ${NoColor}"
plymouth --ping &&
plymouth --show-splash &&
./test || true

plymouth --quit
echo -e "${Color}plymouth://done ${NoColor}"

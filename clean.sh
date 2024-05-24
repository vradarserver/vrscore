#!/bin/bash

SHDIR="$(cd "$(dirname "$0")" && pwd)"

find "$SHDIR" -type d \( -name "bin" -o -name "obj" \) -exec rm -r {} +
find "$SHDIR/DevOps" -type d \( -name "Distributables" \) -exec rm -r {} +

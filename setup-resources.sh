#!/bin/sh

for name in $(find resource -name '*.yml'); do
    target="$(dirname "$name")/$(basename "$name" .yml).json"
    yq -o=json "." "$name" > "$target"
done
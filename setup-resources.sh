#!/bin/sh

for name in resource/**/*.yml; do
    target="$(dirname "$name")/$(basename "$name" .yml).json"
    yq -o=json "." "$name" > "$target"
done
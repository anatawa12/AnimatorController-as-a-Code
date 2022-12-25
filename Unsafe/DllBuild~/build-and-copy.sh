#!/bin/sh

set -eu

cd "$(dirname "$0")"

# ilasm --version
# Mono IL assembler compiler version 6.12.0.182

ilasm -dll -output="../com.anatawa12.animator-controller-as-a-code.unsafe.dll" Anatawa12.AnimatorControllerAsACode.Unsafe.il 

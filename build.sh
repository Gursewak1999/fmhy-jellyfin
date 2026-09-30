#!/bin/bash

# Build script for FMHY Free Movies Jellyfin Plugin

set -e

PROJECT_DIR="FmhyPlugin"
OUTPUT_DIR="output"
PLUGIN_NAME="FmhyPlugin"

echo "=== FMHY Free Movies Plugin Build Script ==="
echo ""

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf "$OUTPUT_DIR"
rm -rf "$PROJECT_DIR/bin"
rm -rf "$PROJECT_DIR/obj"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Restore dependencies
echo "Restoring dependencies..."
cd "$PROJECT_DIR"
dotnet restore

# Build release
echo "Building release..."
dotnet build -c Release --no-restore

# Create plugin package
echo "Creating plugin package..."
cd ..
mkdir -p "$OUTPUT_DIR/$PLUGIN_NAME"

# Copy DLL
cp "$PROJECT_DIR/bin/Release/net8.0/$PLUGIN_NAME.dll" "$OUTPUT_DIR/$PLUGIN_NAME/"

# Copy plugin.json
cp "$PROJECT_DIR/plugin.json" "$OUTPUT_DIR/$PLUGIN_NAME/"

# Copy web files
mkdir -p "$OUTPUT_DIR/$PLUGIN_NAME/Web"
cp "$PROJECT_DIR/Web/configuration.html" "$OUTPUT_DIR/$PLUGIN_NAME/Web/"
cp "$PROJECT_DIR/Web/browse.html" "$OUTPUT_DIR/$PLUGIN_NAME/Web/"

# Create zip
echo "Creating zip package..."
cd "$OUTPUT_DIR"
zip -r "${PLUGIN_NAME}.zip" "$PLUGIN_NAME"
cd ..

echo ""
echo "=== Build Complete ==="
echo "Plugin package: $OUTPUT_DIR/${PLUGIN_NAME}.zip"
echo "Extracted plugin: $OUTPUT_DIR/$PLUGIN_NAME/"
echo ""
echo "To install:"
echo "1. Extract $OUTPUT_DIR/${PLUGIN_NAME}.zip to your Jellyfin plugins directory"
echo "2. Restart Jellyfin"
echo "3. Configure at Dashboard → Plugins → FMHY Free Movies"
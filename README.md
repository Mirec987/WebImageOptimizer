# ImageOptimizer
A .NET tool for batch image processing, converting to optimized JPEGs and creating thumbnails for web performance. Configurable via JSON.

A simple image batch processing tool built with .NET and ImageSharp. The tool allows you to process multiple images at once, resizing them, adjusting their quality, and creating thumbnails. Configurable through a JSON file, it provides a flexible way to automate image optimizations.

## Features

- **Batch Processing**: Process multiple images at once.
- **Configurable**: Configure input/output directories, JPEG quality, and thumbnail dimensions via a JSON file.
- **Image Optimization**: Automatically skips images with already optimized quality.
- **Thumbnail Creation**: Generate thumbnails with specified dimensions.

## Requirements

- .NET 6.0 or later
- SixLabors.ImageSharp

## Configuration

Edit the `config.json` file in the root directory to set your preferences.

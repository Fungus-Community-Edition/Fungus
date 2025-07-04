# Changelog

All notable changes to this project will be documented in this file.

## 2025-07-01
- Renamed folder and namespace from `Fungus` to `Amanita` across editor scripts and assets.
- Reorganized legacy scripts so it's easier to cut them out of the main package later

## 2025-07-04

### ✨ Features
- Introduced 1-second post-startup delay before freezing core module configurations
  - Facilitates early customization of metadata creators, serializers, and caching strategies
- Upgraded slot metadata system to support modular, pluggable creators
  - Enables designer-defined presentation formats (e.g., Roman numerals, custom icons, contextual details)
- Added support for custom tweening via a homegrown library
  - Strategy Pattern ensures extensibility for different animation styles
- Improved GitBook documentation to reflect new plugin architecture and DI setup
  - Includes designer-friendly walkthroughs for configuring save metadata and presentation

### 🔧 Refactors
- Refactored save system architecture to leverage Dependency Injection (DI)
- Replaced service lookups and manual configuration with constructor injection
- Decoupled SaveManager responsibilities into focused, injectable services
- File handling, slot formatting, and metadata generation now modular and testable
- Promotes engine-agnostic design and simplifies future portability
- Introduced fail-fast checks to support robust validation and debugging
- Clear error-logging for misconfigured metadata pipelines or serialization logic
- Enhanced testability via mockable interfaces and separation of concerns
- Facilitates unit testing and debugging of edge cases across save workflows

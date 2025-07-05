# Changelog

All notable changes to this project will be documented in this file.

## 2025-07-01
- Renamed folder and namespace from `Fungus` to `Amanita` across editor scripts and assets.
- Reorganized legacy scripts so it's easier to cut them out of the main package later

## 2025-07-04
About the save sys
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

## 2025-07-05
Changes to other systems for the sake of the save sys
- GameStarted Blocks no longer execute on their own; their parent Flowcharts are what decide when they execute
- The Singleton getters no longer create instances when accessed
  - Turns out that when you try to access a Singleton through the debugger's Watch list, it executes the creation logic (if its in the getter). This can cause quite a few issues during testing
- Flowcharts are now what make sure that there's an AmanitaManager in all scenes they exist in
- Created a prefab for the main Myceliaudio game object. It is now part of the AmanitaManager prefab
- Most of these changes were to solve issues with Singleton state persisting between unit tests
- Refactored a lot of the unit tests so that no the singletons are created and assigned during SetUp.
  - As opposed to how they were before, with OneTimeSetUp just instantiating the prefabs
- AmanitaManager's submodules no longer have Awake or Start methods. They have Init methods so that AmanitaManager can control when they're prepped
- Removed the ghost-object check in AmanitaManager since improvements in Unity have rendered it unnecessary
  - That, and it's apparently bad practice to have singleton getters create or detect instances when there are none

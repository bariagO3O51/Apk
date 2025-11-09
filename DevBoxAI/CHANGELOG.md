# Changelog

Alle nennenswerten Änderungen an DevBoxAI werden in dieser Datei dokumentiert.

Das Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.0.0/),
und dieses Projekt folgt [Semantic Versioning](https://semver.org/lang/de/).

## [Unreleased]

## [1.0.0] - 2025-11-08

### Added

#### Core Features
- **Chat-basierte App-Entwicklung**: Natürlichsprachliche Eingabe für Android-App-Generierung
- **AI-Integration**: Claude-3.5-Sonnet für Code-Generierung
- **Live-Vorschau**: Device Frames (Pixel 7, Pixel 7 Pro, Samsung S23, Tablets)
- **Code-Editor**: Syntax-Highlighting für Kotlin, XML, Gradle
- **Build-System**: Gradle-Integration für APK/AAB-Erstellung
- **ADB-Integration**: Direkte Installation auf verbundenen Geräten

#### UI/UX
- Material Design 3 WPF-Interface
- Responsive Layout mit 3-Panel-Design:
  - Chat-Panel (links)
  - Code-Editor/Preview (Mitte)
  - File Tree (rechts)
- Dark/Light Theme Support
- Status Bar mit Build-Progress
- Toolbar mit Device-Selection

#### Code-Generierung
- **Architektur-Patterns**:
  - MVVM (Model-View-ViewModel)
  - MVI (Model-View-Intent)
- **Dependency Injection**: Hilt/Koin Support
- **Database**: Room/SQLDelight Integration
- **Networking**: Retrofit/OkHttp oder Ktor
- **UI-Framework**: Jetpack Compose mit Material 3

#### Integrationen
- Firebase (Auth, Firestore, Analytics, Crashlytics)
- Supabase
- REST/GraphQL APIs
- Stripe Payments
- Google Maps
- Push Notifications

#### Project Management
- Projekt-Erstellung aus Chat-Beschreibung
- Projekt-Speicherung und -Laden
- Recent Projects Liste
- Git-Repository-Integration (Vorbereitung)

#### Build & Deploy
- Debug- und Release-Builds
- APK/AAB-Export
- Code-Signierung (Keystore-Verwaltung vorbereitet)
- ProGuard/R8 Minification
- ADB-Device-Detection

### Technical
- **.NET 8.0**: Neueste LTS-Version
- **WPF**: Windows Presentation Foundation
- **Material Design**: MaterialDesignThemes.Wpf
- **MVVM**: CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Modular Architecture**: 4 separate Projekte (App, Core, AI, AndroidGenerator)

### Documentation
- Umfassendes README.md mit Quickstart
- Entwickler-Dokumentation (DEVELOPERS.md)
- Build-Scripts (PowerShell & Bash)
- Code-Kommentare und XML-Docs

## [0.9.0] - 2025-10-XX (Beta)

### Added
- Alpha-Version mit Basis-Funktionalität
- Proof-of-Concept für AI-Generierung
- Basic WPF UI

## [0.1.0] - 2025-09-XX (Alpha)

### Added
- Initialer Prototyp
- Grundlegende Projektstruktur

---

## Legende

- `Added`: Neue Features
- `Changed`: Änderungen an existierenden Features
- `Deprecated`: Bald zu entfernende Features
- `Removed`: Entfernte Features
- `Fixed`: Bug-Fixes
- `Security`: Sicherheits-Fixes

[Unreleased]: https://github.com/yourusername/devboxai/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/yourusername/devboxai/releases/tag/v1.0.0
[0.9.0]: https://github.com/yourusername/devboxai/releases/tag/v0.9.0
[0.1.0]: https://github.com/yourusername/devboxai/releases/tag/v0.1.0

# DevBoxAI - Windows Desktop App für Android-App-Generierung

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows-blue)

## 🚀 Überblick

**DevBoxAI** ist eine revolutionäre Windows-Desktop-Anwendung, die es ermöglicht, vollständige Android-Apps durch natürlichsprachliche Chat-Eingaben zu generieren. Powered by Anthropic Claude AI.

### ✨ Hauptmerkmale

- 💬 **Chat-basierte Entwicklung**: Beschreibe deine App-Idee, DevBoxAI generiert den Code
- 🎨 **Live-Vorschau**: Flutter-ähnliche Echtzeit-Darstellung mit Device Frames
- 🏗️ **Vollständiger Tech-Stack**: Kotlin, Jetpack Compose, MVVM, Hilt, Room, Retrofit
- 📦 **Build & Deploy**: Signierte APK/AAB direkt für Google Play Store
- 🔌 **Integrationen**: Firebase, Supabase, Stripe, Maps, Analytics

## 📁 Repository-Struktur

```
/
├── DevBoxAI/              ← Haupt-Projekt (Windows Desktop App)
│   ├── src/
│   ├── docs/
│   ├── build.ps1
│   └── README.md         ← Detaillierte Dokumentation
│
├── README.txt            ← Original Android Studio Projekt Info
└── TeamsPartFinderProject.zip
```

## 🎯 Quick Start

### 1. Öffne das DevBoxAI-Projekt

```bash
cd DevBoxAI
```

### 2. Build & Run

**Windows (PowerShell):**

```powershell
# Kompilieren
.\build.ps1 -Configuration Release

# Ausführen
.\build\publish\DevBoxAI.exe
```

**Cross-Platform (Linux/Mac → Windows Build):**

```bash
chmod +x build.sh
./build.sh --configuration Release
```

### 3. API-Key konfigurieren

```powershell
# Anthropic Claude API-Key setzen
$env:ANTHROPIC_API_KEY = "sk-ant-api03-YOUR-KEY-HERE"
```

Hol dir einen API-Key: https://www.anthropic.com

## 📚 Dokumentation

Vollständige Dokumentation findest du im DevBoxAI-Unterordner:

- **[DevBoxAI/README.md](DevBoxAI/README.md)** - Umfassende Projekt-Dokumentation
- **[DevBoxAI/docs/QUICKSTART.md](DevBoxAI/docs/QUICKSTART.md)** - 5-Minuten-Tutorial
- **[DevBoxAI/docs/DEVELOPERS.md](DevBoxAI/docs/DEVELOPERS.md)** - Entwickler-Guide
- **[DevBoxAI/CHANGELOG.md](DevBoxAI/CHANGELOG.md)** - Versionshistorie

## 🎬 Demo

### Beispiel: ToDo-App generieren

```
User: Erstelle eine ToDo-App mit:
      - Liste aller Aufgaben
      - Hinzufügen-Button
      - Checkbox zum Abhaken
      - Swipe-to-Delete
      - Room Database

DevBoxAI: ✅ Projekt erstellt!
          ✅ 12 Dateien generiert
          ✅ Vorschau verfügbar
          ✅ Build-ready
```

**Ergebnis:** Vollständige, lauffähige Android-App in ~30 Sekunden

## 🛠️ Tech Stack

### Desktop-App (DevBoxAI)

- **.NET 8.0** - Framework
- **WPF** - UI Framework
- **Material Design** - UI Library
- **MVVM** - Architecture Pattern
- **Anthropic Claude** - AI Code Generation

### Generierte Android-Apps

- **Kotlin** - Sprache
- **Jetpack Compose** - UI Framework
- **Material 3** - Design System
- **Hilt** - Dependency Injection
- **Room** - Local Database
- **Retrofit** - Networking
- **Coroutines** - Async Operations

## 🏗️ Architektur

```
DevBoxAI (Windows Desktop App)
├── DevBoxAI (WPF UI)
├── DevBoxAI.Core (Models & Interfaces)
├── DevBoxAI.AI (Claude Integration)
└── DevBoxAI.AndroidGenerator (Code Generation)

Generated Android Project
├── app/
│   ├── src/main/java/.../
│   │   ├── ui/ (Jetpack Compose)
│   │   ├── data/ (Repository, Room, API)
│   │   ├── domain/ (Business Logic)
│   │   └── di/ (Hilt Modules)
│   └── build.gradle.kts
└── settings.gradle.kts
```

## 🔧 Entwicklung

### Voraussetzungen

- Visual Studio 2022 oder höher
- .NET 8.0 SDK
- Windows 10/11 (64-bit)
- Android SDK (optional, für Builds)

### Projekt kompilieren

```bash
cd DevBoxAI
dotnet restore
dotnet build --configuration Release
```

### Tests ausführen (TODO)

```bash
dotnet test
```

## 🤝 Beitragen

Contributions sind willkommen! Siehe [CONTRIBUTING.md](DevBoxAI/CONTRIBUTING.md) für Details.

1. Fork das Repository
2. Feature-Branch erstellen (`git checkout -b feature/AmazingFeature`)
3. Änderungen committen (`git commit -m 'Add AmazingFeature'`)
4. Branch pushen (`git push origin feature/AmazingFeature`)
5. Pull Request erstellen

## 📄 Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert - siehe [LICENSE](DevBoxAI/LICENSE) für Details.

## 🙏 Credits

- **Anthropic Claude** - AI-powered Code Generation
- **Material Design Team** - Design System
- **Microsoft** - .NET & WPF Framework
- **Android Team** - Android SDK & Jetpack

## 📞 Support & Community

- **Issues**: [GitHub Issues](https://github.com/bariagO3O51/Apk/issues)
- **Discussions**: [GitHub Discussions](https://github.com/bariagO3O51/Apk/discussions)
- **Email**: support@devboxai.com

## 🗺️ Roadmap

### Version 1.1 (Q1 2025)
- [ ] iOS-App-Generierung (SwiftUI)
- [ ] Flutter-App-Generierung
- [ ] Visual Design-Editor
- [ ] Template-Marketplace

### Version 1.2 (Q2 2025)
- [ ] Cloud-Sync
- [ ] Team-Kollaboration
- [ ] CI/CD-Integration
- [ ] Plugin-System

### Version 2.0 (Q3 2025)
- [ ] Multi-Platform (macOS, Linux)
- [ ] Web-App-Generierung
- [ ] Voice-Input
- [ ] Design-Import (Figma, Sketch)

## 📊 Projekt-Status

![GitHub stars](https://img.shields.io/github/stars/bariagO3O51/Apk?style=social)
![GitHub forks](https://img.shields.io/github/forks/bariagO3O51/Apk?style=social)
![GitHub watchers](https://img.shields.io/github/watchers/bariagO3O51/Apk?style=social)

---

**DevBoxAI** - Von der Idee zur Android-App in Minuten! 🚀

Made with ❤️ for Developers | Powered by Claude AI

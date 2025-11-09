# DevBoxAI — Windows-Desktop-App für Android-Apps aus Chat-Eingaben

![DevBoxAI Logo](https://via.placeholder.com/800x200/673AB7/FFFFFF?text=DevBoxAI+-+Android+App+Generator)

**DevBoxAI** ist eine leistungsstarke Windows-Desktop-Anwendung, die Android-Apps direkt aus natürlichsprachlichen Anforderungen generiert. Mit KI-gestützter Code-Generierung, Live-Vorschau und vollständiger Build-Integration von der Idee bis zur signierten APK.

## 🚀 Kernfunktionen

### Von Idee zu Build ohne IDE-Overhead

- **Chat-basierte Entwicklung**: Beschreibe deine App-Idee in natürlicher Sprache
- **Vollständige Code-Generierung**: Kompletter Tech-Stack mit Kotlin, Jetpack Compose, MVVM/MVI
- **Live-Vorschau**: Flutter-ähnliche Echtzeit-Darstellung mit Device Frames
- **Sofort lauffähig**: Signierte APK/AAB direkt exportierbar für Google Play

### Was DevBoxAI generiert

- **UI**: Material 3 Design, responsiv, Barrierefreiheit, Dark/Light Theme
- **Architektur**: MVVM oder MVI mit Hilt Dependency Injection
- **Netzwerk**: Retrofit/OkHttp oder Ktor mit Auth-Flows (OAuth2, Firebase)
- **Datenbank**: Room/SQLDelight mit Migrations und Repository-Pattern
- **Tests**: Unit-Tests, Instrumentation-Tests, UI-Tests mit Espresso
- **Assets**: Icons, Splash-Screen, Adaptive App-Icon, Lokalisierung

## 📋 Systemanforderungen

### Minimale Anforderungen

- **OS**: Windows 10/11 (64-bit)
- **RAM**: 8 GB (empfohlen: 16 GB)
- **Speicher**: 10 GB frei (30 GB empfohlen mit Android SDK)
- **.NET**: .NET 8.0 Runtime
- **Android SDK**: Optional (kann durch Setup-Assistent installiert werden)

### Für vollständige Build-Funktionalität

- **Android SDK**: API Level 24-34
- **JDK**: Version 17 oder höher
- **Gradle**: 8.0+ (wird automatisch heruntergeladen)
- **ADB**: Für Device-Deployment

## 🔧 Installation

### Option 1: Vorkompilierte DevBoxAI.exe

1. Lade die neueste Version von [Releases](https://github.com/yourusername/devboxai/releases)
2. Entpacke das Archiv
3. Führe `DevBoxAI.exe` aus

### Option 2: Aus Quellcode kompilieren

#### Voraussetzungen

- Visual Studio 2022 oder höher
- .NET 8.0 SDK
- Git

#### Build-Schritte

```bash
# Repository klonen
git clone https://github.com/yourusername/devboxai.git
cd devboxai/DevBoxAI

# NuGet-Pakete wiederherstellen und kompilieren
dotnet restore
dotnet build --configuration Release

# Oder PowerShell Build-Script verwenden
.\build.ps1 -Configuration Release

# Die DevBoxAI.exe findest du in: .\build\publish\
```

## 🎯 Schnellstart

### 1. API-Key konfigurieren

DevBoxAI verwendet Anthropic Claude für die Code-Generierung. Du benötigst einen API-Key:

1. Registriere dich bei [Anthropic](https://www.anthropic.com)
2. Erstelle einen API-Key
3. Setze die Umgebungsvariable:
   ```powershell
   $env:ANTHROPIC_API_KEY = "dein-api-key"
   ```

### 2. DevBoxAI starten

```bash
DevBoxAI.exe
```

### 3. Erste App erstellen

Gib im Chat ein:

```
Erstelle eine ToDo-App mit:
- Login mit Email und Passwort
- Liste aller ToDos mit Checkbox
- Hinzufügen neuer ToDos
- Cloud-Synchronisierung mit Firebase
- Dark Mode
```

DevBoxAI wird:
1. Projekt-Struktur erstellen
2. Alle Kotlin-Dateien generieren
3. UI-Layouts mit Material 3 erstellen
4. Firebase-Integration konfigurieren
5. Build-Dateien (Gradle) generieren

## 💡 Verwendungsbeispiele

### Beispiel 1: E-Commerce App

```
Baue eine E-Commerce-App mit:
- Produktkatalog mit Kategorien
- Warenkorb mit Mengenauswahl
- Stripe-Zahlungsintegration
- Bestellhistorie
- Push-Benachrichtigungen
```

### Beispiel 2: Fitness-Tracker

```
Generiere eine Fitness-Tracker-App:
- Schrittzähler mit Sensor-Integration
- Trainingsplan mit Übungen
- Fortschrittsgrafiken
- Google Fit Integration
- Lokale Datenspeicherung mit Room
```

### Beispiel 3: Chat-App

```
Erstelle eine Chat-Anwendung:
- Echtzeit-Messaging mit WebSockets
- Gruppenchats
- Medien-Upload (Bilder, Videos)
- Push-Notifications
- End-to-End Verschlüsselung
```

## 🏗️ Architektur

```
DevBoxAI/
├── src/
│   ├── DevBoxAI/              # WPF Desktop App
│   │   ├── Views/             # XAML UI
│   │   ├── ViewModels/        # MVVM ViewModels
│   │   └── Services/          # Business Logic
│   ├── DevBoxAI.Core/         # Shared Models & Interfaces
│   ├── DevBoxAI.AI/           # Claude AI Integration
│   └── DevBoxAI.AndroidGenerator/  # Android Code Generation
├── build/                     # Build Output
├── docs/                      # Dokumentation
└── templates/                 # Android Templates
```

## 🔨 Features im Detail

### Chat-Interface

- **Natürlichsprachliche Eingabe**: Keine Code-Kenntnisse erforderlich
- **Kontext-Bewusstsein**: AI versteht Projekt-Kontext
- **Iterative Entwicklung**: "Füge Login hinzu", "Ändere Farbe zu Blau"
- **Fehlerkorrektur**: "Behebe den Crash beim Klick"

### Live-Vorschau

- **Device Frames**: Pixel 7, Pixel 7 Pro, Samsung S23, Tablets
- **Interaktiv**: Klickbare UI, Navigation, Gesten
- **Dark/Light Mode**: Toggle zwischen Themes
- **Layout Inspection**: Bounds, Padding, Margins anzeigen
- **Performance Overlay**: FPS, Memory, Render-Zeit

### Code-Editor

- **Syntax Highlighting**: Kotlin, XML, Gradle
- **File Tree**: Komplette Projekt-Struktur
- **Editierbar**: Manuelle Code-Anpassungen möglich
- **Diff-Ansicht**: Änderungen nachvollziehen

### Build & Deploy

- **Gradle Integration**: Native Gradle Builds
- **Signierung**: Keystore-Verwaltung für Release-Builds
- **ADB Integration**: Direkt auf verbundene Geräte deployen
- **APK/AAB Export**: Play Store ready

### Integrationen

Unterstützte Services:

- **Backend**: Firebase, Supabase, Custom REST/GraphQL APIs
- **Zahlungen**: Stripe, PayPal
- **Maps**: Google Maps, Mapbox
- **Analytics**: Firebase Analytics, Mixpanel
- **Crash Reporting**: Firebase Crashlytics, Sentry
- **Auth**: Firebase Auth, OAuth2, Custom

## 📚 Generierter Code-Struktur

DevBoxAI generiert ein production-ready Android-Projekt:

```
GeneratedApp/
├── app/
│   ├── src/
│   │   ├── main/
│   │   │   ├── java/com/example/app/
│   │   │   │   ├── MainActivity.kt
│   │   │   │   ├── ui/
│   │   │   │   │   ├── screens/      # Compose Screens
│   │   │   │   │   ├── components/   # Reusable UI
│   │   │   │   │   └── theme/        # Material Theme
│   │   │   │   ├── data/
│   │   │   │   │   ├── local/        # Room Database
│   │   │   │   │   ├── remote/       # Retrofit/API
│   │   │   │   │   └── repository/   # Repository Pattern
│   │   │   │   ├── domain/
│   │   │   │   │   ├── model/        # Domain Models
│   │   │   │   │   └── usecase/      # Business Logic
│   │   │   │   └── di/               # Hilt Modules
│   │   │   ├── res/
│   │   │   │   ├── values/           # Strings, Colors
│   │   │   │   ├── drawable/         # Icons, Images
│   │   │   │   └── mipmap/           # App Icons
│   │   │   └── AndroidManifest.xml
│   │   ├── test/                     # Unit Tests
│   │   └── androidTest/              # Instrumentation Tests
│   ├── build.gradle.kts
│   └── proguard-rules.pro
├── build.gradle.kts
├── settings.gradle.kts
└── gradle.properties
```

## 🔐 Sicherheit & Best Practices

### Code-Qualität

- **SOLID Principles**: Clean Architecture
- **Type Safety**: Kotlin Null-Safety
- **Dependency Injection**: Hilt für testbaren Code
- **Repository Pattern**: Saubere Datenschicht-Abstraktion

### Sicherheit

- **ProGuard/R8**: Code-Obfuscation für Release
- **Network Security Config**: TLS-Pinning optional
- **Keystore-Verwaltung**: Sichere APK-Signierung
- **Secrets Management**: API-Keys nicht im Code

### Testing

- **Unit Tests**: ViewModel, Repository, UseCase Tests
- **Integration Tests**: API-Integration Tests
- **UI Tests**: Espresso/Compose UI Tests
- **Test Coverage**: Automatische Coverage-Reports

## 🛠️ Entwickler-Features

### Git-Integration

- **Repository Init**: Automatische Git-Initialisierung
- **Commit History**: Alle Änderungen nachvollziehbar
- **Diff-Ansicht**: Vor/Nach Vergleich
- **Branch Management**: Feature-Branches erstellen

### CI/CD Export

Export von Pipeline-Konfigurationen:

- GitHub Actions
- GitLab CI
- Jenkins
- Bitbucket Pipelines

### Dokumentation

Automatisch generiert:

- **README.md**: Projekt-Übersicht
- **API-Dokumentation**: Endpoints, Models
- **Architecture Decision Records**: Design-Entscheidungen
- **Changelog**: Versions-Historie

## ⚙️ Konfiguration

### Einstellungen

DevBoxAI kann über Settings konfiguriert werden:

- **AI-Model**: Claude-3.5-Sonnet, Claude-3-Opus
- **Code-Style**: Kotlin Coding Conventions
- **Template**: Material 3, Custom Templates
- **Build**: Gradle Version, SDK Versions
- **Workspace**: Projekt-Speicherort

### Android SDK Setup

Beim ersten Start hilft der Setup-Assistent:

1. Android SDK-Pfad auswählen
2. Fehlende Components installieren
3. Licenses akzeptieren
4. Environment-Variablen setzen

## 🚧 Roadmap

### In Entwicklung

- [ ] iOS-App-Generierung (SwiftUI)
- [ ] Flutter-App-Generierung
- [ ] Desktop-App-Generierung (WPF, macOS)
- [ ] Web-App-Generierung (React, Vue)
- [ ] Design-Import (Figma, Sketch)
- [ ] Voice-Input für Chat

### Geplant

- [ ] Multiplayer-Kollaboration
- [ ] Template-Marketplace
- [ ] Plugin-System
- [ ] Cloud-Workspace-Sync
- [ ] AI-Model Auswahl (GPT-4, Gemini)

## 🤝 Beitragen

Contributions sind willkommen! Bitte beachte:

1. Fork das Repository
2. Erstelle einen Feature-Branch
3. Commit deine Änderungen
4. Push zum Branch
5. Erstelle einen Pull Request

## 📄 Lizenz

DevBoxAI ist unter der MIT-Lizenz lizenziert. Siehe [LICENSE](LICENSE) für Details.

## 🙏 Credits

DevBoxAI nutzt folgende großartige Technologien:

- **Anthropic Claude**: AI-Code-Generierung
- **Material Design**: UI-Framework
- **WPF & .NET**: Desktop-Framework
- **Android Jetpack**: Android-Libraries

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/devboxai/issues)
- **Diskussionen**: [GitHub Discussions](https://github.com/yourusername/devboxai/discussions)
- **Email**: support@devboxai.com

## 📊 Statistiken

![GitHub stars](https://img.shields.io/github/stars/yourusername/devboxai)
![GitHub forks](https://img.shields.io/github/forks/yourusername/devboxai)
![GitHub issues](https://img.shields.io/github/issues/yourusername/devboxai)
![License](https://img.shields.io/github/license/yourusername/devboxai)

---

**DevBoxAI** - Von der Idee zur Android-App in Minuten, nicht Wochen! 🚀

Made with ❤️ for Android Developers

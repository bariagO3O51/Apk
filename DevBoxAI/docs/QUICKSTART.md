# DevBoxAI - Quick Start Guide

Dieser Guide führt dich in 5 Minuten von der Installation bis zur ersten generierten Android-App.

## ⚡ 5-Minuten-Setup

### Schritt 1: Installation (2 Minuten)

#### Windows

**Option A: Vorkompilierte Version**

1. Download: [DevBoxAI-v1.0.0.zip](https://github.com/yourusername/devboxai/releases)
2. Entpacken nach `C:\Program Files\DevBoxAI`
3. Doppelklick auf `DevBoxAI.exe`

**Option B: Aus Quellcode**

```powershell
# Klone Repository
git clone https://github.com/yourusername/devboxai.git
cd devboxai/DevBoxAI

# Kompiliere
.\build.ps1

# Starte
.\build\publish\DevBoxAI.exe
```

### Schritt 2: API-Key einrichten (1 Minute)

DevBoxAI benötigt einen Anthropic Claude API-Key:

1. **Registriere dich**: https://www.anthropic.com
2. **Erstelle API-Key**: Console → API Keys → Create Key
3. **Setze Umgebungsvariable**:

```powershell
# PowerShell (temporär für Session)
$env:ANTHROPIC_API_KEY = "sk-ant-api03-..."

# PowerShell (permanent)
[System.Environment]::SetEnvironmentVariable('ANTHROPIC_API_KEY', 'sk-ant-api03-...', 'User')

# CMD
set ANTHROPIC_API_KEY=sk-ant-api03-...
```

**Alternative:** Beim Start wird ein Eingabefeld angezeigt, falls kein Key gesetzt ist.

### Schritt 3: Erste App erstellen (2 Minuten)

1. **DevBoxAI starten**

2. **Im Chat eingeben:**

```
Erstelle eine einfache Counter-App mit:
- Ein Text der den aktuellen Zähler anzeigt
- Plus- und Minus-Buttons
- Material 3 Design
- Dark Mode Support
```

3. **Warte ~30 Sekunden**

DevBoxAI generiert:
- ✅ Kotlin Activity & ViewModel
- ✅ Jetpack Compose UI
- ✅ Material 3 Theme
- ✅ Gradle Build-Files
- ✅ AndroidManifest.xml
- ✅ Resources (strings, colors)

4. **Vorschau ansehen**

Klicke auf "Preview"-Tab → Siehst du die App im Device-Frame!

5. **APK erstellen** (optional, benötigt Android SDK)

Klicke "Build APK" → Fertig in ~1 Minute

## 🎯 Deine ersten 3 Apps

### App 1: ToDo-Liste

```
Baue eine ToDo-App mit:
- Liste aller ToDos
- Hinzufügen-Button mit Dialog
- Checkbox zum Abhaken
- Swipe-to-Delete
- Speicherung mit Room Database
```

**Ergebnis:** Vollständige ToDo-App mit Datenbank-Persistenz

---

### App 2: Wetter-App

```
Erstelle eine Wetter-App:
- Aktuelles Wetter für einen Standort
- 5-Tage-Vorhersage
- Icons für Wetterzustände
- OpenWeatherMap API-Integration
- Pull-to-Refresh
```

**Ergebnis:** Wetter-App mit API-Integration

---

### App 3: Chat-App

```
Generiere eine Chat-Anwendung:
- Chat-Liste
- Nachricht senden und empfangen
- Gruppiert nach Datum
- Firebase Realtime Database
- Push-Benachrichtigungen
```

**Ergebnis:** Real-time Chat mit Firebase

## 💬 Chat-Tipps

### ✅ Gute Prompts

**Spezifisch und strukturiert:**

```
Erstelle eine Fitness-App mit:
1. Dashboard mit Statistiken (Schritte, Kalorien)
2. Trainingsplan-Liste
3. Timer für Übungen
4. Google Fit Integration
5. Grafiken für Fortschritt
```

**Klare Feature-Liste:**

```
E-Commerce-App:
- Produktkatalog mit Kategorien
- Warenkorb
- Stripe-Checkout
- Bestellhistorie
- Push-Notifications für Angebote
```

### ❌ Weniger gute Prompts

**Zu vage:**
```
Mach mir eine App
```

**Zu komplex (in einem Schritt):**
```
Erstelle die nächste Super-App mit Social Media, E-Commerce,
Video-Streaming, Gaming, Krypto-Wallet und AI-Chat
```

**Besser:** Schrittweise entwickeln:
```
1. "Erstelle eine Social-Media-App mit Posts und Likes"
2. "Füge Kommentare hinzu"
3. "Integriere Bildupload"
```

## 🔧 Iteration & Änderungen

### App anpassen

**Nach der Generierung:**

```
Ändere die Farbe zu Blau
```

```
Füge einen Settings-Screen hinzu mit:
- Dark Mode Toggle
- Sprache ändern
- Über-Seite
```

```
Ersetze die Bottom-Navigation durch ein Drawer-Menü
```

### Bugs fixen

```
Behebe den Crash beim Klick auf den Login-Button
```

```
Das Profil-Bild wird nicht angezeigt, fixe das bitte
```

## 🏗️ Build & Deploy

### Debug-Build (für Testing)

1. Klicke "Build APK"
2. Warte ~1-2 Minuten
3. APK findest du in: `C:\Users\YourName\AppData\Roaming\DevBoxAI\Workspace\ProjectName\app\build\outputs\apk\debug\`

### Release-Build (für Play Store)

**Vorbereitung: Keystore erstellen**

```powershell
# Android SDK muss installiert sein
keytool -genkey -v -keystore my-release-key.jks `
  -keyalg RSA -keysize 2048 -validity 10000 `
  -alias my-key-alias
```

**In DevBoxAI:**

1. Settings → Build Configuration
2. Keystore-Pfad: `my-release-key.jks`
3. Passwords eingeben
4. Build-Type: "Release" wählen
5. "Build APK" klicken

### Installation auf Gerät

**Via ADB:**

1. USB-Debugging auf Android-Gerät aktivieren
2. Gerät per USB verbinden
3. In DevBoxAI: Device-Dropdown → Wähle dein Gerät
4. Klicke "Install on Device"

**Manuell:**

1. APK-Datei auf Gerät kopieren
2. Mit Datei-Manager öffnen
3. "Installieren" klicken

## 🐛 Troubleshooting

### "API Key ungültig"

**Lösung:**

1. Prüfe ob Key richtig gesetzt: `echo $env:ANTHROPIC_API_KEY`
2. Key muss mit `sk-ant-api03-` beginnen
3. Neu erstellen: https://console.anthropic.com

### "Gradle Build failed"

**Lösung:**

1. Android SDK installiert?
   - Download: https://developer.android.com/studio
2. ANDROID_HOME gesetzt?
   ```powershell
   $env:ANDROID_HOME = "C:\Users\YourName\AppData\Local\Android\Sdk"
   ```
3. Java JDK 17 installiert?
   ```powershell
   java -version  # sollte 17.x zeigen
   ```

### "Preview zeigt nichts an"

**Lösung:**

1. Warte bis Code-Generierung abgeschlossen
2. Klicke "Preview" erneut
3. Falls immer noch leer: Projekt neu generieren

### "DevBoxAI startet nicht"

**Lösung:**

1. .NET 8.0 Runtime installiert?
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
2. Windows 10/11 64-bit?
3. Antivirus deaktiviert? (manchmal false-positive)

## 📚 Nächste Schritte

### Lerne mehr:

- **Tutorial-Serie**: [docs/tutorials/](../tutorials/)
- **API-Referenz**: [docs/api/](../api/)
- **Beispiel-Apps**: [examples/](../../examples/)

### Community:

- **Discord**: https://discord.gg/devboxai
- **GitHub Discussions**: https://github.com/yourusername/devboxai/discussions
- **YouTube**: https://youtube.com/@devboxai

### Advanced Features:

- **Custom Templates**: Erstelle eigene Projekt-Templates
- **Plugins**: Erweitere DevBoxAI mit Plugins
- **CI/CD**: Automatisiere Builds mit GitHub Actions

---

**Bereit loszulegen?** Öffne DevBoxAI und erstelle deine erste App! 🚀

Bei Problemen: [GitHub Issues](https://github.com/yourusername/devboxai/issues)

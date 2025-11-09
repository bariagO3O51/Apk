using DevBoxAI.Core.Models;
using System.IO;
using System.Text;

namespace DevBoxAI.AndroidGenerator;

public class ProjectGenerator
{
    private readonly string _workspaceRoot;

    public ProjectGenerator(string workspaceRoot)
    {
        _workspaceRoot = workspaceRoot;
    }

    public async Task<string> CreateProjectStructureAsync(AndroidProject project)
    {
        var projectPath = Path.Combine(_workspaceRoot, project.Name);

        // Create directory structure
        CreateDirectoryStructure(projectPath, project.PackageName);

        // Generate base files
        await GenerateGradleFilesAsync(projectPath, project);
        await GenerateManifestAsync(projectPath, project);
        await GenerateMainActivityAsync(projectPath, project);
        await GenerateResourcesAsync(projectPath, project);

        project.Path = projectPath;
        return projectPath;
    }

    private void CreateDirectoryStructure(string projectPath, string packageName)
    {
        var packagePath = packageName.Replace('.', '/');

        // Root directories
        Directory.CreateDirectory(projectPath);
        Directory.CreateDirectory(Path.Combine(projectPath, "app"));
        Directory.CreateDirectory(Path.Combine(projectPath, "gradle", "wrapper"));

        // Source directories
        var srcMain = Path.Combine(projectPath, "app", "src", "main");
        var srcTest = Path.Combine(projectPath, "app", "src", "test");
        var srcAndroidTest = Path.Combine(projectPath, "app", "src", "androidTest");

        Directory.CreateDirectory(Path.Combine(srcMain, "java", packagePath));
        Directory.CreateDirectory(Path.Combine(srcMain, "java", packagePath, "ui"));
        Directory.CreateDirectory(Path.Combine(srcMain, "java", packagePath, "data"));
        Directory.CreateDirectory(Path.Combine(srcMain, "java", packagePath, "domain"));
        Directory.CreateDirectory(Path.Combine(srcMain, "java", packagePath, "di"));

        Directory.CreateDirectory(Path.Combine(srcMain, "res", "layout"));
        Directory.CreateDirectory(Path.Combine(srcMain, "res", "values"));
        Directory.CreateDirectory(Path.Combine(srcMain, "res", "drawable"));
        Directory.CreateDirectory(Path.Combine(srcMain, "res", "mipmap-hdpi"));
        Directory.CreateDirectory(Path.Combine(srcMain, "res", "mipmap-mdpi"));
        Directory.CreateDirectory(Path.Combine(srcMain, "res", "mipmap-xhdpi"));
        Directory.CreateDirectory(Path.Combine(srcMain, "res", "mipmap-xxhdpi"));
        Directory.CreateDirectory(Path.Combine(srcMain, "res", "mipmap-xxxhdpi"));

        Directory.CreateDirectory(Path.Combine(srcTest, "java", packagePath));
        Directory.CreateDirectory(Path.Combine(srcAndroidTest, "java", packagePath));
    }

    private async Task GenerateGradleFilesAsync(string projectPath, AndroidProject project)
    {
        // settings.gradle.kts
        var settingsGradle = @"pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}
dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = """ + project.Name + @"""
include("":app"")
";
        await File.WriteAllTextAsync(
            Path.Combine(projectPath, "settings.gradle.kts"),
            settingsGradle
        );

        // Root build.gradle.kts
        var rootGradle = @"plugins {
    id(""com.android.application"") version ""8.2.0"" apply false
    id(""org.jetbrains.kotlin.android"") version ""1.9.20"" apply false
    id(""com.google.dagger.hilt.android"") version ""2.48"" apply false
}
";
        await File.WriteAllTextAsync(
            Path.Combine(projectPath, "build.gradle.kts"),
            rootGradle
        );

        // App build.gradle.kts
        var appGradle = GenerateAppGradleFile(project);
        await File.WriteAllTextAsync(
            Path.Combine(projectPath, "app", "build.gradle.kts"),
            appGradle
        );

        // gradle.properties
        var gradleProps = @"org.gradle.jvmargs=-Xmx2048m -Dfile.encoding=UTF-8
android.useAndroidX=true
android.enableJetifier=true
kotlin.code.style=official
android.nonTransitiveRClass=true
";
        await File.WriteAllTextAsync(
            Path.Combine(projectPath, "gradle.properties"),
            gradleProps
        );
    }

    private string GenerateAppGradleFile(AndroidProject project)
    {
        var config = project.Configuration;
        var hasFirebase = project.Integrations.Any(i => i.Type == IntegrationType.Firebase);

        var plugins = new StringBuilder();
        plugins.AppendLine("plugins {");
        plugins.AppendLine("    id(\"com.android.application\")");
        plugins.AppendLine("    id(\"org.jetbrains.kotlin.android\")");
        plugins.AppendLine("    id(\"kotlin-kapt\")");
        plugins.AppendLine("    id(\"com.google.dagger.hilt.android\")");
        if (hasFirebase)
        {
            plugins.AppendLine("    id(\"com.google.gms.google-services\")");
        }
        plugins.AppendLine("}");

        return $@"{plugins}

android {{
    namespace = ""{project.PackageName}""
    compileSdk = {config.CompileSdkVersion}

    defaultConfig {{
        applicationId = ""{project.PackageName}""
        minSdk = {config.MinSdkVersion}
        targetSdk = {config.TargetSdkVersion}
        versionCode = {config.VersionCode}
        versionName = ""{config.VersionName}""

        testInstrumentationRunner = ""androidx.test.runner.AndroidJUnitRunner""
        vectorDrawables {{
            useSupportLibrary = true
        }}
    }}

    buildTypes {{
        release {{
            isMinifyEnabled = true
            proguardFiles(
                getDefaultProguardFile(""proguard-android-optimize.txt""),
                ""proguard-rules.pro""
            )
        }}
    }}

    compileOptions {{
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }}

    kotlinOptions {{
        jvmTarget = ""17""
    }}

    buildFeatures {{
        compose = true
    }}

    composeOptions {{
        kotlinCompilerExtensionVersion = ""1.5.4""
    }}

    packaging {{
        resources {{
            excludes += ""/META-INF/{{AL2.0,LGPL2.1}}""
        }}
    }}
}}

dependencies {{
    // Core Android
    implementation(""androidx.core:core-ktx:1.12.0"")
    implementation(""androidx.lifecycle:lifecycle-runtime-ktx:2.6.2"")
    implementation(""androidx.activity:activity-compose:1.8.1"")

    // Compose
    implementation(platform(""androidx.compose:compose-bom:2023.10.01""))
    implementation(""androidx.compose.ui:ui"")
    implementation(""androidx.compose.ui:ui-graphics"")
    implementation(""androidx.compose.ui:ui-tooling-preview"")
    implementation(""androidx.compose.material3:material3"")
    implementation(""androidx.navigation:navigation-compose:2.7.5"")

    // Hilt
    implementation(""com.google.dagger:hilt-android:2.48"")
    kapt(""com.google.dagger:hilt-android-compiler:2.48"")
    implementation(""androidx.hilt:hilt-navigation-compose:1.1.0"")

    // Room
    implementation(""androidx.room:room-runtime:2.6.0"")
    implementation(""androidx.room:room-ktx:2.6.0"")
    kapt(""androidx.room:room-compiler:2.6.0"")

    // Retrofit
    implementation(""com.squareup.retrofit2:retrofit:2.9.0"")
    implementation(""com.squareup.retrofit2:converter-gson:2.9.0"")
    implementation(""com.squareup.okhttp3:okhttp:4.12.0"")
    implementation(""com.squareup.okhttp3:logging-interceptor:4.12.0"")

    // Coroutines
    implementation(""org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3"")

    // Testing
    testImplementation(""junit:junit:4.13.2"")
    testImplementation(""org.mockito:mockito-core:5.7.0"")
    androidTestImplementation(""androidx.test.ext:junit:1.1.5"")
    androidTestImplementation(""androidx.test.espresso:espresso-core:3.5.1"")
    androidTestImplementation(platform(""androidx.compose:compose-bom:2023.10.01""))
    androidTestImplementation(""androidx.compose.ui:ui-test-junit4"")
    debugImplementation(""androidx.compose.ui:ui-tooling"")
    debugImplementation(""androidx.compose.ui:ui-test-manifest"")
}}
";
    }

    private async Task GenerateManifestAsync(string projectPath, AndroidProject project)
    {
        var manifest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<manifest xmlns:android=""http://schemas.android.com/apk/res/android""
    xmlns:tools=""http://schemas.android.com/tools"">

    <uses-permission android:name=""android.permission.INTERNET"" />

    <application
        android:name="".{project.Name}Application""
        android:allowBackup=""true""
        android:dataExtractionRules=""@xml/data_extraction_rules""
        android:fullBackupContent=""@xml/backup_rules""
        android:icon=""@mipmap/ic_launcher""
        android:label=""@string/app_name""
        android:roundIcon=""@mipmap/ic_launcher_round""
        android:supportsRtl=""true""
        android:theme=""@style/Theme.{project.Name}""
        tools:targetApi=""31"">
        <activity
            android:name="".MainActivity""
            android:exported=""true""
            android:theme=""@style/Theme.{project.Name}"">
            <intent-filter>
                <action android:name=""android.intent.action.MAIN"" />
                <category android:name=""android.intent.category.LAUNCHER"" />
            </intent-filter>
        </activity>
    </application>

</manifest>";

        var manifestPath = Path.Combine(projectPath, "app", "src", "main", "AndroidManifest.xml");
        await File.WriteAllTextAsync(manifestPath, manifest);
    }

    private async Task GenerateMainActivityAsync(string projectPath, AndroidProject project)
    {
        var packagePath = project.PackageName.Replace('.', '/');
        var activityPath = Path.Combine(
            projectPath, "app", "src", "main", "java", packagePath, "MainActivity.kt"
        );

        var activity = $@"package {project.PackageName}

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import dagger.hilt.android.AndroidEntryPoint

@AndroidEntryPoint
class MainActivity : ComponentActivity() {{
    override fun onCreate(savedInstanceState: Bundle?) {{
        super.onCreate(savedInstanceState)
        setContent {{
            {project.Name}Theme {{
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {{
                    MainScreen()
                }}
            }}
        }}
    }}
}}

@Composable
fun MainScreen() {{
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {{
        Text(
            text = ""{project.Configuration.AppName}"",
            style = MaterialTheme.typography.headlineLarge
        )
        Spacer(modifier = Modifier.height(16.dp))
        Text(
            text = ""Generated by DevBoxAI"",
            style = MaterialTheme.typography.bodyMedium
        )
    }}
}}

@Preview(showBackground = true)
@Composable
fun DefaultPreview() {{
    {project.Name}Theme {{
        MainScreen()
    }}
}}
";

        Directory.CreateDirectory(Path.GetDirectoryName(activityPath)!);
        await File.WriteAllTextAsync(activityPath, activity);
    }

    private async Task GenerateResourcesAsync(string projectPath, AndroidProject project)
    {
        var resPath = Path.Combine(projectPath, "app", "src", "main", "res");

        // strings.xml
        var strings = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<resources>
    <string name=""app_name"">{project.Configuration.AppName}</string>
</resources>";
        await File.WriteAllTextAsync(
            Path.Combine(resPath, "values", "strings.xml"),
            strings
        );

        // colors.xml
        var colors = @"<?xml version=""1.0"" encoding=""utf-8""?>
<resources>
    <color name=""purple_200"">#FFBB86FC</color>
    <color name=""purple_500"">#FF6200EE</color>
    <color name=""purple_700"">#FF3700B3</color>
    <color name=""teal_200"">#FF03DAC5</color>
    <color name=""teal_700"">#FF018786</color>
    <color name=""black"">#FF000000</color>
    <color name=""white"">#FFFFFFFF</color>
</resources>";
        await File.WriteAllTextAsync(
            Path.Combine(resPath, "values", "colors.xml"),
            colors
        );

        // themes.xml
        var themes = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<resources>
    <style name=""Theme.{project.Name}"" parent=""android:Theme.Material.Light.NoActionBar"" />
</resources>";
        await File.WriteAllTextAsync(
            Path.Combine(resPath, "values", "themes.xml"),
            themes
        );
    }
}

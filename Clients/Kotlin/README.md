# Kotlin Gateway foundation

Minimum Android API: 26. Add `src/main/kotlin` to an existing Android application's main source set, for example from the app module:

```kotlin
android {
    defaultConfig { minSdk = 26 }
    sourceSets["main"].java.srcDir("PATH/TO/DisplayMagicianv4/Clients/Kotlin/src/main/kotlin")
}
```

Use the app's existing `kotlinx-coroutines-android` dependency. No JSON serialization plugin, Retrofit, generated schema union types, custom signing algorithm, or SSE dependency is required. Methods are suspending and run blocking HTTPS I/O on `Dispatchers.IO`.

Declare `android.permission.INTERNET` in the containing app. Network permission, camera/QR scanning, application lifecycle, Wear OS Data Layer, and the UI belong to that app. Keep one `DMGateway` instance per paired host. The SDK uses a per-host pinned TLS socket factory; it does not replace process-wide TLS defaults or hostname verifiers. Private credentials and pending mutation requests are encrypted under Android Keystore and stored outside app backup.

From the Windows repository root, run `verify_rest_clients.ps1 -CompileKotlin -InstallTools` to fetch checksum-pinned local verification tools, compile all client sources against the Android SDK, and execute portable contract/pinning tests. The JSON-java dependency is for JVM tests only; Android supplies `org.json`. A JVM test cannot validate Android Keystore or the device's HTTPS hostname-verification implementation.

See Android's [network security guidance](https://developer.android.com/privacy-and-security/security-ssl) and [Keystore documentation](https://developer.android.com/privacy-and-security/keystore), and run the [device acceptance checklist](../README.md#acceptance-checklist).

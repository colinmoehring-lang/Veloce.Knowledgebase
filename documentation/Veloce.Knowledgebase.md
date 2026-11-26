# Veloce Knowledgebase

Das Backend stellt eine versionierte ASP.NET-Core-API für Konten, Fahrzeuge und
Fahrten bereit. Es verwendet .NET 10, Entity Framework Core und PostgreSQL.
Die Android-App spricht die API unter `/api/integration/v1` an.

## Konfiguration und lokaler Start

Voraussetzungen sind das .NET-10-SDK, eine PostgreSQL-Datenbank und ein
SMTP-Konto mit STARTTLS. Konfiguration kann über Umgebungsvariablen oder lokal
über .NET User Secrets gesetzt werden. Zugangsdaten nicht einchecken.

| Variable | Bedeutung |
| --- | --- |
| `ConnectionStrings__veloce-knowledgebasedb` | Npgsql-Verbindungszeichenfolge; alternativ `ConnectionStrings__DefaultConnection` |
| `Jwt__SecretKey` | Pflicht; zufälliger, geheimer Schlüssel mit mindestens 32 UTF-8-Bytes |
| `Jwt__Issuer` | Optional; Standard `Veloce.Knowledgebase` |
| `Jwt__Audience` | Optional; Standard `Veloce.Knowledgebase.Clients` |
| `Jwt__ExpiryMinutes` | Optional; Standard 120 Minuten |
| `Smtp__Username`, `Smtp__Password` | Zugangsdaten für den E-Mail-Versand |
| `Smtp__Host`, `Smtp__Port` | Optional; Standard `smtp-relay.brevo.com:587`, STARTTLS |
| `Smtp__FromEmail`, `Smtp__FromName` | Optional; Standard `noreply@investra.de` und `Investra`; Absender beim Anbieter verifizieren |
| `Proxy__KnownProxies` | Optional; kommaseparierte, ausdrücklich vertrauenswürdige Proxy-IP-Adressen für Forwarded Headers |

Die Datenbankverbindung muss als Npgsql-Zeichenfolge vorliegen, beispielsweise
`Host=<host>;Port=5432;Database=<database>;Username=<user>;Password=<password>`.
Für Produktionsverbindungen TLS mit Zertifikatsprüfung konfigurieren.
Supabase-PostgreSQL-URIs müssen dafür in dieses Format übertragen werden.
Der bisherige voreingestellte JWT-Schlüssel wurde entfernt. Nach dem Wechsel auf
einen eigenen Schlüssel sowie durch die Prüfung von Issuer und Audience können
zuvor ausgestellte Tokens ungültig werden; Clients müssen sich erneut anmelden.

Im Projektverzeichnis lokal beispielsweise User Secrets konfigurieren:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<Npgsql-Verbindungszeichenfolge>" --project .\dotnet\app\Veloce.Knowledgebase
dotnet user-secrets set "Jwt:SecretKey" "<zufällig-generierter-Schlüssel>" --project .\dotnet\app\Veloce.Knowledgebase
dotnet user-secrets set "Smtp:Username" "<SMTP-Benutzer>" --project .\dotnet\app\Veloce.Knowledgebase
dotnet user-secrets set "Smtp:Password" "<SMTP-Passwort>" --project .\dotnet\app\Veloce.Knowledgebase
dotnet run --project .\dotnet\app\Veloce.Knowledgebase --launch-profile http
```

Die Anwendung führt beim Start ausstehende EF-Migrationen aus. Dazu benötigt die
Datenbankrolle Schemaänderungsrechte. Bei Fehlern startet der Dienst nicht.
Für direkte `dotnet ef`-Aufrufe erwartet die Design-Time-Factory die
Verbindungszeichenfolge als Umgebungsvariable.
Lokal sind Swagger unter `http://localhost:6000/swagger` und der reine
Liveness-Endpunkt `http://localhost:6000/health/live` verfügbar. Letzterer
prüft **nicht** die Datenbank oder SMTP.

## Render und Supabase

- Auf Render einen Docker-Web-Service mit dem Repository-Verzeichnis als
  Build-Kontext erstellen. Das Dockerfile bindet an `0.0.0.0:8080`; auf Render
  `PORT=8080` setzen und `/health/live` als Health-Check-Pfad verwenden.
- Die oben genannten Secrets in Render hinterlegen, nicht im Docker-Image.
- Im Supabase-Dialog **Connect** die für die Erreichbarkeit passende
  PostgreSQL-Verbindung auswählen. Für einen dauerhaften Backend-Prozess empfiehlt
  Supabase die direkte Verbindung bei IPv6-Erreichbarkeit bzw. den Session-Pooler
  für IPv4. Host, Port und Benutzername aus dem Dialog übernehmen; Pooler- und
  direkte Benutzernamen unterscheiden sich. TLS-Zertifikate prüfen und
  Verbindungszahl sowie Limits des gewählten Plans berücksichtigen.
- Render blockiert beim Free-Web-Service ausgehende SMTP-Verbindungen auf
  Port 25, 465 und 587; Port 25 ist auch bei bezahlten Diensten gesperrt.
  Für den konfigurierten STARTTLS-Versand auf Port 587 ist daher ein passender
  bezahlter Plan erforderlich. Render empfiehlt Free-Instanzen nicht für
  Produktion. Siehe [Render Free](https://render.com/docs/free).
- Vor dem öffentlichen Einsatz die tatsächlichen vertrauenswürdigen Proxy-IPs
  und das Forwarded-Header-Verhalten mit Render verifizieren und gegebenenfalls
  `Proxy__KnownProxies` konfigurieren. Nur eingetragene Adressen sowie die
  ASP.NET-Core-Loopback-Standards werden vertraut; genau ein Forwarded-Hop wird
  ausgewertet. Ohne passende Proxy-Konfiguration teilen sich Nutzer hinter
  demselben Proxy dessen IP-Limit. Keine beliebigen oder ungeprüften
  Proxy-Adressen eintragen.
- Migrationen laufen derzeit beim Start des Web-Service über dieselbe Verbindung.
  Vor Deployments Backups anlegen, Migrationsrechte und Verbindung separat prüfen
  und Migrationen bei mehreren Instanzen und Rollbacks betrieblich einplanen.
  Die Migration schlägt bei verwaisten Codes oder fremden Fahrzeugzuordnungen
  bewusst fehl, statt Daten zu löschen. Vorhandene Inkonsistenzen vorher prüfen.
  Sie aktiviert Row Level Security für die Backendtabellen und entzieht `PUBLIC`
  sowie, falls vorhanden, Supabase-`anon` und `authenticated` deren
  Tabellenrechte. Es werden keine Client-Policies angelegt. Die Backend-Verbindung benötigt einen passenden
  Eigentümer bzw. eine Rolle, die RLS umgehen darf; diese Zugangsdaten bleiben
  ausschließlich serverseitig. Bestehende Policies, Grants, exponierte Schemas
  und Views im Supabase-Projekt zusätzlich separat prüfen.
  Der zusammengesetzte Fahrzeug-Constraint und der Status-Trigger sind in der
  Migration als SQL definiert und müssen bei zukünftigen Schemaänderungen
  berücksichtigt werden. Ein Rollback auf eine ältere Version ist nach der Umstellung der Code-Hashes
  nicht ohne Weiteres möglich; die RLS-Einstellungen bleiben beim Rollback aktiv.
- Keine Supabase- oder SMTP-Zugangsdaten an den Android-Client ausliefern.

Referenzen: [Render Web Services](https://render.com/docs/web-services) und
[Supabase PostgreSQL-Verbindungen](https://supabase.com/docs/guides/database/connecting-to-postgres).

## API

Alle folgenden Routen beginnen mit `/api/integration/v1`. Außer `Auth` benötigen
sie `Authorization: Bearer <accessToken>`. Swagger zeigt die vollständigen DTOs.

| Methode und Route | Funktion |
| --- | --- |
| `POST /Auth/sign-up` | Konto anlegen oder bei ausstehender Verifizierung Code erneut senden |
| `POST /Auth/verify-email` | Konto mit E-Mail und Code verifizieren |
| `POST /Auth/login` | JWT nach erfolgreicher Anmeldung beziehen |
| `POST /Auth/forgot-password` | Passwort-Reset-Code anfordern |
| `POST /Auth/reset-password` | Passwort mit Code ändern |
| `GET, POST /Vehicle` | Eigene Fahrzeuge auflisten bzw. anlegen |
| `GET, PUT, DELETE /Vehicle/{id}` | Eigenes Fahrzeug lesen, ändern bzw. löschen |
| `GET /User/records` | Eigene Fahrtrekorde lesen |
| `POST /Ride/start` | Fahrt starten, optional mit eigenem Fahrzeug |
| `POST /Ride/{rideId}/point` | Messpunkt speichern |
| `POST /Ride/{rideId}/stop` | Fahrt beenden und Statistik berechnen |
| `GET /Ride`, `GET /Ride/{rideId}` | Eigene Fahrten auflisten bzw. abrufen |
| `GET /RidePoint/all/{rideId}` | Koordinaten einer eigenen Fahrt nach Zeit sortiert abrufen |
| `GET /RidePoint/{ridePointId}` | Details eines eigenen Fahrpunkts abrufen |

Registrierung und Passwort-Reset benötigen einen funktionierenden SMTP-Versand.
Codes sind sechs Ziffern lang und 15 Minuten gültig. Wird bei einer ausstehenden
Registrierung ein neuer Code angefordert, bleibt das ursprünglich gesetzte
Passwort erhalten. Bei einem Versandfehler kann die Registrierung bereits in
der Datenbank stehen; ein erneuter Registrierungsaufruf sendet einen neuen Code.
Neue Codes werden als zweck- und nutzergebundener HMAC gespeichert. Bestehende
Klartext-Codes bleiben bis zu ihrem Ablauf verwendbar und werden nicht automatisch
gelöscht. Das Rotieren des JWT-Schlüssels macht auch noch ausstehende neue Codes
ungültig.

Für Auth-Anfragen gelten PostgreSQL-gestützte Limits über alle Instanzen:
100 Anfragen pro IP und 10 pro normalisierter E-Mail-Adresse in jeweils
15 Minuten. Der IP-Schutz greift auch bei ungültigen Request-Bodys.
Passwortwechsel und Änderungen des Kontostatus widerrufen bestehende JWTs;
bei jedem authentifizierten Request wird der aktuelle Kontostatus geprüft.

Fahrpunkte verwenden `coordinate` im Format `"Breitengrad,Längengrad"` mit
Dezimalpunkt, `timestamp` mit UTC-Offset, `speed` in km/h, `lean` in Grad und
`gForce` als G-Wert. Beim Stoppen wird `distance` in Metern aus zeitlich
sortierten Koordinaten mittels Haversine berechnet; `duration` ist in Sekunden.
`averageSpeed` ist das arithmetische Mittel der Messwerte, nicht die
streckengewichtete Durchschnittsgeschwindigkeit. Ohne Punkte bleibt die Distanz
null. Ungültige gespeicherte Koordinaten verhindern die Statistikberechnung.

## Datenmodell und Betriebshinweise

Das Modell trennt Nutzer, Fahrzeuge, Fahrten und Fahrpunkte. Fahrzeuge gehören
Nutzern; Fahrten gehören Nutzern und optional einem Fahrzeug; Fahrpunkte gehören
einer Fahrt. Beim Löschen eines Fahrzeugs bleibt die Fahrt erhalten und der
Fahrzeugbezug wird entfernt. Datenbank-Constraints erzwingen die zugehörigen
Nutzer- und Fahrzeugbeziehungen; normalisierte E-Mail-Adressen sind eindeutig.
Codes gehören per Fremdschlüssel zu einem Nutzer.

Neue Fahrten sind aktiv oder abgeschlossen. Nach Abschluss werden Messpunkte mit
HTTP 409 abgewiesen; erneutes Stoppen liefert die gespeicherte Zusammenfassung.
Für bestehende Fahrten setzt die Migration den Status `Unknown`, weil sich ihr
früherer Status nicht zuverlässig rekonstruieren lässt; sie bleiben lesbar,
können aber nicht verändert werden. Alte Nulldistanzen werden nicht rückwirkend
berechnet.

Authentifizierungsvorgänge werden pro E-Mail-Adresse über PostgreSQL-Sperren
serialisiert; die Datenbankänderungen eines Vorgangs erfolgen transaktional.
E-Mail-Versand erfolgt erst nach dem Commit und ist keine atomare
Datenbankoperation.

Vor Freigabe müssen die tatsächliche Render-/Supabase-Verbindung, SMTP-Zustellung,
Proxy-Konfiguration, Backups, Monitoring und Wiederherstellung im Zielsystem
geprüft werden. Swagger ist derzeit öffentlich erreichbar. Datenbankrollen,
Datenschutz und Aufbewahrung von Standortdaten müssen für den konkreten Betrieb
festgelegt werden.

## NuGet-Paketverwaltung

Paketversionen werden zentral in `Directory.Packages.props` verwaltet. Neue
Pakete erhalten dort einen `PackageVersion`-Eintrag; die Projekte referenzieren
sie ohne Versionsangabe.

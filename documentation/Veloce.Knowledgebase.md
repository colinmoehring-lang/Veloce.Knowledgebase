# SMTP-Konfiguration für Passwort-Reset-E-Mails

Passwort-Reset-Codes werden über SMTP mit STARTTLS versendet. Für den Versand müssen
nur folgende Umgebungsvariablen in der Anwendung gesetzt sein:

| Variable | Beschreibung |
| --- | --- |
| `Smtp__Username` | SMTP-Benutzername |
| `Smtp__Password` | SMTP-Passwort |

Der SMTP-Host ist standardmäßig `smtp-relay.brevo.com`, der Port standardmäßig `587`;
die Verbindung verwendet STARTTLS. Als Absender werden `noreply@investra.de` und
`Investra` verwendet. Die Absenderadresse muss in Brevo verifiziert sein.
Zugangsdaten sollten als geschützte Umgebungsvariablen beziehungsweise Secret-Store-
Einträge bereitgestellt und nicht in Konfigurationsdateien eingecheckt werden.

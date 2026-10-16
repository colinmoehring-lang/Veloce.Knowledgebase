using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Veloce.Knowledgebase.Services.Email;

public class SmtpEmailSender(
    IConfiguration configuration,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<SmtpEmailSender> _logger = logger;

    public async Task SendVerificationCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        var subject = "Dein Investra Verifizierungscode";
        var htmlContent = BuildVerificationCodeEmail(code);
        var plainContent = $"Dein Verifizierungscode lautet: {code}\n\nDer Code ist 15 Minuten gültig.";

        await SendEmailAsync(email, subject, htmlContent, plainContent, cancellationToken);
    }

    public async Task SendPasswordResetCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        var subject = "Dein Investra Passwort-Reset-Code";
        var htmlContent = BuildPasswordResetCodeEmail(code);
        var plainContent = $"Dein Passwort-Reset-Code lautet: {code}\n\nDer Code ist 15 Minuten gültig.";

        await SendEmailAsync(email, subject, htmlContent, plainContent, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string plainContent,
        CancellationToken cancellationToken)
    {
        var host = _configuration["Smtp:Host"]
            ?? throw new InvalidOperationException("Smtp:Host is not configured.");
        var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
        var username = _configuration["Smtp:Username"]
            ?? throw new InvalidOperationException("Smtp:Username is not configured.");
        var password = _configuration["Smtp:Password"]
            ?? throw new InvalidOperationException("Smtp:Password is not configured.");
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@investra.de";
        var fromName = _configuration["Smtp:FromName"] ?? "Investra";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(string.Empty, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlContent,
            TextBody = plainContent
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(username, password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        _logger.LogInformation("Email sent successfully to {Email} via SMTP ({Host}).", toEmail, host);
    }

    private static string BuildVerificationCodeEmail(string code) => $"""
        <!DOCTYPE html>
        <html lang="de">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
          <title>Verifizierungscode</title>
        </head>
        <body style="margin:0;padding:0;background:#0f1117;font-family:'Segoe UI',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#0f1117;padding:40px 0;">
            <tr>
              <td align="center">
                <table width="560" cellpadding="0" cellspacing="0" style="background:#1a1d27;border-radius:16px;overflow:hidden;border:1px solid #2a2d3e;">
                  <!-- Header -->
                  <tr>
                    <td style="background:linear-gradient(135deg,#6c63ff,#3ecfcf);padding:40px 40px 32px;text-align:center;">
                      <h1 style="margin:0;color:#ffffff;font-size:28px;font-weight:700;letter-spacing:-0.5px;">Investra</h1>
                      <p style="margin:8px 0 0;color:rgba(255,255,255,0.85);font-size:14px;">E-Mail-Verifizierung</p>
                    </td>
                  </tr>
                  <!-- Body -->
                  <tr>
                    <td style="padding:40px;">
                      <p style="margin:0 0 24px;color:#c0c4d6;font-size:16px;line-height:1.6;">
                        Willkommen bei Investra! Bitte gib den folgenden Code ein, um deine E-Mail-Adresse zu bestätigen:
                      </p>
                      <!-- Code Box -->
                      <div style="background:#0f1117;border:1px solid #2a2d3e;border-radius:12px;padding:28px;text-align:center;margin:0 0 28px;">
                        <p style="margin:0 0 8px;color:#7b80a0;font-size:12px;text-transform:uppercase;letter-spacing:2px;">Dein Code</p>
                        <span style="font-size:42px;font-weight:700;letter-spacing:12px;color:#6c63ff;font-family:'Courier New',monospace;">{code}</span>
                      </div>
                      <p style="margin:0 0 8px;color:#7b80a0;font-size:13px;">
                        ⏱ Dieser Code ist <strong style="color:#c0c4d6;">15 Minuten</strong> gültig.
                      </p>
                      <p style="margin:0;color:#7b80a0;font-size:13px;">
                        Falls du kein Konto erstellt hast, ignoriere diese E-Mail bitte.
                      </p>
                    </td>
                  </tr>
                  <!-- Footer -->
                  <tr>
                    <td style="padding:20px 40px 32px;border-top:1px solid #2a2d3e;">
                      <p style="margin:0;color:#4a4e6a;font-size:12px;text-align:center;">
                        © {DateTime.UtcNow.Year} Investra · Lotac GmbH · Alle Rechte vorbehalten
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;

    private static string BuildPasswordResetCodeEmail(string code) => $"""
        <!DOCTYPE html>
        <html lang="de">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
          <title>Passwort zurücksetzen</title>
        </head>
        <body style="margin:0;padding:0;background:#0f1117;font-family:'Segoe UI',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#0f1117;padding:40px 0;">
            <tr>
              <td align="center">
                <table width="560" cellpadding="0" cellspacing="0" style="background:#1a1d27;border-radius:16px;overflow:hidden;border:1px solid #2a2d3e;">
                  <!-- Header -->
                  <tr>
                    <td style="background:linear-gradient(135deg,#ff6b6b,#ff9a3c);padding:40px 40px 32px;text-align:center;">
                      <h1 style="margin:0;color:#ffffff;font-size:28px;font-weight:700;letter-spacing:-0.5px;">Investra</h1>
                      <p style="margin:8px 0 0;color:rgba(255,255,255,0.85);font-size:14px;">Passwort zurücksetzen</p>
                    </td>
                  </tr>
                  <!-- Body -->
                  <tr>
                    <td style="padding:40px;">
                      <p style="margin:0 0 24px;color:#c0c4d6;font-size:16px;line-height:1.6;">
                        Du hast eine Anfrage zum Zurücksetzen deines Passworts gestellt. Bitte gib den folgenden Code ein:
                      </p>
                      <!-- Code Box -->
                      <div style="background:#0f1117;border:1px solid #2a2d3e;border-radius:12px;padding:28px;text-align:center;margin:0 0 28px;">
                        <p style="margin:0 0 8px;color:#7b80a0;font-size:12px;text-transform:uppercase;letter-spacing:2px;">Dein Code</p>
                        <span style="font-size:42px;font-weight:700;letter-spacing:12px;color:#ff6b6b;font-family:'Courier New',monospace;">{code}</span>
                      </div>
                      <p style="margin:0 0 8px;color:#7b80a0;font-size:13px;">
                        ⏱ Dieser Code ist <strong style="color:#c0c4d6;">15 Minuten</strong> gültig.
                      </p>
                      <p style="margin:0;color:#7b80a0;font-size:13px;">
                        Falls du diese Anfrage nicht gestellt hast, ignoriere diese E-Mail bitte. Dein Konto ist sicher.
                      </p>
                    </td>
                  </tr>
                  <!-- Footer -->
                  <tr>
                    <td style="padding:20px 40px 32px;border-top:1px solid #2a2d3e;">
                      <p style="margin:0;color:#4a4e6a;font-size:12px;text-align:center;">
                        © {DateTime.UtcNow.Year} Investra · Lotac GmbH · Alle Rechte vorbehalten
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
}

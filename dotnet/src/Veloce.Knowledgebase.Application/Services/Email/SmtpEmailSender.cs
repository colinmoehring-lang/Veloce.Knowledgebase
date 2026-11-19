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
        var plainContent = $"Dein Verifizierungscode lautet: {code}\n\nDer Code ist 15 Minuten gÃ¼ltig.";

        await SendEmailAsync(email, subject, htmlContent, plainContent, cancellationToken);
    }

    public async Task SendPasswordResetCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        var subject = "Dein Investra Passwort-Reset-Code";
        var htmlContent = BuildPasswordResetCodeEmail(code);
        var plainContent = $"Dein Passwort-Reset-Code lautet: {code}\n\nDer Code ist 15 Minuten gÃ¼ltig.";

        await SendEmailAsync(email, subject, htmlContent, plainContent, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string plainContent,
        CancellationToken cancellationToken)
    {
        var host = _configuration["Smtp:Host"] ?? "smtp-relay.brevo.com";
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
        <body style="margin:0;padding:0;background:#F6F6F3;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#F6F6F3;padding:40px 0;">
            <tr>
              <td align="center">
                <table width="560" cellpadding="0" cellspacing="0" style="background:#FFFFFF;border-radius:24px;overflow:hidden;border:1px solid #ECECE8;">
                  <!-- Header -->
                  <tr>
                    <td style="background:linear-gradient(135deg,#E58A45,#FFB76B);padding:40px 40px 32px;text-align:center;">
                      <h1 style="margin:0;color:#171717;font-size:28px;font-weight:700;letter-spacing:-0.5px;">Investra</h1>
                      <p style="margin:8px 0 0;color:#888985;font-size:14px;">E-Mail-Verifizierung</p>
                    </td>
                  </tr>
                  <!-- Body -->
                  <tr>
                    <td style="padding:40px;">
                      <p style="margin:0 0 24px;color:#171717;font-size:16px;line-height:1.6;">
                        Willkommen bei Investra! Bitte gib den folgenden Code ein, um deine E-Mail-Adresse zu bestÃ¤tigen:
                      </p>
                      <!-- Code Box -->
                      <div style="background:#FFF0E4;border:1px solid #ECECE8;border-radius:16px;padding:28px;text-align:center;margin:0 0 28px;">
                        <p style="margin:0 0 8px;color:#888985;font-size:12px;text-transform:uppercase;letter-spacing:2px;">Dein Code</p>
                        <span style="font-size:42px;font-weight:700;letter-spacing:12px;color:#E58A45;font-family:'Courier New',monospace;">{code}</span>
                      </div>
                      <p style="margin:0 0 8px;color:#888985;font-size:13px;">
                        â± Dieser Code ist <strong style="color:#171717;">15 Minuten</strong> gÃ¼ltig.
                      </p>
                      <p style="margin:0;color:#888985;font-size:13px;">
                        Falls du kein Konto erstellt hast, ignoriere diese E-Mail bitte.
                      </p>
                    </td>
                  </tr>
                  <!-- Footer -->
                  <tr>
                    <td style="padding:20px 40px 32px;border-top:1px solid #ECECE8;">
                      <p style="margin:0;color:#888985;font-size:12px;text-align:center;">
                        Â© {DateTime.UtcNow.Year} Investra Â· Lotac GmbH Â· Alle Rechte vorbehalten
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
          <title>Passwort zurÃ¼cksetzen</title>
        </head>
        <body style="margin:0;padding:0;background:#F6F6F3;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#F6F6F3;padding:40px 0;">
            <tr>
              <td align="center">
                <table width="560" cellpadding="0" cellspacing="0" style="background:#FFFFFF;border-radius:24px;overflow:hidden;border:1px solid #ECECE8;">
                  <!-- Header -->
                  <tr>
                    <td style="background:linear-gradient(135deg,#E58A45,#FFB76B);padding:40px 40px 32px;text-align:center;">
                      <h1 style="margin:0;color:#171717;font-size:28px;font-weight:700;letter-spacing:-0.5px;">Investra</h1>
                      <p style="margin:8px 0 0;color:#888985;font-size:14px;">Passwort zurÃ¼cksetzen</p>
                    </td>
                  </tr>
                  <!-- Body -->
                  <tr>
                    <td style="padding:40px;">
                      <p style="margin:0 0 24px;color:#171717;font-size:16px;line-height:1.6;">
                        Du hast eine Anfrage zum ZurÃ¼cksetzen deines Passworts gestellt. Bitte gib den folgenden Code ein:
                      </p>
                      <!-- Code Box -->
                      <div style="background:#FFF0E4;border:1px solid #ECECE8;border-radius:16px;padding:28px;text-align:center;margin:0 0 28px;">
                        <p style="margin:0 0 8px;color:#888985;font-size:12px;text-transform:uppercase;letter-spacing:2px;">Dein Code</p>
                        <span style="font-size:42px;font-weight:700;letter-spacing:12px;color:#E58A45;font-family:'Courier New',monospace;">{code}</span>
                      </div>
                      <p style="margin:0 0 8px;color:#888985;font-size:13px;">
                        â± Dieser Code ist <strong style="color:#171717;">15 Minuten</strong> gÃ¼ltig.
                      </p>
                      <p style="margin:0;color:#888985;font-size:13px;">
                        Falls du diese Anfrage nicht gestellt hast, ignoriere diese E-Mail bitte. Dein Konto ist sicher.
                      </p>
                    </td>
                  </tr>
                  <!-- Footer -->
                  <tr>
                    <td style="padding:20px 40px 32px;border-top:1px solid #ECECE8;">
                      <p style="margin:0;color:#888985;font-size:12px;text-align:center;">
                        Â© {DateTime.UtcNow.Year} Investra Â· Lotac GmbH Â· Alle Rechte vorbehalten
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



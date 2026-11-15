using System.Net;
using Terrun.Infrastructure.Email;

namespace Terrun.Api.Modules.Identity.Services;

// The email the runner reads is in Spanish (standards §1: visible texts in Spanish, code in English)
internal static class VerificationEmail
{
    private const string _subject = "Tu código para verificar tu correo en Terrun";

    public static EmailMessage Build(string to, string displayName, string code)
    {
        var text = $"""
            Hola, {displayName}:

            Tu código de verificación de Terrun es {code}.
            Escríbelo en la app para confirmar tu correo. Vence en 24 horas.

            Si no creaste una cuenta en Terrun, ignora este correo.
            """;

        var safeName = WebUtility.HtmlEncode(displayName);
        var html = $"""
            <p>Hola, {safeName}:</p>
            <p>Tu código de verificación de Terrun es:</p>
            <p style="font-size:28px;font-weight:bold;letter-spacing:6px">{code}</p>
            <p>Escríbelo en la app para confirmar tu correo. Vence en 24 horas.</p>
            <p>Si no creaste una cuenta en Terrun, ignora este correo.</p>
            """;

        return new EmailMessage(to, _subject, text, html, EmailKind.EmailVerification, SecretForDevelopment: code);
    }
}

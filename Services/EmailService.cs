using FlightBookingAPI.Models;
using FlightBookingAPI.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
namespace FlightBookingAPI.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task EnviarCorreoRecuperacion(string correoDestino, string token)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("SkyTracker", _settings.SenderEmail));
            mensaje.To.Add(new MailboxAddress("", correoDestino));
            mensaje.Subject = "Recupera tu contraseña - SkyTracker";
            var link = $"http://localhost:3000/auth/reset-password?token={token}";
            mensaje.Body = new TextPart("html")
            {
                Text = $@"
                    <div style='font-family: Arial, sans-serif; background-color: #0a0a0a; padding: 40px; color: #ffffff;'>
                        <div style='max-width: 480px; margin: 0 auto; background-color: #111111; border: 1px solid #27272a; border-radius: 16px; padding: 32px;'>
                            <h2 style='color: #22d3ee; margin-bottom: 8px;'>SkyTracker</h2>
                            <p style='color: #a1a1aa; font-size: 14px; margin-bottom: 24px;'>Recuperación de contraseña</p>
                            <p style='color: #e4e4e7; font-size: 15px; line-height: 1.5;'>
                                Recibimos una solicitud para restablecer tu contraseña. Haz click en el botón de abajo para crear una nueva.
                            </p>
                            <a href='{link}' style='display: inline-block; margin-top: 24px; padding: 12px 28px; background: linear-gradient(to right, #06b6d4, #0891b2); color: #ffffff; text-decoration: none; border-radius: 12px; font-weight: 600;'>
                                Restablecer contraseña
                            </a>
                            <p style='color: #71717a; font-size: 13px; margin-top: 24px;'>
                                Este enlace expira en 30 minutos. Si no solicitaste esto, ignora este correo.
                            </p>
                        </div>
                    </div>"
            };
            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
            await client.SendAsync(mensaje);
            await client.DisconnectAsync(true);
        }

        public async Task EnviarCorreoBoleto(string correoDestino, Boleto boleto)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("SkyTracker", _settings.SenderEmail));
            mensaje.To.Add(new MailboxAddress(boleto.DatosPasajero.Nombres, correoDestino));
            mensaje.Subject = $"Tu boleto {boleto.CodigoReserva} - SkyTracker";

            mensaje.Body = new TextPart("html")
            {
                Text = $@"
                    <div style='font-family: Arial, sans-serif; background-color: #0a0a0a; padding: 40px; color: #ffffff;'>
                        <div style='max-width: 480px; margin: 0 auto; background-color: #111111; border: 1px solid #27272a; border-radius: 16px; padding: 32px;'>
                            <h2 style='color: #22d3ee; margin-bottom: 8px;'>SkyTracker</h2>
                            <p style='color: #a1a1aa; font-size: 14px; margin-bottom: 24px;'>Confirmación de reserva</p>
                            <p style='color: #e4e4e7; font-size: 15px; line-height: 1.5;'>
                                ¡Hola {boleto.DatosPasajero.Nombres}! Tu boleto fue generado correctamente. Aquí tienes los detalles:
                            </p>
                            <div style='background-color: #18181b; border: 1px solid #27272a; border-radius: 12px; padding: 20px; margin-top: 20px;'>
                                <p style='margin: 6px 0; color: #e4e4e7;'><strong style='color: #22d3ee;'>Código de reserva:</strong> {boleto.CodigoReserva}</p>
                                <p style='margin: 6px 0; color: #e4e4e7;'><strong style='color: #22d3ee;'>Pasajero:</strong> {boleto.DatosPasajero.Nombres}</p>
                                <p style='margin: 6px 0; color: #e4e4e7;'><strong style='color: #22d3ee;'>Documento:</strong> {boleto.DatosPasajero.Documento}</p>
                                <p style='margin: 6px 0; color: #e4e4e7;'><strong style='color: #22d3ee;'>Asiento:</strong> {boleto.DatosPasajero.Asiento}</p>
                                <p style='margin: 6px 0; color: #e4e4e7;'><strong style='color: #22d3ee;'>Clase:</strong> {boleto.DatosPasajero.Clase}</p>
                                <p style='margin: 6px 0; color: #e4e4e7;'><strong style='color: #22d3ee;'>Monto pagado:</strong> {boleto.Precio.Monto} {boleto.Precio.Moneda}</p>
                                <p style='margin: 6px 0; color: #e4e4e7;'><strong style='color: #22d3ee;'>Estado de pago:</strong> {boleto.EstadoPago}</p>
                            </div>
                            <p style='color: #71717a; font-size: 13px; margin-top: 24px;'>
                                Guarda este código, lo necesitarás para hacer seguimiento de tu reserva.
                            </p>
                        </div>
                    </div>"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
            await client.SendAsync(mensaje);
            await client.DisconnectAsync(true);
        }
    }
}
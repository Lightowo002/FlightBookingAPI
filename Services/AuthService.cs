using FlightBookingAPI.Models;
using FlightBookingAPI.Settings;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace FlightBookingAPI.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<Usuario> _usuariosCollection;
        private readonly EmailService _emailService;

        public AuthService(IOptions<MongoDBSettings> settings, EmailService emailService)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _usuariosCollection = database.GetCollection<Usuario>("usuarios");
            _emailService = emailService;
        }

        public async Task Registrar(Usuario usuario)
        {
            if (usuario.TipoDocumento == "DNI" && !Regex.IsMatch(usuario.NumeroDocumento, @"^[0-9]{8}$"))
            {
                throw new ArgumentException("El DNI debe tener exactamente 8 dígitos numéricos");
            }

            if (usuario.TipoDocumento == "CE" && !Regex.IsMatch(usuario.NumeroDocumento, @"^[0-9A-Za-z]{9,12}$"))
            {
                throw new ArgumentException("El Carnet de Extranjería debe tener entre 9 y 12 caracteres alfanuméricos");
            }

            var existente = await _usuariosCollection.Find(u => u.Correo == usuario.Correo).FirstOrDefaultAsync();
            if (existente != null)
            {
                throw new ArgumentException("Ese correo ya está registrado");
            }

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
            await _usuariosCollection.InsertOneAsync(usuario);
        }

        public async Task<Usuario> Login(string correo, string contraseña)
        {
            var filter = Builders<Usuario>.Filter.Eq(u => u.Correo, correo);
            var usuario = await _usuariosCollection.Find(filter).FirstOrDefaultAsync();

            if (usuario == null) return null;

            bool contraseñaCorrecta = BCrypt.Net.BCrypt.Verify(contraseña, usuario.Contraseña);

            if (!contraseñaCorrecta) return null;

            return usuario;
        }

        public async Task<Usuario> LoginConGoogle(string token)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(token);
            }
            catch (Exception)
            {
                throw new ArgumentException("Token de Google inválido");
            }

            var correoGoogle = payload.Email;

            var filter = Builders<Usuario>.Filter.Eq(u => u.Correo, correoGoogle);
            var usuario = await _usuariosCollection.Find(filter).FirstOrDefaultAsync();

            if (usuario == null)
            {
                throw new ArgumentException("Este correo no tiene una cuenta registrada");
            }

            return usuario;
        }

        public async Task SolicitarRecuperacion(string correo)
        {
            var usuario = await _usuariosCollection.Find(u => u.Correo == correo).FirstOrDefaultAsync();

            if (usuario == null)
            {
                throw new ArgumentException("Este correo no tiene una cuenta registrada");
            }

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");

            var update = Builders<Usuario>.Update
                .Set(u => u.ResetToken, token)
                .Set(u => u.ResetTokenExpira, DateTime.UtcNow.AddMinutes(30));

            await _usuariosCollection.UpdateOneAsync(u => u.Correo == correo, update);

            await _emailService.EnviarCorreoRecuperacion(correo, token);
        }

        public async Task RestablecerContraseña(string token, string nuevaContraseña)
        {
            var usuario = await _usuariosCollection.Find(u => u.ResetToken == token).FirstOrDefaultAsync();

            if (usuario == null || usuario.ResetTokenExpira < DateTime.UtcNow)
            {
                throw new ArgumentException("El enlace es inválido o ha expirado");
            }

            var nuevaContraseñaHash = BCrypt.Net.BCrypt.HashPassword(nuevaContraseña);

            var update = Builders<Usuario>.Update
                .Set(u => u.Contraseña, nuevaContraseñaHash)
                .Set(u => u.ResetToken, (string?)null)
                .Set(u => u.ResetTokenExpira, (DateTime?)null);

            await _usuariosCollection.UpdateOneAsync(u => u.Id == usuario.Id, update);
        }
    }
}
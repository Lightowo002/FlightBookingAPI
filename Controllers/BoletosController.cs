using FlightBookingAPI.Models;
using FlightBookingAPI.Services;
using FlightBookingAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FlightBookingAPI.Controllers
{
    [ApiController]
    [Route("api/boletos")]
    public class BoletosController : ControllerBase
    {
        private readonly IMongoCollection<Boleto> _boletos;
        private readonly EmailService _emailService;

        public BoletosController(IOptions<MongoDBSettings> mongoSettings, EmailService emailService)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _boletos = database.GetCollection<Boleto>("Boletos");
            _emailService = emailService;
        }

        // POST /api/boletos
        // Crea uno o más boletos al confirmar la compra
        [HttpPost]
        public async Task<IActionResult> CrearBoletos([FromBody] CrearBoletoRequest request)
        {
            if (request.Pasajeros == null || request.Pasajeros.Count == 0)
                return BadRequest(new { message = "Se requiere al menos un pasajero." });

            // Validar que vueloId sea un ObjectId válido
            if (string.IsNullOrEmpty(request.VueloId) || !MongoDB.Bson.ObjectId.TryParse(request.VueloId, out _))
                return BadRequest(new { message = "vueloId inválido." });

            var boletosCreados = new List<Boleto>();

            for (int i = 0; i < request.Pasajeros.Count; i++)
            {
                var pasajero = request.Pasajeros[i];
                var asiento = request.Asientos.Count > i ? request.Asientos[i] : "Sin asignar";
                var clase = request.Clases.Count > i ? request.Clases[i] : "Económica";

                // Precio dividido equitativamente entre pasajeros
                var monto = Math.Round(request.MontoTotal / request.Pasajeros.Count, 2);

                var boleto = new Boleto
                {
                    CodigoReserva = GenerarCodigoReserva(),
                    UsuarioId = request.UsuarioId,
                    VueloId = request.VueloId,
                    DatosPasajero = new DatosPasajero
                    {
                        Nombres = pasajero.Nombres,
                        Documento = pasajero.Documento,
                        Correo = pasajero.Correo,
                        Asiento = asiento,
                        Clase = clase,
                    },
                    Precio = new PrecioBoleto
                    {
                        Monto = monto,
                        Moneda = request.Moneda,
                    },
                    EstadoPago = "Pagado",
                    FechaCompra = DateTime.UtcNow,
                };

                boletosCreados.Add(boleto);
            }

            await _boletos.InsertManyAsync(boletosCreados);

            // Enviar correo de confirmación a cada pasajero que tenga correo válido
            foreach (var boleto in boletosCreados)
            {
                if (!string.IsNullOrWhiteSpace(boleto.DatosPasajero.Correo))
                {
                    try
                    {
                        await _emailService.EnviarCorreoBoleto(boleto.DatosPasajero.Correo, boleto);
                    }
                    catch (Exception)
                    {
                        // No interrumpe la compra si el correo falla, pero podría loguearse
                    }
                }
            }

            return Ok(new
            {
                message = "Boletos creados exitosamente.",
                codigosReserva = boletosCreados.Select(b => b.CodigoReserva).ToList(),
                boletos = boletosCreados.Select(b => new
                {
                    id = b.Id,
                    codigoReserva = b.CodigoReserva,
                    pasajero = b.DatosPasajero.Nombres,
                    asiento = b.DatosPasajero.Asiento,
                    clase = b.DatosPasajero.Clase,
                    monto = b.Precio.Monto,
                    moneda = b.Precio.Moneda,
                    fechaCompra = b.FechaCompra,
                })
            });
        }

        // GET /api/boletos/usuario/{usuarioId}
        // Devuelve todos los boletos de un usuario
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetBoletosPorUsuario(string usuarioId)
        {
            var boletos = await _boletos
                .Find(b => b.UsuarioId == usuarioId)
                .SortByDescending(b => b.FechaCompra)
                .ToListAsync();

            return Ok(boletos);
        }

        // GET /api/boletos/vuelo/{vueloId}/asientos-ocupados
        // Devuelve los asientos ya comprados para un vuelo
        [HttpGet("vuelo/{vueloId}/asientos-ocupados")]
        public async Task<IActionResult> GetAsientosOcupados(string vueloId)
        {
            var boletos = await _boletos
                .Find(b => b.VueloId == vueloId)
                .ToListAsync();

            var asientosOcupados = boletos
                .Select(b => b.DatosPasajero.Asiento)
                .Where(a => !string.IsNullOrEmpty(a))
                .Distinct()
                .ToList();

            return Ok(new { vueloId, asientosOcupados });
        }

        // GET /api/boletos/{codigoReserva}
        // Busca un boleto por código de reserva
        [HttpGet("{codigoReserva}")]
        public async Task<IActionResult> GetBoletoPorCodigo(string codigoReserva)
        {
            var boleto = await _boletos
                .Find(b => b.CodigoReserva == codigoReserva)
                .FirstOrDefaultAsync();

            if (boleto == null)
                return NotFound(new { message = "Boleto no encontrado." });

            return Ok(boleto);
        }

        private static string GenerarCodigoReserva()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var codigo = new string(Enumerable.Range(0, 8)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
            return $"R-{codigo}";
        }
    }
}
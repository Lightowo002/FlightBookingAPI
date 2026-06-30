using FlightBookingAPI.Models;
using FlightBookingAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FlightBookingAPI.Controllers
{
    [ApiController]
    [Route("api/vuelos")]
    public class VuelosController : ControllerBase
    {
        private readonly IMongoCollection<Vuelo> _vuelos;

        public VuelosController(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _vuelos = database.GetCollection<Vuelo>("Vuelos");
        }

        // GET /api/vuelos
        [HttpGet]
        public async Task<IActionResult> GetVuelos()
        {
            var vuelos = await _vuelos.Find(_ => true).ToListAsync();
            return Ok(vuelos);
        }

        // GET /api/vuelos/buscar?origen=Lima&destino=Arequipa&fecha=2026-07-05&pasajeros=1
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarVuelos(
            [FromQuery] string? origen,
            [FromQuery] string? destino,
            [FromQuery] string? fecha,
            [FromQuery] int pasajeros = 1)
        {
            var builder = Builders<Vuelo>.Filter;

            // Filtros de ruta (origen + destino) — se reusan para la búsqueda alternativa
            var filtrosRuta = new List<FilterDefinition<Vuelo>>();

            if (!string.IsNullOrEmpty(origen))
            {
                filtrosRuta.Add(builder.Or(
                    builder.Regex("origen.ciudad", new MongoDB.Bson.BsonRegularExpression(origen, "i")),
                    builder.Regex("origen.codigo_iata", new MongoDB.Bson.BsonRegularExpression(origen, "i"))
                ));
            }

            if (!string.IsNullOrEmpty(destino))
            {
                filtrosRuta.Add(builder.Or(
                    builder.Regex("destino.ciudad", new MongoDB.Bson.BsonRegularExpression(destino, "i")),
                    builder.Regex("destino.codigo_iata", new MongoDB.Bson.BsonRegularExpression(destino, "i"))
                ));
            }

            filtrosRuta.Add(builder.In("estado_actual", new[] { "Programado", "En espera" }));

            var filtroRuta = filtrosRuta.Count > 0 ? builder.And(filtrosRuta) : builder.Empty;

            // 1. Buscar con fecha exacta
            DateTime fechaParsed = DateTime.MinValue;
            bool busquedaConFecha = !string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out fechaParsed);
            List<Vuelo> vuelos = new();
            bool esFechaAlternativa = false;

            if (busquedaConFecha)
            {
                var inicioDia = fechaParsed.Date;
                var finDia = inicioDia.AddDays(1);

                var filtroConFecha = builder.And(
                    filtroRuta,
                    builder.Gte("fecha_hora_partida", inicioDia),
                    builder.Lt("fecha_hora_partida", finDia)
                );

                vuelos = await _vuelos.Find(filtroConFecha).ToListAsync();

                // 2. Si no hay resultados, buscar misma ruta sin filtro de fecha
                if (vuelos.Count == 0)
                {
                    vuelos = await _vuelos.Find(filtroRuta).ToListAsync();
                    esFechaAlternativa = true;
                }
            }
            else
            {
                vuelos = await _vuelos.Find(filtroRuta).ToListAsync();
            }

            var resultado = vuelos.Select(v => new
            {
                id = v.Id,
                codigoVuelo = v.CodigoVuelo,
                origen = new
                {
                    ciudad = v.Origen.Ciudad,
                    aeropuerto = v.Origen.Aeropuerto,
                    codigoIata = v.Origen.CodigoIata ?? v.Origen.Ciudad.Substring(0, 3).ToUpper()
                },
                destino = new
                {
                    ciudad = v.Destino.Ciudad,
                    aeropuerto = v.Destino.Aeropuerto,
                    codigoIata = v.Destino.CodigoIata ?? v.Destino.Ciudad.Substring(0, 3).ToUpper()
                },
                fechaHoraPartida = v.FechaHoraPartida,
                fechaHoraLlegadaEstimada = v.FechaHoraLlegadaEstimada,
                duracionMinutos = (int)(v.FechaHoraLlegadaEstimada - v.FechaHoraPartida).TotalMinutes,
                estadoActual = v.EstadoActual,
                avion = v.InformacionAvion.Modelo,
                capacidadPasajeros = v.InformacionAvion.CapacidadPasajeros,
                precio = v.Precio,
                esFechaAlternativa  // true si no había vuelos en la fecha pedida
            });

            return Ok(new
            {
                vuelos = resultado,
                esFechaAlternativa,
                fechaBuscada = busquedaConFecha ? fechaParsed.ToString("yyyy-MM-dd") : null
            });
        }

        // GET /api/vuelos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVueloPorId(string id)
        {
            var vuelo = await _vuelos.Find(v => v.Id == id).FirstOrDefaultAsync();
            if (vuelo == null)
                return NotFound(new { message = "Vuelo no encontrado" });

            return Ok(new
            {
                id = vuelo.Id,
                codigoVuelo = vuelo.CodigoVuelo,
                origen = new
                {
                    ciudad = vuelo.Origen.Ciudad,
                    aeropuerto = vuelo.Origen.Aeropuerto,
                    codigoIata = vuelo.Origen.CodigoIata ?? vuelo.Origen.Ciudad.Substring(0, 3).ToUpper()
                },
                destino = new
                {
                    ciudad = vuelo.Destino.Ciudad,
                    aeropuerto = vuelo.Destino.Aeropuerto,
                    codigoIata = vuelo.Destino.CodigoIata ?? vuelo.Destino.Ciudad.Substring(0, 3).ToUpper()
                },
                fechaHoraPartida = vuelo.FechaHoraPartida,
                fechaHoraLlegadaEstimada = vuelo.FechaHoraLlegadaEstimada,
                duracionMinutos = (int)(vuelo.FechaHoraLlegadaEstimada - vuelo.FechaHoraPartida).TotalMinutes,
                estadoActual = vuelo.EstadoActual,
                avion = vuelo.InformacionAvion.Modelo,
                capacidadPasajeros = vuelo.InformacionAvion.CapacidadPasajeros,
                precio = vuelo.Precio
            });
        }
    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlightBookingAPI.Models
{
    public class Vuelo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("codigo_vuelo")]
        public string CodigoVuelo { get; set; } = string.Empty;

        [BsonElement("origen")]
        public Ubicacion Origen { get; set; } = new();

        [BsonElement("destino")]
        public Ubicacion Destino { get; set; } = new();

        [BsonElement("fecha_hora_partida")]
        public DateTime FechaHoraPartida { get; set; }

        [BsonElement("fecha_hora_llegada_estimada")]
        public DateTime FechaHoraLlegadaEstimada { get; set; }

        [BsonElement("estado_actual")]
        public string EstadoActual { get; set; } = string.Empty;

        [BsonElement("informacion_avion")]
        public InformacionAvion InformacionAvion { get; set; } = new();

        [BsonElement("responsables_vuelo")]
        public List<Responsable> ResponsablesVuelo { get; set; } = new();

        [BsonElement("historial_retrasos")]
        public List<object> HistorialRetrasos { get; set; } = new();

        // Campo de precio — agrégalo a tus documentos en Mongo o se usará el default
        [BsonElement("precio")]
        public decimal Precio { get; set; } = 0;
    }

    public class Ubicacion
    {
        [BsonElement("ciudad")]
        public string Ciudad { get; set; } = string.Empty;

        [BsonElement("aeropuerto")]
        public string Aeropuerto { get; set; } = string.Empty;

        [BsonElement("codigo_iata")]
        public string? CodigoIata { get; set; }
    }

    public class InformacionAvion
    {
        [BsonElement("modelo")]
        public string Modelo { get; set; } = string.Empty;

        [BsonElement("matricula")]
        public string Matricula { get; set; } = string.Empty;

        [BsonElement("capacidad_pasajeros")]
        public int CapacidadPasajeros { get; set; }
    }

    public class Responsable
    {
        [BsonElement("rol")]
        public string Rol { get; set; } = string.Empty;

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
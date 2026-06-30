using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace FlightBookingAPI.Models
{
    public class Boleto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("codigo_reserva")]
        public string CodigoReserva { get; set; } = string.Empty;
        // String simple — no forzamos ObjectId para evitar errores si el id viene vacío
        [BsonElement("usuario_id")]
        public string UsuarioId { get; set; } = string.Empty;
        [BsonElement("vuelo_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VueloId { get; set; } = string.Empty;
        [BsonElement("datos_pasajero")]
        public DatosPasajero DatosPasajero { get; set; } = new();
        [BsonElement("precio")]
        public PrecioBoleto Precio { get; set; } = new();
        [BsonElement("estado_pago")]
        public string EstadoPago { get; set; } = "Pagado";
        [BsonElement("fecha_compra")]
        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    }
    public class DatosPasajero
    {
        [BsonElement("nombres")]
        public string Nombres { get; set; } = string.Empty;
        [BsonElement("documento")]
        public string Documento { get; set; } = string.Empty;
        [BsonElement("correo")]
        public string Correo { get; set; } = string.Empty;
        [BsonElement("asiento")]
        public string Asiento { get; set; } = string.Empty;
        [BsonElement("clase")]
        public string Clase { get; set; } = string.Empty;
    }
    public class PrecioBoleto
    {
        [BsonElement("monto")]
        public decimal Monto { get; set; }
        [BsonElement("moneda")]
        public string Moneda { get; set; } = "PEN";
    }
    public class CrearBoletoRequest
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string VueloId { get; set; } = string.Empty;
        public List<PasajeroRequest> Pasajeros { get; set; } = new();
        public List<string> Asientos { get; set; } = new();
        public List<string> Clases { get; set; } = new();
        public decimal MontoTotal { get; set; }
        public string Moneda { get; set; } = "PEN";
    }
    public class PasajeroRequest
    {
        public string Nombres { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }
}
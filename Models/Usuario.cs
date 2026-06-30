using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FlightBookingAPI.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [RegularExpression(@"^(DNI|CE)$", ErrorMessage = "El tipo de documento debe ser DNI o CE")]
        public string TipoDocumento { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        public string NumeroDocumento { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "El correo no tiene un formato válido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El telefono es obligatorio")]
        [RegularExpression(@"^9[0-9]{8}$", ErrorMessage = "El teléfono debe ser un número peruano válido de 9 dígitos")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contraseña { get; set; }

        public string Rol { get; set; } = "Pasajero";

        public DateTime FechaRegistro { get; set; } = DateTime.Today;

        public string? ResetToken { get; set; } = null;
        public DateTime? ResetTokenExpira { get; set; } = null;
    }
}
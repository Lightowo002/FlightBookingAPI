using System.ComponentModel.DataAnnotations;

namespace FlightBookingAPI.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contraseña { get; set; }
    }
}

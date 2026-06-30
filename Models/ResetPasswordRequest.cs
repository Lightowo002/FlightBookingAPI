namespace FlightBookingAPI.Models
{
    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NuevaContraseña { get; set; }
    }
}
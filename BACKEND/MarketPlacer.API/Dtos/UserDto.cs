namespace MarketPlacer.API.Dtos
{
    public class RegisterRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Comum"; // "Vendedor" ou "Comum"
    }
}
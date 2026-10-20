namespace LibroReclamaciones.Models;

public sealed class Proveedor
{
    public string RazonSocial { get; set; } = "";
    public string Ruc { get; set; } = "";
    public string Domicilio { get; set; } = "";
    public string CodigoIdentificacion { get; set; } = "";
    public string Ciudad { get; set; } = "";
    public string Representante { get; set; } = "";
    public string Cargo { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Telefono { get; set; } = "";
    public int SiguienteNumero { get; set; } = 1;

    public Proveedor Clonar() => new()
    {
        RazonSocial = RazonSocial,
        Ruc = Ruc,
        Domicilio = Domicilio,
        CodigoIdentificacion = CodigoIdentificacion,
        Ciudad = Ciudad,
        Representante = Representante,
        Cargo = Cargo,
        Correo = Correo,
        Telefono = Telefono,
        SiguienteNumero = SiguienteNumero
    };

    public bool EstaCompleto =>
        !string.IsNullOrWhiteSpace(RazonSocial) &&
        !string.IsNullOrWhiteSpace(Ruc) &&
        !string.IsNullOrWhiteSpace(Domicilio);
}

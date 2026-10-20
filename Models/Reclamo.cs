using System.Text.Json.Serialization;
using LibroReclamaciones.Services;

namespace LibroReclamaciones.Models;

public sealed class Reclamo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Numero { get; set; }
    public int Anio { get; set; } = DateTime.Now.Year;
    public DateOnly FechaReclamo { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string NombreConsumidor { get; set; } = "";
    public string DomicilioConsumidor { get; set; } = "";
    public string TipoDocumento { get; set; } = "DNI";
    public string NumeroDocumento { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";

    public bool EsMenor { get; set; }
    public string NombrePadre { get; set; } = "";
    public string DocumentoPadre { get; set; } = "";
    public string TelefonoPadre { get; set; } = "";
    public string EmailPadre { get; set; } = "";

    public string TipoBien { get; set; } = Tipos.Producto;
    public string DescripcionBien { get; set; } = "";
    public decimal? MontoReclamado { get; set; }

    public string Tipo { get; set; } = Tipos.Reclamo;
    public string Detalle { get; set; } = "";
    public string Pedido { get; set; } = "";
    public List<string> ImagenesConsumidor { get; set; } = [];

    public string TipoAtencion { get; set; } = "";
    public string Resultado { get; set; } = "";
    public string Canal { get; set; } = "";
    public string Observaciones { get; set; } = "";
    public string Fundamento { get; set; } = "";
    public List<string> ImagenesRespuesta { get; set; } = [];
    public DateOnly? FechaComunicacion { get; set; }
    public bool HuboPropuesta { get; set; }
    public string Estado { get; set; } = "Borrador";

    public string ProveedorRazonSocial { get; set; } = "";
    public string ProveedorRuc { get; set; } = "";
    public string ProveedorDomicilio { get; set; } = "";
    public string ProveedorCodigo { get; set; } = "";
    public string ProveedorCiudad { get; set; } = "";
    public string ProveedorRepresentante { get; set; } = "";
    public string ProveedorCargo { get; set; } = "";
    public string ProveedorCorreo { get; set; } = "";
    public string ProveedorTelefono { get; set; } = "";

    public DateTime CreadoEn { get; set; } = DateTime.Now;
    public DateTime ActualizadoEn { get; set; } = DateTime.Now;

    [JsonIgnore]
    public string NumeroTexto => Formato.NumeroHoja(Numero, Anio);

    public void CopiarMembrete(Proveedor proveedor)
    {
        ProveedorRazonSocial = proveedor.RazonSocial.Trim();
        ProveedorRuc = proveedor.Ruc.Trim();
        ProveedorDomicilio = proveedor.Domicilio.Trim();
        ProveedorCodigo = proveedor.CodigoIdentificacion.Trim();
        ProveedorCiudad = proveedor.Ciudad.Trim();
        ProveedorRepresentante = proveedor.Representante.Trim();
        ProveedorCargo = proveedor.Cargo.Trim();
        ProveedorCorreo = proveedor.Correo.Trim();
        ProveedorTelefono = proveedor.Telefono.Trim();
    }
}

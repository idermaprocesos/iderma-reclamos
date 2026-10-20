namespace LibroReclamaciones.Models;

public sealed class ArchivoDatos
{
    public Proveedor Proveedor { get; set; } = new();
    public List<Reclamo> Reclamos { get; set; } = [];
}

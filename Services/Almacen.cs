using System.Text.Json;
using LibroReclamaciones.Models;

namespace LibroReclamaciones.Services;

public sealed class Almacen
{
    public static Almacen Actual { get; } = new();

    private readonly object _sync = new();
    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ArchivoDatos Datos { get; private set; } = new();
    public string Carpeta { get; }
    public string RutaArchivo { get; }

    private Almacen()
    {
        Carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LibroReclamaciones");
        RutaArchivo = Path.Combine(Carpeta, "libro.json");
    }

    public void Cargar()
    {
        lock (_sync)
        {
            Directory.CreateDirectory(Carpeta);
            if (!File.Exists(RutaArchivo))
            {
                Datos = new ArchivoDatos();
                return;
            }

            try
            {
                var json = File.ReadAllText(RutaArchivo);
                Datos = JsonSerializer.Deserialize<ArchivoDatos>(json, Opciones) ?? new ArchivoDatos();
            }
            catch (Exception ex) when (ex is JsonException or IOException)
            {
                var respaldo = RutaArchivo + ".danado-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Copy(RutaArchivo, respaldo, overwrite: true);
                Datos = new ArchivoDatos();
            }

            ImagenesRespuesta.LimpiarHuerfanas(Datos.Reclamos);
        }
    }

    public void Guardar()
    {
        lock (_sync)
        {
            Directory.CreateDirectory(Carpeta);
            if (File.Exists(RutaArchivo))
                File.Copy(RutaArchivo, RutaArchivo + ".bak", overwrite: true);

            var temporal = RutaArchivo + ".tmp";
            File.WriteAllText(temporal, JsonSerializer.Serialize(Datos, Opciones));
            File.Move(temporal, RutaArchivo, overwrite: true);
        }
    }

    public Reclamo? Buscar(Guid id)
    {
        lock (_sync)
            return Datos.Reclamos.FirstOrDefault(r => r.Id == id);
    }

    public bool ExisteNumero(Guid id, int numero, int anio)
    {
        lock (_sync)
            return Datos.Reclamos.Any(r => r.Id != id && r.Numero == numero && r.Anio == anio);
    }

    public void GuardarReclamo(Reclamo reclamo)
    {
        lock (_sync)
        {
            reclamo.ActualizadoEn = DateTime.Now;
            var indice = Datos.Reclamos.FindIndex(r => r.Id == reclamo.Id);
            if (indice >= 0)
                Datos.Reclamos[indice] = reclamo;
            else
                Datos.Reclamos.Add(reclamo);

            if (reclamo.Numero >= Datos.Proveedor.SiguienteNumero)
                Datos.Proveedor.SiguienteNumero = reclamo.Numero + 1;
        }

        Guardar();
    }

    public void Eliminar(Guid id)
    {
        lock (_sync)
            Datos.Reclamos.RemoveAll(r => r.Id == id);
        ImagenesRespuesta.EliminarCarpeta(id);
        Guardar();
    }

    public IReadOnlyList<Reclamo> Listar()
    {
        lock (_sync)
        {
            return Datos.Reclamos
                .OrderByDescending(r => r.FechaReclamo)
                .ThenByDescending(r => r.Anio)
                .ThenByDescending(r => r.Numero)
                .ToList();
        }
    }
}

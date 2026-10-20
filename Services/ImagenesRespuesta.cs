using LibroReclamaciones.Models;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace LibroReclamaciones.Services;

public static class ImagenesRespuesta
{
    private const int LadoMaximo = 1800;

    public static string Absoluta(string relativa)
    {
        var limpia = relativa
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        return Path.Combine(Almacen.Actual.Carpeta, limpia);
    }

    public static IReadOnlyList<string> Existentes(IEnumerable<string>? relativas)
    {
        if (relativas is null)
            return [];

        return relativas
            .Where(ruta => !string.IsNullOrWhiteSpace(ruta) && File.Exists(Absoluta(ruta)))
            .ToList();
    }

    public static async Task<string> GuardarAsync(Guid reclamoId, StorageFile origen)
    {
        var carpeta = CarpetaDe(reclamoId);
        Directory.CreateDirectory(carpeta);
        var nombre = Guid.NewGuid().ToString("N") + ".jpg";
        var destino = Path.Combine(carpeta, nombre);
        await ReducirYGuardarAsync(origen, destino);
        return Path.Combine("imagenes", reclamoId.ToString("N"), nombre);
    }

    public static void Conservar(Guid reclamoId, IEnumerable<string>? relativas)
    {
        var carpeta = CarpetaDe(reclamoId);
        if (!Directory.Exists(carpeta))
            return;

        var nombres = new HashSet<string>(
            (relativas ?? []).Select(ruta => Path.GetFileName(Absoluta(ruta))),
            StringComparer.OrdinalIgnoreCase);

        foreach (var archivo in Directory.GetFiles(carpeta))
        {
            if (!nombres.Contains(Path.GetFileName(archivo)))
                File.Delete(archivo);
        }

        if (Directory.GetFiles(carpeta).Length == 0 && Directory.GetDirectories(carpeta).Length == 0)
            Directory.Delete(carpeta);
    }

    public static void EliminarCarpeta(Guid reclamoId)
    {
        var carpeta = CarpetaDe(reclamoId);
        if (Directory.Exists(carpeta))
            Directory.Delete(carpeta, recursive: true);
    }

    public static void LimpiarHuerfanas(IEnumerable<Reclamo> reclamos)
    {
        var raiz = Path.Combine(Almacen.Actual.Carpeta, "imagenes");
        if (!Directory.Exists(raiz))
            return;

        var porId = reclamos.ToDictionary(
            reclamo => reclamo.Id.ToString("N"),
            reclamo => (reclamo.ImagenesConsumidor ?? []).Concat(reclamo.ImagenesRespuesta ?? []).ToList());

        foreach (var carpeta in Directory.GetDirectories(raiz))
        {
            var id = Path.GetFileName(carpeta);
            if (!porId.TryGetValue(id, out var relativas) || !Guid.TryParse(id, out var reclamoId))
            {
                Directory.Delete(carpeta, recursive: true);
                continue;
            }

            Conservar(reclamoId, relativas);
        }
    }

    private static string CarpetaDe(Guid reclamoId) =>
        Path.Combine(Almacen.Actual.Carpeta, "imagenes", reclamoId.ToString("N"));

    private static async Task ReducirYGuardarAsync(StorageFile origen, string destino)
    {
        using var entrada = await origen.OpenReadAsync();
        var decoder = await BitmapDecoder.CreateAsync(entrada);
        var ancho = decoder.OrientedPixelWidth;
        var alto = decoder.OrientedPixelHeight;
        var escala = Math.Min(1d, LadoMaximo / (double)Math.Max(ancho, alto));
        var nuevoAncho = (uint)Math.Max(1, Math.Round(ancho * escala));
        var nuevoAlto = (uint)Math.Max(1, Math.Round(alto * escala));

        var pixeles = await decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            new BitmapTransform
            {
                ScaledWidth = nuevoAncho,
                ScaledHeight = nuevoAlto,
                InterpolationMode = BitmapInterpolationMode.Fant
            },
            ExifOrientationMode.RespectExifOrientation,
            ColorManagementMode.DoNotColorManage);

        var propiedades = new BitmapPropertySet
        {
            { "ImageQuality", new BitmapTypedValue(0.85, PropertyType.Single) }
        };

        using var memoria = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, memoria, propiedades);
        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            nuevoAncho,
            nuevoAlto,
            96,
            96,
            pixeles.DetachPixelData());
        await encoder.FlushAsync();

        memoria.Seek(0);
        var buffer = new byte[(int)memoria.Size];
        using (var lector = new DataReader(memoria.GetInputStreamAt(0)))
        {
            await lector.LoadAsync((uint)memoria.Size);
            lector.ReadBytes(buffer);
        }

        await File.WriteAllBytesAsync(destino, buffer);
    }
}

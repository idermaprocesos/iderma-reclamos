using System.Globalization;
using System.Text;

namespace LibroReclamaciones.Services;

public static class Formato
{
    public static readonly CultureInfo Peru = new("es-PE");

    public static string Fecha(DateOnly? fecha) =>
        fecha?.ToString("dd/MM/yyyy", Peru) ?? "";

    public static string FechaLarga(DateOnly fecha) =>
        fecha.ToString("d 'de' MMMM 'de' yyyy", Peru);

    public static string Monto(decimal? monto) =>
        monto is null ? "" : $"S/ {monto.Value.ToString("N2", Peru)}";

    public static string NumeroHoja(int numero, int anio) =>
        $"{numero:000000000}-{anio}";

    public static string ArchivoSeguro(string nombre)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var limpio = new StringBuilder(nombre.Length);
        foreach (var caracter in nombre.Trim())
            limpio.Append(invalidos.Contains(caracter) ? '_' : caracter);

        var texto = limpio.ToString();
        return string.IsNullOrWhiteSpace(texto) ? "reclamo" : texto;
    }
}

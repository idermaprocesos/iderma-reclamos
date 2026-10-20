using LibroReclamaciones.Models;
using LibroReclamaciones.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LibroReclamaciones.Pages;

public sealed class FilaReclamo
{
    public required Reclamo Origen { get; init; }
    public string NumeroTexto { get; init; } = "";
    public string Consumidor { get; init; } = "";
    public string Detalle { get; init; } = "";
    public string Meta { get; init; } = "";
    public string Plazo { get; init; } = "";
    public string ClavePlazo { get; init; } = "";
}

public sealed partial class HistorialPage : Page
{
    private bool _cargandoFiltros;

    public HistorialPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        CargarAnios();
        AplicarFiltro();
    }

    private void CargarAnios()
    {
        _cargandoFiltros = true;
        var seleccionado = AnioFiltro.SelectedItem?.ToString();
        AnioFiltro.Items.Clear();
        AnioFiltro.Items.Add("Todos");
        foreach (var anio in Almacen.Actual.Listar().Select(r => r.Anio).Distinct().OrderByDescending(a => a))
            AnioFiltro.Items.Add(anio.ToString());

        Seleccionar(AnioFiltro, seleccionado ?? "Todos");
        if (AnioFiltro.SelectedIndex < 0)
            AnioFiltro.SelectedIndex = 0;
        _cargandoFiltros = false;
    }

    private void Busqueda_Changed(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded)
            AplicarFiltro();
    }

    private void Filtro_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_cargandoFiltros && IsLoaded)
            AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var texto = (Busqueda.Text ?? "").Trim();
        var estado = EstadoFiltro.SelectedItem?.ToString() ?? "Todos";
        var anio = AnioFiltro.SelectedItem?.ToString();

        var filas = new List<FilaReclamo>();
        foreach (var reclamo in Almacen.Actual.Listar())
        {
            if (anio is not null && anio != "Todos" && reclamo.Anio.ToString() != anio)
                continue;

            if (!Coincide(reclamo, texto))
                continue;

            var plazo = PlazoService.Evaluar(reclamo, hoy);
            if (!CoincideEstado(reclamo, plazo, estado))
                continue;

            var descripcion = string.IsNullOrWhiteSpace(reclamo.DescripcionBien)
                ? reclamo.Tipo
                : $"{reclamo.Tipo} · {reclamo.DescripcionBien}";
            filas.Add(new FilaReclamo
            {
                Origen = reclamo,
                NumeroTexto = "Hoja N.º " + reclamo.NumeroTexto,
                Consumidor = reclamo.NombreConsumidor,
                Detalle = descripcion,
                Meta = $"{Formato.Fecha(reclamo.FechaReclamo)} · {reclamo.Estado}" +
                       (string.IsNullOrWhiteSpace(reclamo.Resultado) ? "" : $" · {reclamo.Resultado}"),
                Plazo = plazo.Texto,
                ClavePlazo = plazo.Clave
            });
        }

        Lista.ItemsSource = filas;
        var hay = filas.Count > 0;
        Lista.Visibility = hay ? Visibility.Visible : Visibility.Collapsed;
        Vacio.Visibility = hay ? Visibility.Collapsed : Visibility.Visible;
        Vacio.Text = Almacen.Actual.Listar().Count == 0
            ? "Aún no hay reclamos guardados."
            : "Ningún reclamo coincide con la búsqueda.";
        Resumen.Text = filas.Count == 1 ? "1 hoja" : $"{filas.Count} hojas";
    }

    private static bool Coincide(Reclamo reclamo, string texto)
    {
        if (texto.Length == 0)
            return true;

        var numero = reclamo.NumeroTexto;
        return reclamo.NombreConsumidor.Contains(texto, StringComparison.CurrentCultureIgnoreCase)
            || reclamo.NumeroDocumento.Contains(texto, StringComparison.CurrentCultureIgnoreCase)
            || numero.Contains(texto, StringComparison.CurrentCultureIgnoreCase)
            || reclamo.Numero.ToString().Contains(texto, StringComparison.CurrentCultureIgnoreCase)
            || reclamo.DescripcionBien.Contains(texto, StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool CoincideEstado(Reclamo reclamo, EstadoPlazo plazo, string filtro) => filtro switch
    {
        "Pendientes" => reclamo.Estado is Tipos.Borrador or Tipos.PropuestaEnviada,
        "Propuestas" => reclamo.Estado == Tipos.PropuestaEnviada || reclamo.TipoAtencion == Tipos.Propuesta,
        "Respondidos" => reclamo.Estado == Tipos.Respondido,
        "Vencidos" => plazo.Clave == "vencido",
        _ => true
    };

    private void Editar_Click(object sender, RoutedEventArgs e)
    {
        if (FilaDe(sender) is not { } fila)
            return;
        App.Ventana.MostrarFormulario(fila.Origen.Id);
    }

    private async void Descargar_Click(object sender, RoutedEventArgs e)
    {
        if (FilaDe(sender) is not { } fila)
            return;

        var ruta = await Archivos.ElegirPdfAsync(App.Ventana, PdfExporter.NombreArchivo(fila.Origen));
        if (ruta is null)
            return;

        try
        {
            var reclamo = fila.Origen;
            await Task.Run(() => PdfExporter.Generar(reclamo, ruta));
            Archivos.Abrir(ruta);
        }
        catch (Exception ex)
        {
            var dialogo = new ContentDialog
            {
                Title = "No se pudo crear el PDF",
                Content = ex.Message,
                CloseButtonText = "Cerrar",
                XamlRoot = XamlRoot
            };
            await dialogo.ShowAsync();
        }
    }

    private async void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        if (FilaDe(sender) is not { } fila)
            return;

        var dialogo = new ContentDialog
        {
            Title = "Eliminar hoja",
            Content = $"Se eliminará del historial la hoja {fila.Origen.NumeroTexto} de {fila.Origen.NombreConsumidor}. Esta acción no borra un PDF que ya haya descargado.",
            PrimaryButtonText = "Eliminar",
            CloseButtonText = "Cancelar",
            XamlRoot = XamlRoot,
            DefaultButton = ContentDialogButton.Close
        };
        if (await dialogo.ShowAsync() != ContentDialogResult.Primary)
            return;

        Almacen.Actual.Eliminar(fila.Origen.Id);
        AplicarFiltro();
    }

    private static FilaReclamo? FilaDe(object sender) =>
        sender is FrameworkElement { DataContext: FilaReclamo fila } ? fila : null;

    private static void Seleccionar(ComboBox lista, string valor)
    {
        for (var i = 0; i < lista.Items.Count; i++)
        {
            if (lista.Items[i]?.ToString() == valor)
            {
                lista.SelectedIndex = i;
                return;
            }
        }
    }
}

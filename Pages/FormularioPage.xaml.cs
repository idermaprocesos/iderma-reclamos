using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using LibroReclamaciones.Models;
using LibroReclamaciones.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;

namespace LibroReclamaciones.Pages;

public sealed class MiniaturaImagen
{
    public required string Ruta { get; init; }
    public required ImageSource Vista { get; init; }
}

public sealed partial class FormularioPage : Page
{
    private Guid _id = Guid.NewGuid();
    private DateTime _creadoEn = DateTime.Now;
    private bool _huboPropuesta;
    private bool _listo;
    private int _cargaConsumidor;
    private int _cargaProveedor;
    private Proveedor _membrete = new();
    private readonly ObservableCollection<MiniaturaImagen> _imagenesConsumidor = [];
    private readonly ObservableCollection<MiniaturaImagen> _imagenesProveedor = [];

    public FormularioPage()
    {
        InitializeComponent();
        FechaRespuestaPicker.SelectedDate = null;
        ListaImagenesConsumidor.ItemsSource = _imagenesConsumidor;
        ListaImagenes.ItemsSource = _imagenesProveedor;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ActualizarAvisoProveedor();
        if (e.Parameter is Guid id)
        {
            var reclamo = Almacen.Actual.Buscar(id);
            if (reclamo is not null)
                Mostrar(reclamo);
            _listo = true;
            return;
        }

        if (!_listo)
        {
            Nuevo();
            _listo = true;
        }
        else
        {
            ActualizarDerivados();
        }
    }

    private void Nuevo_Click(object sender, RoutedEventArgs e) => Nuevo();

    private void Nuevo()
    {
        var proveedor = Almacen.Actual.Datos.Proveedor;
        ImagenesRespuesta.Conservar(_id, RutasGuardadas(_id));
        _cargaConsumidor++;
        _cargaProveedor++;
        _imagenesConsumidor.Clear();
        _imagenesProveedor.Clear();
        ActualizarVistaImagenes();
        _id = Guid.NewGuid();
        _creadoEn = DateTime.Now;
        _huboPropuesta = false;
        _membrete = proveedor.Clonar();

        NumeroBox.Value = Math.Max(1, proveedor.SiguienteNumero);
        AnioBox.Value = DateTime.Now.Year;
        FechaReclamoPicker.SelectedDate = DateTimeOffset.Now;
        NombreBox.Text = "";
        DomicilioBox.Text = "";
        TipoDocCombo.SelectedIndex = 0;
        DocumentoBox.Text = "";
        TelefonoBox.Text = "";
        EmailBox.Text = "";
        MenorSwitch.IsOn = false;
        PadreNombreBox.Text = "";
        PadreDocumentoBox.Text = "";
        PadreTelefonoBox.Text = "";
        PadreEmailBox.Text = "";
        TipoProducto.IsChecked = true;
        DescripcionBienBox.Text = "";
        MontoBox.Value = double.NaN;
        TipoReclamo.IsChecked = true;
        DetalleBox.Text = "";
        PedidoBox.Text = "";
        TipoAtencionCombo.SelectedIndex = -1;
        ResultadoCombo.SelectedIndex = -1;
        CanalCombo.SelectedIndex = -1;
        FechaRespuestaPicker.SelectedDate = null;
        ObservacionesBox.Text = "";
        FundamentoBox.Text = "";
        ActualizarDerivados();
        ActualizarAvisoProveedor();
    }

    private void Mostrar(Reclamo reclamo)
    {
        if (reclamo.Id != _id)
            ImagenesRespuesta.Conservar(_id, RutasGuardadas(_id));

        _id = reclamo.Id;
        _creadoEn = reclamo.CreadoEn;
        _huboPropuesta = reclamo.HuboPropuesta;
        _membrete = new Proveedor
        {
            RazonSocial = reclamo.ProveedorRazonSocial,
            Ruc = reclamo.ProveedorRuc,
            Domicilio = reclamo.ProveedorDomicilio,
            CodigoIdentificacion = reclamo.ProveedorCodigo,
            Ciudad = reclamo.ProveedorCiudad,
            Representante = reclamo.ProveedorRepresentante,
            Cargo = reclamo.ProveedorCargo,
            Correo = reclamo.ProveedorCorreo,
            Telefono = reclamo.ProveedorTelefono
        };

        NumeroBox.Value = reclamo.Numero;
        AnioBox.Value = reclamo.Anio;
        FechaReclamoPicker.SelectedDate = AFecha(reclamo.FechaReclamo);
        NombreBox.Text = reclamo.NombreConsumidor;
        DomicilioBox.Text = reclamo.DomicilioConsumidor;
        Seleccionar(TipoDocCombo, reclamo.TipoDocumento);
        DocumentoBox.Text = reclamo.NumeroDocumento;
        TelefonoBox.Text = reclamo.Telefono;
        EmailBox.Text = reclamo.Email;
        MenorSwitch.IsOn = reclamo.EsMenor;
        PadreNombreBox.Text = reclamo.NombrePadre;
        PadreDocumentoBox.Text = reclamo.DocumentoPadre;
        PadreTelefonoBox.Text = reclamo.TelefonoPadre;
        PadreEmailBox.Text = reclamo.EmailPadre;
        TipoProducto.IsChecked = reclamo.TipoBien != Tipos.Servicio;
        TipoServicio.IsChecked = reclamo.TipoBien == Tipos.Servicio;
        DescripcionBienBox.Text = reclamo.DescripcionBien;
        MontoBox.Value = reclamo.MontoReclamado is null ? double.NaN : (double)reclamo.MontoReclamado.Value;
        TipoReclamo.IsChecked = reclamo.Tipo != Tipos.Queja;
        TipoQueja.IsChecked = reclamo.Tipo == Tipos.Queja;
        DetalleBox.Text = reclamo.Detalle;
        PedidoBox.Text = reclamo.Pedido;
        Seleccionar(TipoAtencionCombo, reclamo.TipoAtencion);
        Seleccionar(ResultadoCombo, reclamo.Resultado);
        Seleccionar(CanalCombo, reclamo.Canal);
        FechaRespuestaPicker.SelectedDate = reclamo.FechaComunicacion is null ? null : AFecha(reclamo.FechaComunicacion.Value);
        ObservacionesBox.Text = reclamo.Observaciones;
        FundamentoBox.Text = reclamo.Fundamento;
        var marcaConsumidor = ++_cargaConsumidor;
        var marcaProveedor = ++_cargaProveedor;
        _imagenesConsumidor.Clear();
        _imagenesProveedor.Clear();
        ActualizarVistaImagenes();
        _ = CargarImagenesAsync(_imagenesConsumidor, reclamo.ImagenesConsumidor, () => _cargaConsumidor, marcaConsumidor);
        _ = CargarImagenesAsync(_imagenesProveedor, reclamo.ImagenesRespuesta, () => _cargaProveedor, marcaProveedor);
        PadrePanel.Visibility = MenorSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed;
        ActualizarDerivados();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        if (Guardar(exigirRespuesta: false))
            MostrarAviso(InfoBarSeverity.Success, "Borrador guardado", "Quedó en el historial. Puede descargar el PDF cuando la respuesta esté completa.");
    }

    private async void Exportar_Click(object sender, RoutedEventArgs e)
    {
        if (!Guardar(exigirRespuesta: true))
            return;

        var reclamo = Almacen.Actual.Buscar(_id);
        if (reclamo is null)
            return;

        var ruta = await Archivos.ElegirPdfAsync(App.Ventana, PdfExporter.NombreArchivo(reclamo));
        if (ruta is null)
        {
            MostrarAviso(InfoBarSeverity.Informational, "PDF no guardado", "La respuesta sí quedó en el historial. Puede descargarla después.");
            return;
        }

        try
        {
            await Task.Run(() => PdfExporter.Generar(reclamo, ruta));
            Archivos.Abrir(ruta);
            MostrarAviso(InfoBarSeverity.Success, "PDF descargado", ruta);
        }
        catch (Exception ex)
        {
            MostrarAviso(InfoBarSeverity.Error, "No se pudo crear el PDF", ex.Message);
        }
    }

    private bool Guardar(bool exigirRespuesta)
    {
        var error = Validar(exigirRespuesta);
        if (error is not null)
        {
            MostrarAviso(InfoBarSeverity.Warning, "Revise el formulario", error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_membrete.RazonSocial))
            _membrete = Almacen.Actual.Datos.Proveedor.Clonar();

        var reclamo = Leer();
        _huboPropuesta = reclamo.HuboPropuesta;
        Almacen.Actual.GuardarReclamo(reclamo);
        ImagenesRespuesta.Conservar(_id, reclamo.ImagenesConsumidor.Concat(reclamo.ImagenesRespuesta));
        ActualizarDerivados();
        return true;
    }

    private string? Validar(bool exigirRespuesta)
    {
        if (double.IsNaN(NumeroBox.Value) || NumeroBox.Value < 1)
            return "Indique el número de la hoja de reclamación.";
        if (double.IsNaN(AnioBox.Value))
            return "Indique el año de la hoja.";
        if (string.IsNullOrWhiteSpace(NombreBox.Text))
            return "Indique el nombre del consumidor.";
        if (LeerFecha(FechaReclamoPicker) is null)
            return "Indique la fecha del reclamo o queja.";

        var numero = (int)NumeroBox.Value;
        var anio = (int)AnioBox.Value;
        if (Almacen.Actual.ExisteNumero(_id, numero, anio))
            return $"Ya existe la hoja {Formato.NumeroHoja(numero, anio)}.";

        if (!exigirRespuesta)
            return null;

        AsegurarMembrete();
        if (!_membrete.EstaCompleto && !Almacen.Actual.Datos.Proveedor.EstaCompleto)
            return "Complete la razón social, el RUC y el domicilio del proveedor.";
        if (string.IsNullOrWhiteSpace(DetalleBox.Text))
            return "Falta el detalle del reclamo o queja.";
        if (string.IsNullOrWhiteSpace(PedidoBox.Text))
            return "Falta el pedido concreto del consumidor.";

        var tipoAtencion = TipoAtencionCombo.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(tipoAtencion))
            return "Indique si la comunicación es una respuesta definitiva, una propuesta de solución o un acuerdo.";
        if (string.IsNullOrWhiteSpace(ObservacionesBox.Text))
            return "Faltan las observaciones y acciones adoptadas por el proveedor.";
        if (LeerFecha(FechaRespuestaPicker) is null)
            return "Indique la fecha de comunicación de la respuesta.";
        if (CanalCombo.SelectedItem is null)
            return "Indique el canal por el que se comunica la respuesta.";

        var resultado = ResultadoCombo.SelectedItem?.ToString() ?? "";
        if (tipoAtencion == Tipos.Definitiva && string.IsNullOrWhiteSpace(resultado))
            return "Indique el resultado de la respuesta definitiva.";
        if ((resultado == Tipos.NoAcoge || resultado == Tipos.Parcial) && string.IsNullOrWhiteSpace(FundamentoBox.Text))
            return "Si no se acoge el pedido, o se acoge solo en parte, debe fundamentar la posición.";

        return null;
    }

    private Reclamo Leer()
    {
        AsegurarMembrete();
        var tipoAtencion = TipoAtencionCombo.SelectedItem?.ToString() ?? "";
        var observaciones = ObservacionesBox.Text.Trim();
        var fechaRespuesta = LeerFecha(FechaRespuestaPicker);
        var estado = Tipos.Borrador;
        if (fechaRespuesta is not null && observaciones.Length > 0)
        {
            estado = tipoAtencion switch
            {
                Tipos.Definitiva or Tipos.Acuerdo => Tipos.Respondido,
                Tipos.Propuesta => Tipos.PropuestaEnviada,
                _ => Tipos.Borrador
            };
        }

        var reclamo = new Reclamo
        {
            Id = _id,
            CreadoEn = _creadoEn,
            Numero = (int)NumeroBox.Value,
            Anio = (int)AnioBox.Value,
            FechaReclamo = LeerFecha(FechaReclamoPicker) ?? DateOnly.FromDateTime(DateTime.Today),
            NombreConsumidor = NombreBox.Text.Trim(),
            DomicilioConsumidor = DomicilioBox.Text.Trim(),
            TipoDocumento = TipoDocCombo.SelectedItem?.ToString() ?? "DNI",
            NumeroDocumento = DocumentoBox.Text.Trim(),
            Telefono = TelefonoBox.Text.Trim(),
            Email = EmailBox.Text.Trim(),
            EsMenor = MenorSwitch.IsOn,
            NombrePadre = PadreNombreBox.Text.Trim(),
            DocumentoPadre = PadreDocumentoBox.Text.Trim(),
            TelefonoPadre = PadreTelefonoBox.Text.Trim(),
            EmailPadre = PadreEmailBox.Text.Trim(),
            TipoBien = TipoServicio.IsChecked == true ? Tipos.Servicio : Tipos.Producto,
            DescripcionBien = DescripcionBienBox.Text.Trim(),
            MontoReclamado = double.IsNaN(MontoBox.Value) ? null : decimal.Round((decimal)MontoBox.Value, 2),
            Tipo = TipoQueja.IsChecked == true ? Tipos.Queja : Tipos.Reclamo,
            Detalle = DetalleBox.Text.Trim(),
            Pedido = PedidoBox.Text.Trim(),
            ImagenesConsumidor = _imagenesConsumidor.Select(imagen => imagen.Ruta).ToList(),
            TipoAtencion = tipoAtencion,
            Resultado = ResultadoCombo.SelectedItem?.ToString() ?? "",
            Canal = CanalCombo.SelectedItem?.ToString() ?? "",
            Observaciones = observaciones,
            Fundamento = FundamentoBox.Text.Trim(),
            ImagenesRespuesta = _imagenesProveedor.Select(imagen => imagen.Ruta).ToList(),
            FechaComunicacion = fechaRespuesta,
            HuboPropuesta = _huboPropuesta || tipoAtencion == Tipos.Propuesta,
            Estado = estado
        };
        reclamo.CopiarMembrete(_membrete);
        return reclamo;
    }

    private async void Redactar_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ObservacionesBox.Text))
        {
            var dialogo = new ContentDialog
            {
                Title = "Reemplazar el texto",
                Content = "Ya hay una respuesta escrita. ¿Desea reemplazarla por un borrador editable?",
                PrimaryButtonText = "Reemplazar",
                CloseButtonText = "Cancelar",
                XamlRoot = XamlRoot,
                DefaultButton = ContentDialogButton.Close
            };
            if (await dialogo.ShowAsync() != ContentDialogResult.Primary)
                return;
        }

        var reclamo = LeerSinValidar();
        ObservacionesBox.Text = BorradorRespuesta.Observaciones(reclamo);
        FundamentoBox.Text = BorradorRespuesta.Fundamento(reclamo);
    }

    private Reclamo LeerSinValidar()
    {
        if (double.IsNaN(NumeroBox.Value))
            NumeroBox.Value = 1;
        if (double.IsNaN(AnioBox.Value))
            AnioBox.Value = DateTime.Now.Year;
        return Leer();
    }

    private void AsegurarMembrete()
    {
        if (string.IsNullOrWhiteSpace(_membrete.RazonSocial))
            _membrete = Almacen.Actual.Datos.Proveedor.Clonar();
    }

    private void ActualizarMembrete_Click(object sender, RoutedEventArgs e)
    {
        _membrete = Almacen.Actual.Datos.Proveedor.Clonar();
        ActualizarDerivados();
        ActualizarAvisoProveedor();
        MostrarAviso(InfoBarSeverity.Informational, "Membrete actualizado", "El PDF usará los datos actuales del proveedor.");
    }

    private async void ImportarImagenes_Click(object sender, RoutedEventArgs e)
    {
        var delConsumidor = sender is FrameworkElement { Tag: "consumidor" };
        var lista = delConsumidor ? _imagenesConsumidor : _imagenesProveedor;
        var quien = delConsumidor ? "del consumidor" : "del proveedor";

        IReadOnlyList<Windows.Storage.StorageFile> archivos;
        try
        {
            archivos = await Archivos.ElegirImagenesAsync(App.Ventana);
        }
        catch (Exception ex)
        {
            MostrarAviso(InfoBarSeverity.Error, "No se pudieron elegir las imágenes", ex.Message);
            return;
        }

        if (archivos.Count == 0)
            return;

        var errores = new List<string>();
        var importadas = 0;
        foreach (var archivo in archivos)
        {
            try
            {
                var relativa = await ImagenesRespuesta.GuardarAsync(_id, archivo);
                var miniatura = await CargarMiniaturaAsync(relativa);
                if (miniatura is not null)
                {
                    lista.Add(miniatura);
                    importadas++;
                }
            }
            catch (Exception ex)
            {
                errores.Add($"{archivo.Name}: {ex.Message}");
            }
        }

        ActualizarVistaImagenes();
        if (errores.Count > 0)
            MostrarAviso(InfoBarSeverity.Warning, "Algunas imágenes no se importaron", string.Join(" ", errores));
        else
            MostrarAviso(InfoBarSeverity.Success, "Imágenes importadas", importadas == 1
                ? $"La imagen {quien} se incluirá en el PDF al guardar."
                : $"{importadas} imágenes {quien} se incluirán en el PDF al guardar.");
    }

    private void QuitarImagen_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string ruta })
            return;

        QuitarDe(_imagenesConsumidor, ruta);
        QuitarDe(_imagenesProveedor, ruta);
        ActualizarVistaImagenes();
    }

    private static void QuitarDe(ObservableCollection<MiniaturaImagen> lista, string ruta)
    {
        var miniatura = lista.FirstOrDefault(imagen => imagen.Ruta == ruta);
        if (miniatura is not null)
            lista.Remove(miniatura);
    }

    private void QuitarImagenes_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: "consumidor" })
        {
            _cargaConsumidor++;
            _imagenesConsumidor.Clear();
        }
        else
        {
            _cargaProveedor++;
            _imagenesProveedor.Clear();
        }

        ActualizarVistaImagenes();
    }

    private async Task CargarImagenesAsync(
        ObservableCollection<MiniaturaImagen> destino,
        IEnumerable<string>? relativas,
        Func<int> marcaActual,
        int marca)
    {
        destino.Clear();
        ActualizarVistaImagenes();

        foreach (var relativa in ImagenesRespuesta.Existentes(relativas))
        {
            var miniatura = await CargarMiniaturaAsync(relativa);
            if (marcaActual() != marca)
                return;
            if (miniatura is not null)
                destino.Add(miniatura);
        }

        if (marcaActual() == marca)
            ActualizarVistaImagenes();
    }

    private static IEnumerable<string> RutasGuardadas(Guid id)
    {
        var reclamo = Almacen.Actual.Buscar(id);
        if (reclamo is null)
            return [];

        return (reclamo.ImagenesConsumidor ?? []).Concat(reclamo.ImagenesRespuesta ?? []);
    }

    private static async Task<MiniaturaImagen?> CargarMiniaturaAsync(string relativa)
    {
        var absoluta = ImagenesRespuesta.Absoluta(relativa);
        if (!File.Exists(absoluta))
            return null;

        var bitmap = new BitmapImage();
        using var flujo = File.OpenRead(absoluta);
        using var lectura = flujo.AsRandomAccessStream();
        await bitmap.SetSourceAsync(lectura);
        return new MiniaturaImagen { Ruta = relativa, Vista = bitmap };
    }

    private void ActualizarVistaImagenes()
    {
        AplicarVista(_imagenesConsumidor, ListaImagenesConsumidor, SinImagenesConsumidor, QuitarImagenesConsumidor);
        AplicarVista(_imagenesProveedor, ListaImagenes, SinImagenes, QuitarImagenesBoton);
    }

    private static void AplicarVista(
        ObservableCollection<MiniaturaImagen> lista,
        ItemsControl galeria,
        TextBlock vacio,
        Button quitarTodas)
    {
        var hay = lista.Count > 0;
        galeria.Visibility = hay ? Visibility.Visible : Visibility.Collapsed;
        vacio.Visibility = hay ? Visibility.Collapsed : Visibility.Visible;
        quitarTodas.Visibility = hay ? Visibility.Visible : Visibility.Collapsed;
    }

    private void MenorSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        PadrePanel.Visibility = MenorSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Derivados_Changed(NumberBox sender, NumberBoxValueChangedEventArgs args) => ActualizarDerivados();

    private void Fecha_Changed(object sender, DatePickerValueChangedEventArgs args) => ActualizarDerivados();

    private void Resultado_Changed(object sender, SelectionChangedEventArgs e) => ActualizarDerivados();

    private void TipoAtencion_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (TipoAtencionCombo.SelectedItem?.ToString() == Tipos.Acuerdo)
            Seleccionar(ResultadoCombo, Tipos.AcuerdoResultado);
        ActualizarDerivados();
    }

    private void ActualizarDerivados()
    {
        if (double.IsNaN(NumeroBox.Value) || double.IsNaN(AnioBox.Value))
        {
            NumeroVista.Text = "";
        }
        else
        {
            NumeroVista.Text = "Hoja N.º " + Formato.NumeroHoja((int)NumeroBox.Value, (int)AnioBox.Value);
        }

        MembreteVista.Text = string.IsNullOrWhiteSpace(_membrete.RazonSocial)
            ? "Aún no hay datos del proveedor para imprimir en el PDF."
            : $"Se emitirá a nombre de {_membrete.RazonSocial.Trim()} — RUC {_membrete.Ruc.Trim()}";

        var fecha = LeerFecha(FechaReclamoPicker);
        if (fecha is null)
        {
            PlazoInfo.Severity = InfoBarSeverity.Informational;
            PlazoInfo.Message = "Indique la fecha del reclamo para calcular el plazo de 15 días hábiles.";
            return;
        }

        var vista = new Reclamo
        {
            FechaReclamo = fecha.Value,
            FechaComunicacion = LeerFecha(FechaRespuestaPicker),
            TipoAtencion = TipoAtencionCombo.SelectedItem?.ToString() ?? "",
            Observaciones = ObservacionesBox.Text,
            HuboPropuesta = _huboPropuesta || TipoAtencionCombo.SelectedItem?.ToString() == Tipos.Propuesta,
            Estado = ""
        };
        if (!string.IsNullOrWhiteSpace(vista.Observaciones) && vista.FechaComunicacion is not null)
        {
            vista.Estado = vista.TipoAtencion switch
            {
                Tipos.Definitiva or Tipos.Acuerdo => Tipos.Respondido,
                Tipos.Propuesta => Tipos.PropuestaEnviada,
                _ => Tipos.Borrador
            };
        }

        var estado = PlazoService.Evaluar(vista, DateOnly.FromDateTime(DateTime.Today));
        PlazoInfo.Message = estado.Texto + " El cálculo no incluye feriados regionales ni puentes dispuestos por norma del año.";
        PlazoInfo.Severity = estado.Clave switch
        {
            "respondido" => InfoBarSeverity.Success,
            "vencido" => InfoBarSeverity.Error,
            "vence" or "pronto" => InfoBarSeverity.Warning,
            _ => InfoBarSeverity.Informational
        };
    }

    private void ActualizarAvisoProveedor()
    {
        AvisoProveedor.IsOpen = !Almacen.Actual.Datos.Proveedor.EstaCompleto && !_membrete.EstaCompleto;
    }

    private void MostrarAviso(InfoBarSeverity severidad, string titulo, string mensaje)
    {
        Aviso.Severity = severidad;
        Aviso.Title = titulo;
        Aviso.Message = mensaje;
        Aviso.IsOpen = true;
    }

    private static DateOnly? LeerFecha(DatePicker selector)
    {
        if (selector.SelectedDate is not DateTimeOffset fecha)
            return null;
        return DateOnly.FromDateTime(fecha.Date);
    }

    private static DateTimeOffset AFecha(DateOnly fecha) =>
        new(fecha.ToDateTime(TimeOnly.MinValue));

    private static void Seleccionar(ComboBox lista, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            lista.SelectedIndex = -1;
            return;
        }

        for (var i = 0; i < lista.Items.Count; i++)
        {
            if (lista.Items[i]?.ToString() == valor)
            {
                lista.SelectedIndex = i;
                return;
            }
        }

        lista.SelectedIndex = -1;
    }
}

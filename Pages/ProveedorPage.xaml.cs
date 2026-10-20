using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using LibroReclamaciones.Services;

namespace LibroReclamaciones.Pages;

public sealed partial class ProveedorPage : Page
{
    public ProveedorPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var proveedor = Almacen.Actual.Datos.Proveedor;
        RazonBox.Text = proveedor.RazonSocial;
        RucBox.Text = proveedor.Ruc;
        CodigoBox.Text = proveedor.CodigoIdentificacion;
        DomicilioBox.Text = proveedor.Domicilio;
        CiudadBox.Text = proveedor.Ciudad;
        RepresentanteBox.Text = proveedor.Representante;
        CargoBox.Text = proveedor.Cargo;
        CorreoBox.Text = proveedor.Correo;
        TelefonoBox.Text = proveedor.Telefono;
        CorrelativoBox.Value = Math.Max(1, proveedor.SiguienteNumero);
        RutaDatos.Text = "Los reclamos se guardan en " + Almacen.Actual.RutaArchivo;
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        var error = Guardar();
        if (error is not null)
        {
            Aviso.Severity = InfoBarSeverity.Warning;
            Aviso.Title = "Faltan datos";
            Aviso.Message = error;
            Aviso.IsOpen = true;
            return;
        }

        var ruc = RucBox.Text.Trim();
        Aviso.Severity = ruc.Length == 11 ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        Aviso.Title = "Datos guardados";
        Aviso.Message = ruc.Length == 11
            ? "Se usarán en las próximas hojas y en sus PDF."
            : "Se guardaron. El RUC de una persona jurídica suele tener 11 dígitos.";
        Aviso.IsOpen = true;
    }

    private string? Guardar()
    {
        if (string.IsNullOrWhiteSpace(RazonBox.Text) || string.IsNullOrWhiteSpace(RucBox.Text) || string.IsNullOrWhiteSpace(DomicilioBox.Text))
            return "La razón social, el RUC y el domicilio son necesarios para el formato oficial.";

        var proveedor = Almacen.Actual.Datos.Proveedor;
        proveedor.RazonSocial = RazonBox.Text.Trim();
        proveedor.Ruc = RucBox.Text.Trim();
        proveedor.CodigoIdentificacion = CodigoBox.Text.Trim();
        proveedor.Domicilio = DomicilioBox.Text.Trim();
        proveedor.Ciudad = CiudadBox.Text.Trim();
        proveedor.Representante = RepresentanteBox.Text.Trim();
        proveedor.Cargo = CargoBox.Text.Trim();
        proveedor.Correo = CorreoBox.Text.Trim();
        proveedor.Telefono = TelefonoBox.Text.Trim();
        proveedor.SiguienteNumero = double.IsNaN(CorrelativoBox.Value) ? 1 : Math.Max(1, (int)CorrelativoBox.Value);
        Almacen.Actual.Guardar();
        return null;
    }

    private void AbrirCarpeta_Click(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(Almacen.Actual.Carpeta);
        System.Diagnostics.Process.Start("explorer.exe", Almacen.Actual.Carpeta);
    }
}

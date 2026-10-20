using LibroReclamaciones.Services;
using Microsoft.UI.Xaml;

namespace LibroReclamaciones;

public partial class App : Application
{
    public static MainWindow Ventana { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Almacen.Actual.Cargar();
        Ventana = new MainWindow();
        Ventana.Activate();
    }
}

using System.Runtime.InteropServices;
using LibroReclamaciones.Pages;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace LibroReclamaciones;

public sealed partial class MainWindow : Window
{
    private object? _parametroFormulario;

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        Iderma.Shell.BotonLauncher.Colocar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 860));
        Activated += AlAbrir;
        NavView.SelectedItem = FormularioNavItem;
    }

    private void AlAbrir(object sender, WindowActivatedEventArgs args)
    {
        Activated -= AlAbrir;
        AplicarIcono();
        if (AppWindow.Presenter is OverlappedPresenter presenter)
            presenter.Maximize();
    }

    private void AplicarIcono()
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "Assets", "logo-reclamos.ico");
        if (!File.Exists(ruta))
            ruta = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (!File.Exists(ruta))
            return;

        AppWindow.SetIcon(ruta);

        var hwnd = WindowNative.GetWindowHandle(this);
        _iconoGrande = LoadImage(IntPtr.Zero, ruta, 1, 32, 32, 0x0010);
        _iconoChico = LoadImage(IntPtr.Zero, ruta, 1, 16, 16, 0x0010);
        if (_iconoGrande != IntPtr.Zero)
        {
            SendMessage(hwnd, 0x0080, (IntPtr)1, _iconoGrande);
            SetClassLongPtr(hwnd, -14, _iconoGrande);
        }

        if (_iconoChico != IntPtr.Zero)
        {
            SendMessage(hwnd, 0x0080, (IntPtr)0, _iconoChico);
            SetClassLongPtr(hwnd, -34, _iconoChico);
        }
    }

    private static IntPtr _iconoGrande;
    private static IntPtr _iconoChico;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadImage(IntPtr instancia, string ruta, uint tipo, int ancho, int alto, uint opciones);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hwnd, uint mensaje, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SetClassLongPtrW", SetLastError = true)]
    private static extern IntPtr SetClassLongPtr(IntPtr hwnd, int indice, IntPtr valor);

    public void MostrarFormulario(Guid id)
    {
        _parametroFormulario = id;
        if (ReferenceEquals(NavView.SelectedItem, FormularioNavItem))
            NavFrame.Navigate(typeof(FormularioPage), ConsumirParametro());
        else
            NavView.SelectedItem = FormularioNavItem;
    }

    public void MostrarHistorial()
    {
        NavView.SelectedItem = HistorialNavItem;
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item)
            return;

        switch (item.Tag)
        {
            case "formulario":
                NavFrame.Navigate(typeof(FormularioPage), ConsumirParametro());
                break;
            case "historial":
                NavFrame.Navigate(typeof(HistorialPage));
                break;
            case "proveedor":
                NavFrame.Navigate(typeof(ProveedorPage));
                break;
        }
    }

    private object? ConsumirParametro()
    {
        var parametro = _parametroFormulario;
        _parametroFormulario = null;
        return parametro;
    }
}

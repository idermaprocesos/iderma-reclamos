using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace LibroReclamaciones.Services;

public static class Archivos
{
    public static async Task<string?> ElegirPdfAsync(Window ventana, string nombreSugerido)
    {
        var selector = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = nombreSugerido
        };
        selector.FileTypeChoices.Add("Documento PDF", [".pdf"]);

        var hwnd = WindowNative.GetWindowHandle(ventana);
        InitializeWithWindow.Initialize(selector, hwnd);
        var archivo = await selector.PickSaveFileAsync();
        return archivo?.Path;
    }

    public static async Task<IReadOnlyList<StorageFile>> ElegirImagenesAsync(Window ventana)
    {
        var selector = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            ViewMode = PickerViewMode.Thumbnail
        };
        selector.FileTypeFilter.Add(".jpg");
        selector.FileTypeFilter.Add(".jpeg");
        selector.FileTypeFilter.Add(".png");
        selector.FileTypeFilter.Add(".bmp");
        selector.FileTypeFilter.Add(".webp");

        var hwnd = WindowNative.GetWindowHandle(ventana);
        InitializeWithWindow.Initialize(selector, hwnd);
        var archivos = await selector.PickMultipleFilesAsync();
        return archivos is null ? [] : archivos.ToList();
    }

    public static void Abrir(string ruta)
    {
        var inicio = new System.Diagnostics.ProcessStartInfo(ruta) { UseShellExecute = true };
        System.Diagnostics.Process.Start(inicio);
    }
}

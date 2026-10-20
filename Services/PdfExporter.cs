using LibroReclamaciones.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LibroReclamaciones.Services;

public static class PdfExporter
{
    private const string Navy = "#0E3A5D";
    private const string Oro = "#C6A35A";
    private const string Linea = "#D0D7E2";
    private const string TintaSuave = "#475467";
    private const string Fondo = "#F4F7FB";

    static PdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static string NombreArchivo(Reclamo reclamo)
    {
        var consumidor = Formato.ArchivoSeguro(reclamo.NombreConsumidor);
        if (consumidor.Length > 40)
            consumidor = consumidor[..40].Trim();
        return $"Respuesta-HR-{reclamo.NumeroTexto}-{consumidor}.pdf";
    }

    public static void Generar(Reclamo reclamo, string ruta)
    {
        Document.Create(contenedor =>
        {
            contenedor.Page(pagina => ConfigurarHoja(pagina, reclamo));
            contenedor.Page(pagina => ConfigurarCarta(pagina, reclamo));
        }).GeneratePdf(ruta);
    }

    private static void ConfigurarHoja(PageDescriptor pagina, Reclamo reclamo)
    {
        pagina.Size(PageSizes.A4);
        pagina.Margin(1.05f, Unit.Centimetre);
        pagina.DefaultTextStyle(estilo => estilo.FontSize(9).FontColor("#1D2939"));
        pagina.Content().Column(columna =>
        {
            columna.Spacing(4);
            columna.Item().Element(c => EncabezadoHoja(c, reclamo));
            columna.Item().Element(c => SeccionConsumidor(c, reclamo));
            columna.Item().Element(c => SeccionBien(c, reclamo));
            columna.Item().Element(c => SeccionDetalle(c, reclamo));
            columna.Item().Element(c => SeccionRespuesta(c, reclamo));
            columna.Item().Element(Notas);
            AgregarImagenes(columna, reclamo.ImagenesConsumidor, "IMÁGENES ADJUNTAS POR EL CONSUMIDOR", 168);
            AgregarImagenes(columna, reclamo.ImagenesRespuesta, "IMÁGENES ADJUNTAS POR EL PROVEEDOR", 168);
        });
        pagina.Footer().Element(c => Pie(c, reclamo));
    }

    private static void ConfigurarCarta(PageDescriptor pagina, Reclamo reclamo)
    {
        pagina.Size(PageSizes.A4);
        pagina.Margin(1.6f, Unit.Centimetre);
        pagina.DefaultTextStyle(estilo => estilo.FontSize(10.5f).FontColor("#1D2939").LineHeight(1.25f));
        pagina.Content().Column(columna =>
        {
            columna.Spacing(7);
            columna.Item().Element(c => Membrete(c, reclamo));
            columna.Item().PaddingTop(8).Text(LugarYFecha(reclamo)).FontSize(11);
            columna.Item().PaddingTop(6).Column(bloque =>
            {
                bloque.Item().Text("Señor(a)").FontSize(11);
                bloque.Item().Text(Valor(reclamo.NombreConsumidor)).Bold();
                if (!string.IsNullOrWhiteSpace(reclamo.NumeroDocumento))
                    bloque.Item().Text($"{reclamo.TipoDocumento}: {reclamo.NumeroDocumento}").FontColor(TintaSuave).FontSize(10);
                if (!string.IsNullOrWhiteSpace(reclamo.DomicilioConsumidor))
                    bloque.Item().Text(reclamo.DomicilioConsumidor).FontSize(10);
                if (!string.IsNullOrWhiteSpace(reclamo.Email))
                    bloque.Item().Text(reclamo.Email).FontSize(10);
            });
            columna.Item().PaddingTop(4).Text(texto =>
            {
                texto.Span("Ref.: ").Bold();
                texto.Span($"Respuesta a la hoja de reclamación N.º {reclamo.NumeroTexto}");
            });
            columna.Item().Text("Libro de Reclamaciones — D.S. N.º 011-2011-PCM, modificado por D.S. N.º 101-2022-PCM")
                .FontSize(9).FontColor(TintaSuave);
            columna.Item().PaddingTop(8).Text("De nuestra consideración:").FontSize(11);
            foreach (var parrafo in ParrafosCarta(reclamo))
                columna.Item().Text(parrafo).FontSize(11).Justify();
            AgregarImagenes(columna, reclamo.ImagenesConsumidor, "IMÁGENES PRESENTADAS POR EL CONSUMIDOR", 220);
            AgregarImagenes(columna, reclamo.ImagenesRespuesta, "IMÁGENES ADJUNTAS POR EL PROVEEDOR", 220);
            columna.Item().PaddingTop(4).Text("Atentamente,").FontSize(11);
            columna.Item().PaddingTop(8).Column(firma =>
            {
                firma.Item().Height(46);
                firma.Item().Width(240).BorderTop(0.6f).BorderColor(Navy);
                firma.Item().PaddingTop(4).Text(Valor(reclamo.ProveedorRepresentante, "Representante del proveedor")).Bold();
                if (!string.IsNullOrWhiteSpace(reclamo.ProveedorCargo))
                    firma.Item().Text(reclamo.ProveedorCargo).FontSize(10);
                firma.Item().Text(Valor(reclamo.ProveedorRazonSocial)).FontSize(10);
                firma.Item().Text($"RUC {Valor(reclamo.ProveedorRuc)}").FontSize(10);
            });
        });
        pagina.Footer().Element(c => Pie(c, reclamo));
    }

    private static void EncabezadoHoja(IContainer contenedor, Reclamo reclamo)
    {
        contenedor.Column(columna =>
        {
            columna.Item().Element(c => DibujarLogo(c, 46));
            columna.Item().PaddingTop(2).PaddingBottom(3).LineHorizontal(1.5f).LineColor(Oro);
            columna.Item().Row(fila =>
            {
                fila.RelativeItem().Column(texto =>
                {
                    texto.Item().Text("LIBRO DE RECLAMACIONES").FontSize(16).Bold().FontColor(Navy);
                    texto.Item().Text("Hoja de reclamación — copia para archivo del proveedor")
                        .FontSize(8).FontColor(TintaSuave);
                });
                fila.ConstantItem(190).AlignRight().Column(caja =>
                {
                    caja.Item().Background(Navy).Padding(6).AlignCenter()
                        .Text("HOJA DE RECLAMACIÓN").FontColor(Colors.White).FontSize(8).Bold();
                    caja.Item().Border(0.8f).BorderColor(Navy).Padding(6).AlignCenter()
                        .Text($"N.º {reclamo.NumeroTexto}").FontSize(12).Bold().FontColor(Navy);
                });
            });

            columna.Item().PaddingTop(6).Background(Fondo).Border(0.6f).BorderColor(Linea).Padding(8).Column(datos =>
            {
                datos.Item().Text(Valor(reclamo.ProveedorRazonSocial, "Proveedor no indicado")).Bold().FontSize(11);
                datos.Item().Text($"RUC: {Valor(reclamo.ProveedorRuc)}").FontSize(9);
                datos.Item().Text(Valor(reclamo.ProveedorDomicilio, "Domicilio del establecimiento no indicado")).FontSize(9);
                if (!string.IsNullOrWhiteSpace(reclamo.ProveedorCodigo))
                    datos.Item().Text($"Código de identificación: {reclamo.ProveedorCodigo}").FontSize(9);
            });

            columna.Item().PaddingTop(4).Row(fila =>
            {
                fila.AutoItem().AlignMiddle().Text("FECHA DEL RECLAMO").Bold().FontSize(8).FontColor(Navy);
                fila.ConstantItem(10);
                CajaFecha(fila, "DÍA", reclamo.FechaReclamo.Day.ToString("00"));
                fila.ConstantItem(6);
                CajaFecha(fila, "MES", reclamo.FechaReclamo.Month.ToString("00"));
                fila.ConstantItem(6);
                CajaFecha(fila, "AÑO", reclamo.FechaReclamo.Year.ToString());
                fila.RelativeItem().AlignMiddle().AlignRight()
                    .Text(Formato.FechaLarga(reclamo.FechaReclamo)).FontSize(8).FontColor(TintaSuave);
            });
        });
    }

    private static void CajaFecha(RowDescriptor fila, string etiqueta, string valor)
    {
        fila.ConstantItem(58).Border(0.6f).BorderColor(Linea).Column(caja =>
        {
            caja.Item().Background(Fondo).AlignCenter().Text(etiqueta).FontSize(6).FontColor(TintaSuave);
            caja.Item().PaddingVertical(3).AlignCenter().Text(valor).FontSize(10).Bold();
        });
    }

    private static void SeccionConsumidor(IContainer contenedor, Reclamo reclamo)
    {
        Seccion(contenedor, "1. IDENTIFICACIÓN DEL CONSUMIDOR RECLAMANTE", cuerpo =>
        {
            cuerpo.Column(columna =>
            {
                columna.Spacing(5);
                columna.Item().Element(c => Campo(c, "NOMBRE", reclamo.NombreConsumidor));
                columna.Item().Element(c => Campo(c, "DOMICILIO", reclamo.DomicilioConsumidor));
                columna.Item().Row(fila =>
                {
                    fila.RelativeItem().Element(c => Campo(c, "DNI / CE", $"{reclamo.TipoDocumento} {reclamo.NumeroDocumento}".Trim()));
                    fila.ConstantItem(8);
                    fila.RelativeItem().Element(c => Campo(c, "TELÉFONO", reclamo.Telefono));
                    fila.ConstantItem(8);
                    fila.RelativeItem(1.4f).Element(c => Campo(c, "CORREO ELECTRÓNICO", reclamo.Email));
                });
                if (reclamo.EsMenor || !string.IsNullOrWhiteSpace(reclamo.NombrePadre))
                {
                    columna.Item().Text("PADRE, MADRE O REPRESENTANTE (menor de edad)").FontSize(7).Bold().FontColor(Navy);
                    columna.Item().Row(fila =>
                    {
                        fila.RelativeItem(1.4f).Element(c => Campo(c, "NOMBRE", reclamo.NombrePadre));
                        fila.ConstantItem(8);
                        fila.RelativeItem().Element(c => Campo(c, "DOCUMENTO", reclamo.DocumentoPadre));
                        fila.ConstantItem(8);
                        fila.RelativeItem().Element(c => Campo(c, "TELÉFONO", reclamo.TelefonoPadre));
                        fila.ConstantItem(8);
                        fila.RelativeItem(1.2f).Element(c => Campo(c, "CORREO", reclamo.EmailPadre));
                    });
                }
            });
        });
    }

    private static void SeccionBien(IContainer contenedor, Reclamo reclamo)
    {
        Seccion(contenedor, "2. IDENTIFICACIÓN DEL BIEN CONTRATADO", cuerpo =>
        {
            cuerpo.Column(columna =>
            {
                columna.Spacing(6);
                columna.Item().Row(fila =>
                {
                    fila.RelativeItem().AlignMiddle().Row(marcas =>
                    {
                        marcas.AutoItem().Element(c => Marca(c, "PRODUCTO", reclamo.TipoBien == Tipos.Producto));
                        marcas.ConstantItem(16);
                        marcas.AutoItem().Element(c => Marca(c, "SERVICIO", reclamo.TipoBien == Tipos.Servicio));
                    });
                    fila.ConstantItem(12);
                    fila.RelativeItem().Element(c => Campo(c, "MONTO RECLAMADO", Formato.Monto(reclamo.MontoReclamado)));
                });
                columna.Item().Element(c => Campo(c, "DESCRIPCIÓN", reclamo.DescripcionBien));
            });
        });
    }

    private static void SeccionDetalle(IContainer contenedor, Reclamo reclamo)
    {
        Seccion(contenedor, "3. DETALLE DE LA RECLAMACIÓN Y PEDIDO DEL CONSUMIDOR", cuerpo =>
        {
            cuerpo.Column(columna =>
            {
                columna.Spacing(6);
                columna.Item().Row(fila =>
                {
                    fila.AutoItem().Element(c => Marca(c, "RECLAMO  ¹", reclamo.Tipo == Tipos.Reclamo));
                    fila.ConstantItem(18);
                    fila.AutoItem().Element(c => Marca(c, "QUEJA  ²", reclamo.Tipo == Tipos.Queja));
                });
                columna.Item().Element(c => CampoLargo(c, "DETALLE", reclamo.Detalle, 22));
                columna.Item().Element(c => CampoLargo(c, "PEDIDO CONCRETO", reclamo.Pedido, 14));
                var imagenesConsumidor = ImagenesRespuesta.Existentes(reclamo.ImagenesConsumidor).Count;
                if (imagenesConsumidor > 0)
                {
                    var aviso = imagenesConsumidor == 1
                        ? "Se adjunta 1 imagen del consumidor."
                        : $"Se adjuntan {imagenesConsumidor} imágenes del consumidor.";
                    columna.Item().Text(aviso).FontSize(8).FontColor(TintaSuave);
                }
                BloqueFirma(columna, "FIRMA DEL CONSUMIDOR");
            });
        });
    }

    private static void SeccionRespuesta(IContainer contenedor, Reclamo reclamo)
    {
        var pendiente = string.IsNullOrWhiteSpace(reclamo.Observaciones);
        var texto = pendiente
            ? "Pendiente de respuesta del proveedor."
            : reclamo.Observaciones.Trim();
        if (!pendiente && !string.IsNullOrWhiteSpace(reclamo.Fundamento))
            texto += "\n\nFundamento: " + reclamo.Fundamento.Trim();
        if (!pendiente && !string.IsNullOrWhiteSpace(reclamo.Resultado))
            texto = $"Resultado: {reclamo.Resultado}\nTipo de comunicación: {Valor(reclamo.TipoAtencion)}\nCanal: {Valor(reclamo.Canal)}\n\n{texto}";

        Seccion(contenedor, "4. OBSERVACIONES Y ACCIONES ADOPTADAS POR EL PROVEEDOR", cuerpo =>
        {
            cuerpo.Column(columna =>
            {
                columna.Spacing(6);
                columna.Item().Element(c => CampoLargo(c, "ACCIONES ADOPTADAS", texto, 28));
                var cantidadImagenes = ImagenesRespuesta.Existentes(reclamo.ImagenesRespuesta).Count;
                if (cantidadImagenes > 0)
                {
                    var aviso = cantidadImagenes == 1
                        ? "Se adjunta 1 imagen del proveedor."
                        : $"Se adjuntan {cantidadImagenes} imágenes del proveedor.";
                    columna.Item().Text(aviso).FontSize(8).FontColor(TintaSuave);
                }
                columna.Item().PaddingTop(8).Row(fila =>
                {
                    fila.RelativeItem().AlignMiddle().Text("FECHA DE COMUNICACIÓN DE LA RESPUESTA").Bold().FontSize(8).FontColor(Navy);
                    fila.ConstantItem(8);
                    CajaFecha(fila, "DÍA", reclamo.FechaComunicacion?.Day.ToString("00") ?? "");
                    fila.ConstantItem(6);
                    CajaFecha(fila, "MES", reclamo.FechaComunicacion?.Month.ToString("00") ?? "");
                    fila.ConstantItem(6);
                    CajaFecha(fila, "AÑO", reclamo.FechaComunicacion?.Year.ToString() ?? "");
                });
                BloqueFirma(columna, "FIRMA DEL PROVEEDOR");
            });
        });
    }

    private static void Notas(IContainer contenedor)
    {
        contenedor.Column(columna =>
        {
            columna.Spacing(2);
            columna.Item().Text("¹ RECLAMO: Disconformidad relacionada a los productos o servicios.").FontSize(7).FontColor(TintaSuave);
            columna.Item().Text("² QUEJA: Disconformidad no relacionada a los productos o servicios; o, malestar o descontento respecto a la atención al público.").FontSize(7).FontColor(TintaSuave);
            columna.Item().PaddingTop(2).Text("Destinatario de esta copia: PROVEEDOR").FontSize(7).Bold().FontColor(Navy);
            columna.Item().Text("* La formulación del reclamo no impide acudir a otras vías de solución de controversias ni es requisito previo para interponer una denuncia ante el INDECOPI.").FontSize(7).FontColor(TintaSuave);
            columna.Item().Text("* El proveedor debe dar respuesta al reclamo o queja en un plazo no mayor a quince (15) días hábiles, el cual es improrrogable.").FontSize(7).FontColor(TintaSuave);
            columna.Item().Text("Conserve la hoja de reclamación y esta respuesta por dos (2) años.").FontSize(7).FontColor(TintaSuave);
        });
    }

    private static void Membrete(IContainer contenedor, Reclamo reclamo)
    {
        contenedor.BorderBottom(1.5f).BorderColor(Navy).PaddingBottom(8).Column(columna =>
        {
            columna.Item().Element(c => DibujarLogo(c, 62));
            columna.Item().PaddingTop(3).PaddingBottom(4).LineHorizontal(1.5f).LineColor(Oro);
            columna.Item().PaddingTop(4).Text(Valor(reclamo.ProveedorRazonSocial, "Proveedor")).FontSize(14).Bold().FontColor(Navy);
            columna.Item().Text($"RUC {Valor(reclamo.ProveedorRuc)}").FontSize(9).FontColor(TintaSuave);
            columna.Item().Text(Valor(reclamo.ProveedorDomicilio)).FontSize(9).FontColor(TintaSuave);
            var contacto = string.Join("   ·   ", new[] { reclamo.ProveedorTelefono, reclamo.ProveedorCorreo }.Where(t => !string.IsNullOrWhiteSpace(t)));
            if (!string.IsNullOrWhiteSpace(contacto))
                columna.Item().Text(contacto).FontSize(9).FontColor(TintaSuave);
            columna.Item().PaddingTop(4).Text("COMUNICACIÓN DE RESPUESTA AL CONSUMIDOR").FontSize(8).Bold().FontColor(Navy);
        });
    }

    private static IEnumerable<string> ParrafosCarta(Reclamo reclamo)
    {
        var tipo = reclamo.Tipo.Equals(Tipos.Queja, StringComparison.OrdinalIgnoreCase) ? "queja" : "reclamo";
        var bien = reclamo.TipoBien.Equals(Tipos.Servicio, StringComparison.OrdinalIgnoreCase) ? "servicio" : "producto";
        var descripcion = string.IsNullOrWhiteSpace(reclamo.DescripcionBien) ? "no detallado" : reclamo.DescripcionBien.Trim();
        var pedido = string.IsNullOrWhiteSpace(reclamo.Pedido) ? "no precisado en la hoja" : reclamo.Pedido.Trim();

        yield return $"En atención a la hoja de reclamación N.º {reclamo.NumeroTexto}, presentada el {Formato.FechaLarga(reclamo.FechaReclamo)}, mediante la cual formula un {tipo} relacionado con el {bien} «{descripcion}», cumplimos con comunicar lo siguiente.";
        yield return $"Pedido del consumidor: {pedido}";

        if (reclamo.TipoAtencion == Tipos.Propuesta)
            yield return "La presente es una propuesta de solución y todavía no constituye la respuesta formal definitiva.";
        else if (!string.IsNullOrWhiteSpace(reclamo.Resultado))
            yield return $"Resultado: {reclamo.Resultado}.";

        if (!string.IsNullOrWhiteSpace(reclamo.Observaciones))
            yield return "Acciones adoptadas:\n" + reclamo.Observaciones.Trim();
        else
            yield return "A la fecha de generación de este documento, el proveedor aún no registra las acciones adoptadas.";

        if (!string.IsNullOrWhiteSpace(reclamo.Fundamento))
            yield return "Fundamento:\n" + reclamo.Fundamento.Trim();

        yield return Cierre(reclamo);

        var via = reclamo.Canal switch
        {
            Tipos.Correo => "correo electrónico",
            Tipos.Carta => "carta",
            Tipos.Presencial => "entrega presencial",
            _ => "el medio indicado por el consumidor"
        };
        yield return $"Esta comunicación se remite por {via}, de acuerdo con el canal señalado para la respuesta.";

        var delConsumidor = ImagenesRespuesta.Existentes(reclamo.ImagenesConsumidor).Count;
        if (delConsumidor == 1)
            yield return "El consumidor presentó una imagen junto con su reclamo.";
        else if (delConsumidor > 1)
            yield return $"El consumidor presentó {delConsumidor} imágenes junto con su reclamo.";

        var delProveedor = ImagenesRespuesta.Existentes(reclamo.ImagenesRespuesta).Count;
        if (delProveedor == 1)
            yield return "Se adjunta a esta comunicación una imagen del proveedor.";
        else if (delProveedor > 1)
            yield return $"Se adjuntan a esta comunicación {delProveedor} imágenes del proveedor.";
    }

    private static void AgregarImagenes(ColumnDescriptor columna, IEnumerable<string>? relativas, string titulo, float alto)
    {
        var rutas = ImagenesRespuesta.Existentes(relativas);
        if (rutas.Count == 0)
            return;

        columna.Item().PaddingTop(8).Text(titulo).FontSize(8).Bold().FontColor(Navy);
        for (var i = 0; i < rutas.Count; i += 2)
        {
            var izquierda = rutas[i];
            var numeroIzquierda = i + 1;
            var derecha = i + 1 < rutas.Count ? rutas[i + 1] : null;
            var numeroDerecha = i + 2;
            columna.Item().PaddingTop(4).Row(fila =>
            {
                fila.Spacing(10);
                fila.RelativeItem().Element(c => PintarImagen(c, izquierda, numeroIzquierda, alto));
                if (derecha is null)
                    fila.RelativeItem();
                else
                    fila.RelativeItem().Element(c => PintarImagen(c, derecha, numeroDerecha, alto));
            });
        }
    }

    private static void PintarImagen(IContainer contenedor, string ruta, int numero, float alto)
    {
        contenedor.Border(0.6f).BorderColor(Linea).Padding(4).Column(columna =>
        {
            columna.Item().Height(alto).AlignCenter().Image(ImagenesRespuesta.Absoluta(ruta)).FitArea();
            columna.Item().PaddingTop(2).AlignCenter().Text($"Imagen {numero}").FontSize(7).FontColor(TintaSuave);
        });
    }

    private static string Cierre(Reclamo reclamo)
    {
        if (reclamo.TipoAtencion == Tipos.Propuesta)
            return "De aceptar el ofrecimiento, sírvase responder de forma expresa con la frase «ACUERDO ACEPTADO PARA SOLUCIONAR EL RECLAMO», u otra equivalente, por un medio que deje constancia. El envío de esta propuesta suspende el cómputo del plazo de respuesta por un máximo de cinco (5) días hábiles. Si no hay aceptación expresa dentro de ese plazo, la propuesta se tiene por rechazada y subsiste la obligación de emitir una respuesta formal.";

        if (reclamo.TipoAtencion == Tipos.Acuerdo)
            return "Se deja constancia de la solución acordada con la frase «ACUERDO ACEPTADO PARA SOLUCIONAR EL RECLAMO». La formulación del reclamo no impide acudir a otras vías de solución de controversias ni es requisito previo para interponer una denuncia ante el Indecopi.";

        return "Esta comunicación constituye la respuesta escrita del proveedor y se emite dentro del plazo máximo de quince (15) días hábiles previsto en el Código de Protección y Defensa del Consumidor y en el Reglamento del Libro de Reclamaciones, aprobado por Decreto Supremo N.º 011-2011-PCM y modificado por el Decreto Supremo N.º 101-2022-PCM. Dicho plazo es improrrogable. La formulación del reclamo no impide acudir a otras vías de solución de controversias ni es requisito previo para interponer una denuncia ante el Indecopi.";
    }

    private static string LugarYFecha(Reclamo reclamo)
    {
        var fecha = reclamo.FechaComunicacion ?? DateOnly.FromDateTime(DateTime.Today);
        var larga = Formato.FechaLarga(fecha);
        return string.IsNullOrWhiteSpace(reclamo.ProveedorCiudad) ? larga : $"{reclamo.ProveedorCiudad}, {larga}";
    }

    private static void DibujarLogo(IContainer contenedor, float alto)
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "Assets", "IdermaLogo.png");
        if (!File.Exists(ruta))
        {
            contenedor.Height(1);
            return;
        }

        var ancho = alto * (1184f / 868f);
        contenedor.AlignCenter().Width(ancho).Height(alto).Image(ruta).FitArea();
    }

    private static void Seccion(IContainer contenedor, string titulo, Action<IContainer> contenido)
    {
        contenedor.Border(0.7f).BorderColor(Linea).Column(columna =>
        {
            columna.Item().Background(Navy).PaddingHorizontal(6).PaddingVertical(3)
                .Text(titulo).FontColor(Colors.White).FontSize(8).Bold();
            columna.Item().Padding(6).Element(contenido);
        });
    }

    private static void Campo(IContainer contenedor, string etiqueta, string? valor)
    {
        contenedor.Column(columna =>
        {
            columna.Item().Text(etiqueta).FontSize(6.5f).FontColor(TintaSuave);
            columna.Item().BorderBottom(0.4f).BorderColor(Linea).PaddingTop(1).PaddingBottom(2)
                .Text(Valor(valor)).FontSize(9);
        });
    }

    private static void CampoLargo(IContainer contenedor, string etiqueta, string? valor, float altoMinimo)
    {
        contenedor.Column(columna =>
        {
            columna.Item().Text(etiqueta).FontSize(6.5f).FontColor(TintaSuave);
            columna.Item().MinHeight(altoMinimo).PaddingTop(2).Column(parrafos =>
            {
                parrafos.Spacing(5);
                foreach (var parrafo in SepararParrafos(valor))
                    parrafos.Item().Text(parrafo).FontSize(9).LineHeight(1.25f).Justify();
            });
        });
    }

    private static void BloqueFirma(ColumnDescriptor columna, string etiqueta)
    {
        columna.Item().PaddingTop(14).AlignRight().Width(230).Column(firma =>
        {
            firma.Item().Height(48);
            firma.Item().BorderBottom(0.7f).BorderColor(Navy);
            firma.Item().PaddingTop(4).AlignCenter().Text(etiqueta).FontSize(7).FontColor(TintaSuave);
        });
    }

    private static IEnumerable<string> SepararParrafos(string? valor)
    {
        var texto = Valor(valor).Replace("\r\n", "\n");
        foreach (var parte in texto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            yield return parte;
    }

    private static void Marca(IContainer contenedor, string texto, bool activo)
    {
        contenedor.Row(fila =>
        {
            fila.ConstantItem(11).Height(11).Border(0.8f).BorderColor(Navy)
                .AlignCenter().AlignMiddle()
                .Text(activo ? "X" : "").FontSize(8).Bold().FontColor(Navy);
            fila.ConstantItem(5);
            fila.AutoItem().AlignMiddle().Text(texto).FontSize(8);
        });
    }

    private static void Pie(IContainer contenedor, Reclamo reclamo)
    {
        contenedor.PaddingTop(4).AlignCenter().Text(texto =>
        {
            texto.DefaultTextStyle(estilo => estilo.FontSize(7).FontColor(TintaSuave));
            texto.Span($"HR {reclamo.NumeroTexto}  ·  Anexo I  ·  Página ");
            texto.CurrentPageNumber();
            texto.Span(" de ");
            texto.TotalPages();
        });
    }

    private static string Valor(string? texto, string vacio = "—") =>
        string.IsNullOrWhiteSpace(texto) ? vacio : texto.Trim();
}

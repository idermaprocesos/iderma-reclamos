using LibroReclamaciones.Models;

namespace LibroReclamaciones.Services;

public readonly record struct EstadoPlazo(string Texto, string Clave);

public static class PlazoService
{
    public static DateOnly Limite(Reclamo reclamo)
    {
        var limite = AgregarDiasHabiles(reclamo.FechaReclamo, 15);
        if (reclamo.HuboPropuesta || reclamo.TipoAtencion == Tipos.Propuesta)
            limite = AgregarDiasHabiles(limite, 5);
        return limite;
    }

    public static EstadoPlazo Evaluar(Reclamo reclamo, DateOnly hoy)
    {
        if (reclamo.Estado == Tipos.Respondido || reclamo.TipoAtencion == Tipos.Acuerdo && reclamo.FechaComunicacion is not null)
        {
            var cuando = reclamo.FechaComunicacion is null
                ? ""
                : $" el {Formato.Fecha(reclamo.FechaComunicacion)}";
            var etiqueta = reclamo.TipoAtencion == Tipos.Acuerdo ? "Acuerdo registrado" : "Respondido";
            return new EstadoPlazo($"{etiqueta}{cuando}", "respondido");
        }

        var limite = Limite(reclamo);
        var dias = DiasHabilesHasta(hoy, limite);
        var fechaLimite = Formato.Fecha(limite);

        if (reclamo.Estado == Tipos.PropuestaEnviada || reclamo.TipoAtencion == Tipos.Propuesta && reclamo.FechaComunicacion is not null)
        {
            var baseTexto = $"Propuesta enviada. Si no es aceptada, la respuesta formal vence el {fechaLimite}";
            if (dias < 0)
                return new EstadoPlazo($"{baseTexto}. El plazo ya venció.", "vencido");
            if (dias == 0)
                return new EstadoPlazo($"{baseTexto}. Vence hoy.", "vence");
            return new EstadoPlazo($"{baseTexto}. Quedan {dias} días hábiles.", dias <= 3 ? "pronto" : "ok");
        }

        if (dias < 0)
            return new EstadoPlazo($"Vencido el {fechaLimite} ({Math.Abs(dias)} días hábiles)", "vencido");
        if (dias == 0)
            return new EstadoPlazo($"Vence hoy ({fechaLimite})", "vence");
        if (dias <= 3)
            return new EstadoPlazo($"Por vencer: quedan {dias} días hábiles. Límite {fechaLimite}", "pronto");

        var suspension = reclamo.HuboPropuesta
            ? " Incluye 5 días hábiles de suspensión por una propuesta de solución."
            : "";
        return new EstadoPlazo($"En plazo: quedan {dias} días hábiles. Límite {fechaLimite}.{suspension}", "ok");
    }

    public static DateOnly AgregarDiasHabiles(DateOnly fecha, int dias)
    {
        var actual = fecha;
        var restantes = dias;
        while (restantes > 0)
        {
            actual = actual.AddDays(1);
            if (EsDiaHabil(actual))
                restantes--;
        }

        return actual;
    }

    public static int DiasHabilesHasta(DateOnly desde, DateOnly hasta)
    {
        if (hasta < desde)
            return -DiasHabilesHasta(hasta, desde);

        var cantidad = 0;
        var cursor = desde;
        while (cursor < hasta)
        {
            cursor = cursor.AddDays(1);
            if (EsDiaHabil(cursor))
                cantidad++;
        }

        return cantidad;
    }

    public static bool EsDiaHabil(DateOnly fecha) =>
        fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday && !EsFeriadoNacional(fecha);

    public static bool EsFeriadoNacional(DateOnly fecha)
    {
        if (fecha is { Month: 1, Day: 1 }
            or { Month: 5, Day: 1 }
            or { Month: 6, Day: 29 }
            or { Month: 7, Day: 28 }
            or { Month: 7, Day: 29 }
            or { Month: 8, Day: 30 }
            or { Month: 10, Day: 8 }
            or { Month: 11, Day: 1 }
            or { Month: 12, Day: 8 }
            or { Month: 12, Day: 9 }
            or { Month: 12, Day: 25 })
            return true;

        var pascua = CalcularPascua(fecha.Year);
        var juevesSanto = pascua.AddDays(-3);
        var viernesSanto = pascua.AddDays(-2);
        return fecha == juevesSanto || fecha == viernesSanto;
    }

    private static DateOnly CalcularPascua(int anio)
    {
        var a = anio % 19;
        var b = anio / 100;
        var c = anio % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var mes = (h + l - 7 * m + 114) / 31;
        var dia = ((h + l - 7 * m + 114) % 31) + 1;
        return new DateOnly(anio, mes, dia);
    }
}

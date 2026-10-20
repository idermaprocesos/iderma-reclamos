using LibroReclamaciones.Models;

namespace LibroReclamaciones.Services;

public static class BorradorRespuesta
{
    public static string Observaciones(Reclamo reclamo)
    {
        var bien = string.IsNullOrWhiteSpace(reclamo.DescripcionBien)
            ? "el bien contratado"
            : $"«{reclamo.DescripcionBien.Trim()}»";
        var tipo = reclamo.Tipo.Equals(Tipos.Queja, StringComparison.OrdinalIgnoreCase) ? "queja" : "reclamo";

        return reclamo.TipoAtencion switch
        {
            Tipos.Propuesta =>
                $"Se propone al consumidor la siguiente solución respecto de su {tipo} sobre {bien}:\n\n[Describa la medida concreta: cambio, devolución, repetición del servicio, descuento u otra.]\n\nEsta propuesta queda sujeta a la aceptación expresa del consumidor.",
            Tipos.Acuerdo =>
                $"El consumidor y el proveedor acordaron solucionar el {tipo} de la siguiente manera:\n\n[Describa el acuerdo y la forma en que se cumplirá.]\n\nACUERDO ACEPTADO PARA SOLUCIONAR EL RECLAMO.",
            _ when reclamo.Resultado == Tipos.Acoge =>
                $"Se acoge el pedido del consumidor respecto de {bien}. Las acciones que adoptará el proveedor son:\n\n[Indique qué se hará, en qué plazo y por qué medio se cumplirá.]",
            _ when reclamo.Resultado == Tipos.NoAcoge =>
                $"No se acoge el pedido del consumidor respecto de {bien}.\n\n[Explique los hechos verificados y por qué no corresponde lo solicitado. No deje este texto como afirmación si aún no lo ha comprobado.]",
            _ when reclamo.Resultado == Tipos.Parcial =>
                $"Se acoge parcialmente el pedido del consumidor respecto de {bien}.\n\nSe atenderá:\n[Indique qué parte se acepta y cómo se cumplirá.]\n\nNo corresponde atender:\n[Indique qué parte no se acepta.]",
            _ =>
                "Observaciones y acciones adoptadas por el proveedor:\n\n[Complete este texto antes de comunicar la respuesta.]"
        };
    }

    public static string Fundamento(Reclamo reclamo)
    {
        if (reclamo.TipoAtencion is Tipos.Acuerdo or Tipos.Propuesta)
            return "";

        if (reclamo.Resultado == Tipos.Acoge)
            return "El pedido guarda relación con el bien o servicio contratado y corresponde atenderlo en los términos descritos en las acciones adoptadas.";

        return "[Fundamente la posición con hechos verificables: lo contratado, la información brindada al consumidor, la prestación efectuada u otra razón objetiva.]";
    }
}

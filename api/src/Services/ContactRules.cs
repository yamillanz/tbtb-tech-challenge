using TbtbChallenge.Api.Exceptions;

namespace TbtbChallenge.Api.Services;

public static class ContactRules
{
    public static readonly string[] Channels = ["llamada", "whatsapp", "correo"];

    public static readonly string[] Results = ["contestado", "no contesta", "buzón", "número equivocado", "reagendado", "otro"];

    public static void ValidateChannel(string channel)
    {
        if (!Channels.Contains(channel))
        {
            throw new ValidationException("channel", $"El canal debe ser uno de: {string.Join(", ", Channels)}.");
        }
    }

    public static void ValidateResult(string result)
    {
        if (!Results.Contains(result))
        {
            throw new ValidationException("result", $"El resultado debe ser uno de: {string.Join(", ", Results)}.");
        }
    }

    public static void ValidateContactDate(DateOnly date)
    {
        var startOfMonth = StartOfCurrentProgramMonth();
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        if (date < startOfMonth || date > endOfMonth)
        {
            throw new ValidationException("contactDate", $"La fecha del contacto debe estar dentro del mes en curso ({startOfMonth:yyyy-MM-dd} a {endOfMonth:yyyy-MM-dd}).");
        }
    }

    public static DateOnly StartOfCurrentProgramMonth()
    {
        var today = ProgramToday();
        return new DateOnly(today.Year, today.Month, 1);
    }

    public static DateOnly ProgramToday()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));
    }
}

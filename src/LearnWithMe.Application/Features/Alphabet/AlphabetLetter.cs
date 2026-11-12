namespace LearnWithMe.Application.Features.Alphabet;

public sealed record AlphabetLetter(
    string Uppercase,
    string Lowercase,
    string ExampleWord,
    string Illustration,
    string Pronunciation)
{
    public static IReadOnlyList<AlphabetLetter> All { get; } =
    [
        new("A", "a", "auto", "🚗", "a"),
        new("Ą", "ą", "ząb", "🦷", "ą"),
        new("B", "b", "balon", "🎈", "be"),
        new("C", "c", "cytryna", "🍋", "ce"),
        new("Ć", "ć", "ćma", "🦋", "cie"),
        new("D", "d", "dom", "🏠", "de"),
        new("E", "e", "ekran", "🖥️", "e"),
        new("Ę", "ę", "gęś", "🪿", "ę"),
        new("F", "f", "foka", "🦭", "ef"),
        new("G", "g", "gitara", "🎸", "gie"),
        new("H", "h", "huśtawka", "🛝", "ha"),
        new("I", "i", "igła", "🪡", "i"),
        new("J", "j", "jabłko", "🍎", " jot"),
        new("K", "k", "kot", "🐱", "ka"),
        new("L", "l", "lis", "🦊", "el"),
        new("Ł", "ł", "łódka", "🛶", "eł"),
        new("M", "m", "miś", "🧸", "em"),
        new("N", "n", "nos", "👃", "en"),
        new("Ń", "ń", "koń", "🐴", "eń"),
        new("O", "o", "okno", "🪟", "o"),
        new("Ó", "ó", "ósemka", "8️⃣", "ó"),
        new("P", "p", "pies", "🐶", "pe"),
        new("R", "r", "rower", "🚲", "er"),
        new("S", "s", "słońce", "☀️", "es"),
        new("Ś", "ś", "ślimak", "🐌", "eś"),
        new("T", "t", "tęcza", "🌈", "te"),
        new("U", "u", "ucho", "👂", "u"),
        new("W", "w", "wiewiórka", "🐿️", "wu"),
        new("Y", "y", "yo-yo", "🪀", "igrek"),
        new("Z", "z", "zebra", "🦓", "zet"),
        new("Ź", "ź", "źrebak", "🐎", "zie"),
        new("Ż", "ż", "żaba", "🐸", "żet")
    ];
}

using LearnWithMe.Application.Abstractions;
using LearnWithMe.Domain.Entities;

namespace LearnWithMe.Application.Features.Math;

public sealed class MathLearningService(IMathProgressRepository progressRepository)
    : IMathLearningService
{
    private static readonly string[] Objects = ["🍎", "⭐", "🐻", "🍓", "🚗"];
    private static readonly string[] NumberNames = ["zero", "jeden", "dwa", "trzy", "cztery", "pięć", "sześć", "siedem", "osiem", "dziewięć", "dziesięć"];
    private static readonly string[] TeenNames = ["jedenaście", "dwanaście", "trzynaście", "czternaście", "piętnaście", "szesnaście", "siedemnaście", "osiemnaście", "dziewiętnaście", "dwadzieścia"];

    public IReadOnlyList<MathExerciseOption> ExerciseTypes { get; } =
    [
        new("Cyfry i liczby", MathTopic.Numbers),
        new("Liczymy", MathTopic.Counting),
        new("Dodawanie", MathTopic.Addition),
        new("Odejmowanie", MathTopic.Subtraction),
        new("Równe grupy", MathTopic.Multiplication),
        new("Dzielenie się", MathTopic.Division)
    ];

    public MathQuestion CreateQuestion(MathTopic topic)
    {
        if (!Enum.IsDefined(topic))
        {
            throw new ArgumentOutOfRangeException(nameof(topic), topic, "Unsupported math topic.");
        }

        var first = 0;
        var second = 0;
        var groups = 0;
        var perGroup = 0;

        var answer = topic switch
        {
            MathTopic.Numbers => Random.Shared.Next(0, 21),
            MathTopic.Counting => Random.Shared.Next(1, 11),
            MathTopic.Addition => CreateAddition(out first, out second),
            MathTopic.Subtraction => CreateSubtraction(out first, out second),
            MathTopic.Multiplication => CreateMultiplication(out groups, out perGroup),
            MathTopic.Division => CreateDivision(out groups, out perGroup),
            _ => throw new ArgumentOutOfRangeException(nameof(topic), topic, "Unsupported math topic.")
        };

        var prompt = topic switch
        {
            MathTopic.Numbers => $"Znajdź liczbę {answer}.",
            MathTopic.Counting => "Policz obrazki. Ile ich jest?",
            MathTopic.Addition => $"Ile to {first} + {second}?",
            MathTopic.Subtraction => $"Ile to {first} − {second}?",
            MathTopic.Multiplication => $"Są {groups} równe grupy. W każdej są {perGroup}. Ile jest razem?",
            MathTopic.Division => $"Podziel {groups * perGroup} po równo do {groups} koszyków. Ile będzie w każdym?",
            _ => throw new ArgumentOutOfRangeException(nameof(topic), topic, "Unsupported math topic.")
        };
        var visualHint = topic switch
        {
            MathTopic.Numbers => string.Empty,
            MathTopic.Counting => RepeatObject(answer),
            MathTopic.Addition => $"{RepeatObject(first)}  +  {RepeatObject(second)}",
            MathTopic.Subtraction => $"{RepeatObject(first)}  −  {RepeatObject(second)}",
            MathTopic.Multiplication => string.Join(
                "     ",
                Enumerable.Range(0, groups).Select(_ => RepeatObject(perGroup))),
            MathTopic.Division => string.Join(
                "     ",
                Enumerable.Range(0, groups).Select(_ => $"🧺 {RepeatObject(perGroup)}")),
            _ => string.Empty
        };

        return new MathQuestion(
            topic,
            prompt,
            visualHint,
            answer,
            CreateOptions(answer),
            topic == MathTopic.Numbers ? GetNumberName(answer) : string.Empty,
            topic == MathTopic.Numbers ? CreateNumberExamples(answer) : []);
    }

    public Task<IReadOnlyList<MathTopicProgress>> GetProgressAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        return progressRepository.GetProgressAsync(profileId, cancellationToken);
    }

    public Task RecordAnswerAsync(
        Guid profileId,
        MathTopic topic,
        bool wasCorrect,
        CancellationToken cancellationToken = default)
    {
        return progressRepository.RecordAnswerAsync(profileId, topic, wasCorrect, cancellationToken);
    }

    private static int CreateAddition(out int first, out int second)
    {
        first = Random.Shared.Next(0, 11);
        second = Random.Shared.Next(0, 21 - first);
        return first + second;
    }

    private static int CreateSubtraction(out int first, out int second)
    {
        first = Random.Shared.Next(1, 21);
        second = Random.Shared.Next(0, first + 1);
        return first - second;
    }

    private static int CreateMultiplication(out int groups, out int perGroup)
    {
        groups = Random.Shared.Next(2, 6);
        perGroup = Random.Shared.Next(2, System.Math.Min(5, 20 / groups) + 1);
        return groups * perGroup;
    }

    private static int CreateDivision(out int groups, out int perGroup)
    {
        groups = Random.Shared.Next(2, 6);
        perGroup = Random.Shared.Next(1, System.Math.Min(5, 20 / groups) + 1);
        return perGroup;
    }

    private static IReadOnlyList<int> CreateOptions(int answer)
    {
        var distractors = Enumerable.Range(0, 21)
            .Where(value => value != answer)
            .OrderBy(_ => Random.Shared.Next())
            .Take(3);

        return distractors
            .Append(answer)
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();
    }

    private static string RepeatObject(int count)
    {
        var item = Objects[count % Objects.Length];
        return string.Join(" ", Enumerable.Repeat(item, count));
    }

    private static string GetNumberName(int value)
    {
        return value <= 10 ? NumberNames[value] : TeenNames[value - 11];
    }

    private static IReadOnlyList<NumberReadingExample> CreateNumberExamples(int value)
    {
        return
        [
            CreateNumberExample(value, "🚗", "samochód", "samochody", "samochodów", false),
            CreateNumberExample(value, "👗", "sukienka", "sukienki", "sukienek", true),
            CreateNumberExample(value, "🧸", "zabawka", "zabawki", "zabawek", true)
        ];
    }

    private static NumberReadingExample CreateNumberExample(
        int value,
        string illustration,
        string singular,
        string few,
        string many,
        bool feminine)
    {
        if (value == 1)
        {
            var singularPhrase = $"{(feminine ? "jedna" : "jeden")} {singular}";
            return new NumberReadingExample(illustration, singularPhrase, singularPhrase);
        }

        var lastTwoDigits = value % 100;
        var lastDigit = value % 10;
        var usesFewForm = lastDigit is >= 2 and <= 4 && lastTwoDigits is not (>= 12 and <= 14);
        var numberName = value == 2 && feminine ? "dwie" : GetNumberName(value);
        return new NumberReadingExample(
            illustration,
            $"{numberName} {(usesFewForm ? few : many)}",
            $"{(feminine ? "jedna" : "jeden")} {singular}");
    }
}

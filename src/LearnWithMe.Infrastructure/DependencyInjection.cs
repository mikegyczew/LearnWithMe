using LearnWithMe.Application.Abstractions;
using LearnWithMe.Application.Features.Alphabet;
using LearnWithMe.Application.Features.Math;
using LearnWithMe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LearnWithMe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        services.AddDbContextFactory<LearnWithMeDbContext>(
            options => options.UseSqlite($"Data Source={databasePath}"));
        services.AddTransient<IChildProfileRepository, SqliteChildProfileRepository>();
        services.AddTransient<IProgressRepository, SqliteProgressRepository>();
        services.AddTransient<IMathProgressRepository, SqliteMathProgressRepository>();
        services.AddTransient<IAlphabetLearningService, AlphabetLearningService>();
        services.AddTransient<IMathLearningService, MathLearningService>();

        return services;
    }
}

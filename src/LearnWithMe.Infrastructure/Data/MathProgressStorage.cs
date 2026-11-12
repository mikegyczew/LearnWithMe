using Microsoft.EntityFrameworkCore;

namespace LearnWithMe.Infrastructure.Data;

internal static class MathProgressStorage
{
    public static async Task EnsureCreatedAsync(
        LearnWithMeDbContext context,
        CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "MathTopicProgress" (
                "ProfileId" TEXT NOT NULL,
                "Topic" INTEGER NOT NULL,
                "Attempts" INTEGER NOT NULL,
                "Correct" INTEGER NOT NULL,
                CONSTRAINT "PK_MathTopicProgress" PRIMARY KEY ("ProfileId", "Topic")
            );
            """,
            cancellationToken);
    }
}

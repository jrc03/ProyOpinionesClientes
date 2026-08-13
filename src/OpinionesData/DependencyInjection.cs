using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpinionesData.Context;
using OpinionesData.Facts;
using OpinionesData.Interfaces;
using OpinionesData.Sources;
using OpinionesData.Staging;

namespace OpinionesData;

public static class DependencyInjection
{
    public static IServiceCollection AddOpinionesData(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddPooledDbContextFactory<OpinionesDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<IWebReviewReader, EfWebReviewReader>();
        services.AddSingleton<IStagingWriter, EfStagingWriter>();
        services.AddSingleton<IOpinionLoadReader, EfOpinionLoadReader>();
        services.AddSingleton<IOpinionFactWriter, EfOpinionFactWriter>();

        return services;
    }
}

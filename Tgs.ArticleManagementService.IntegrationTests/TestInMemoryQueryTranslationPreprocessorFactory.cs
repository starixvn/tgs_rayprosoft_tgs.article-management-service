using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Tgs.ArticleManagementService.IntegrationTests;

internal class TestInMemoryQueryTranslationPreprocessorFactory<TQueryTranslationPreProcessor> : IQueryTranslationPreprocessorFactory
    where TQueryTranslationPreProcessor : QueryTranslationPreprocessor
{
    private readonly IServiceProvider _serviceProvider;

    public QueryTranslationPreprocessorDependencies Dependencies { get; }

    public TestInMemoryQueryTranslationPreprocessorFactory(QueryTranslationPreprocessorDependencies dependencies, IServiceProvider serviceProvider)
    {
        Dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
        => ActivatorUtilities.CreateInstance<TQueryTranslationPreProcessor>(
            _serviceProvider,
            Dependencies,
            queryCompilationContext);
}

using System;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Initializes Dapper and GraphQL with the dependency injection container.
        /// </summary>
        /// <param name="serviceCollection">The service collection container.</param>
        /// <param name="setup">An action used to initialize Dapper and GraphQL with the DI container.</param>
        /// <returns>The service collection container.</returns>
        public static IServiceCollection AddDapperGraphQl(this IServiceCollection serviceCollection, Action<DapperGraphQlOptions> setup)
        {
            var options = new DapperGraphQlOptions(serviceCollection);
            setup?.Invoke(options);

            return serviceCollection;
        }
    }
}

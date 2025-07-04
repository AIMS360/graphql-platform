using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace HotChocolate.Execution;

/// <summary>
/// Provides extensions for <see cref="IServiceProvider" /> to interact with the
/// <see cref="IRequestExecutor" />.
/// </summary>
public static class RequestExecutorServiceProviderExtensions
{
    /// <summary>
    /// Gets the <see cref="ISchemaDefinition" /> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// Returns the <see cref="IRequestExecutor" />.
    /// </returns>
    public static async ValueTask<ISchemaDefinition> GetSchemaAsync(
        this IServiceProvider services,
        string? schemaName = null,
        CancellationToken cancellationToken = default)
    {
        var executor =
            await GetRequestExecutorAsync(services, schemaName, cancellationToken)
                .ConfigureAwait(false);

        return executor.Schema;
    }

    /// <summary>
    /// Builds the <see cref="ISchemaDefinition" /> from the <see cref="IRequestExecutorBuilder"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IRequestExecutorBuilder"/>.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// Returns the <see cref="IRequestExecutor" />.
    /// </returns>
    public static async ValueTask<Schema> BuildSchemaAsync(
        this IRequestExecutorBuilder builder,
        string? schemaName = null,
        CancellationToken cancellationToken = default)
    {
        IServiceProvider services = builder.Services.BuildServiceProvider();

        var executor =
            await GetRequestExecutorAsync(services, schemaName, cancellationToken)
                .ConfigureAwait(false);

        return (Schema)executor.Schema;
    }

    /// <summary>
    /// Gets the <see cref="IRequestExecutor" /> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// Returns the <see cref="IRequestExecutor" />.
    /// </returns>
    public static ValueTask<IRequestExecutor> GetRequestExecutorAsync(
        this IServiceProvider services,
        string? schemaName = null,
        CancellationToken cancellationToken = default) =>
        services
            .GetRequiredService<IRequestExecutorProvider>()
            .GetExecutorAsync(schemaName, cancellationToken);

    /// <summary>
    /// Builds the <see cref="IRequestExecutor" /> from the
    /// <see cref="IRequestExecutorBuilder"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IRequestExecutorBuilder"/>.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// Returns the <see cref="IRequestExecutor" />.
    /// </returns>
    public static ValueTask<IRequestExecutor> BuildRequestExecutorAsync(
        this IRequestExecutorBuilder builder,
        string? schemaName = null,
        CancellationToken cancellationToken = default) =>
        builder
            .Services
            .BuildServiceProvider()
            .GetRequiredService<IRequestExecutorProvider>()
            .GetExecutorAsync(schemaName, cancellationToken);

    /// <summary>
    /// Executes the given GraphQL <paramref name="request" />.
    /// </summary>
    /// <param name="services">
    /// The service provider that contains the executor.
    /// </param>
    /// <param name="request">
    /// The GraphQL request object.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// Returns the execution result of the given GraphQL <paramref name="request" />.
    ///
    /// If the request operation is a simple query or mutation the result is a
    /// <see cref="IOperationResult" />.
    ///
    /// If the request operation is a query or mutation where data is deferred, streamed or
    /// includes live data the result is a <see cref="IResponseStream" /> where each result
    /// that the <see cref="IResponseStream" /> yields is a <see cref="IOperationResult" />.
    ///
    /// If the request operation is a subscription the result is a
    /// <see cref="IResponseStream" /> where each result that the
    /// <see cref="IResponseStream" /> yields is a
    /// <see cref="IOperationResult" />.
    /// </returns>
    public static async Task<IExecutionResult> ExecuteRequestAsync(
        this IServiceProvider services,
        IOperationRequest request,
        string? schemaName = null,
        CancellationToken cancellationToken = default)
    {
        var executor =
            await GetRequestExecutorAsync(services, schemaName, cancellationToken)
                .ConfigureAwait(false);

        return await executor
            .ExecuteAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given GraphQL <paramref name="request" />.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IRequestExecutorBuilder"/>.
    /// </param>
    /// <param name="request">
    /// The GraphQL request object.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// Returns the execution result of the given GraphQL <paramref name="request" />.
    ///
    /// If the request operation is a simple query or mutation the result is a
    /// <see cref="IOperationResult" />.
    ///
    /// If the request operation is a query or mutation where data is deferred, streamed or
    /// includes live data the result is a <see cref="IResponseStream" /> where each result
    /// that the <see cref="IResponseStream" /> yields is a <see cref="IOperationResult" />.
    ///
    /// If the request operation is a subscription the result is a
    /// <see cref="IResponseStream" /> where each result that the
    /// <see cref="IResponseStream" /> yields is a
    /// <see cref="IOperationResult" />.
    /// </returns>
    public static async Task<IExecutionResult> ExecuteRequestAsync(
        this IRequestExecutorBuilder builder,
        IOperationRequest request,
        string? schemaName = null,
        CancellationToken cancellationToken = default)
    {
        var executor =
            await BuildRequestExecutorAsync(builder, schemaName, cancellationToken)
                .ConfigureAwait(false);

        return await executor
            .ExecuteAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given GraphQL <paramref name="query" />.
    /// </summary>
    /// <param name="services">
    /// The service provider that contains the executor.
    /// </param>
    /// <param name="query">
    /// The GraphQL query.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// Returns the execution result of the given GraphQL <paramref name="query" />.
    ///
    /// If the request operation is a simple query or mutation the result is a
    /// <see cref="IOperationResult" />.
    ///
    /// If the request operation is a query or mutation where data is deferred, streamed or
    /// includes live data the result is a <see cref="IResponseStream" /> where each result
    /// that the <see cref="IResponseStream" /> yields is a
    /// <see cref="IOperationResult" />.
    ///
    /// If the request operation is a subscription the result is a
    /// <see cref="IResponseStream" /> where each result that the
    /// <see cref="IResponseStream" /> yields is a
    /// <see cref="IOperationResult" />.
    /// </returns>
    public static async Task<IExecutionResult> ExecuteRequestAsync(
        this IServiceProvider services,
        string query,
        string? schemaName = null,
        CancellationToken cancellationToken = default)
    {
        var executor =
            await GetRequestExecutorAsync(services, schemaName, cancellationToken)
                .ConfigureAwait(false);

        return await executor
            .ExecuteAsync(query, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given GraphQL <paramref name="query" />.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IRequestExecutorBuilder"/>.
    /// </param>
    /// <param name="query">
    /// The GraphQL query.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// <para>Returns the execution result of the given GraphQL <paramref name="query" />.</para>
    /// <para>
    /// If the request operation is a simple query or mutation the result is a
    /// <see cref="IOperationResult" />.
    /// </para>
    /// <para>
    /// If the request operation is a query or mutation where data is deferred, streamed or
    /// includes live data the result is a <see cref="IResponseStream" /> where each result
    /// that the <see cref="IResponseStream" /> yields is a
    /// <see cref="IOperationResult" />.
    /// </para>
    /// <para>
    /// If the request operation is a subscription the result is a
    /// <see cref="IResponseStream" /> where each result that the
    /// <see cref="IResponseStream" /> yields is a
    /// <see cref="IOperationResult" />.
    /// </para>
    /// </returns>
    public static async Task<IExecutionResult> ExecuteRequestAsync(
        this IRequestExecutorBuilder builder,
        string query,
        string? schemaName = null,
        CancellationToken cancellationToken = default)
    {
        var executor =
            await BuildRequestExecutorAsync(builder, schemaName, cancellationToken)
                .ConfigureAwait(false);

        return await executor
            .ExecuteAsync(query, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given GraphQL <paramref name="requestBatch" />.
    /// </summary>
    /// <param name="services">
    /// The service provider that contains the executor.
    /// </param>
    /// <param name="requestBatch">
    /// The GraphQL request batch.
    /// </param>
    /// <param name="schemaName">
    /// The schema name.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// Returns a stream of query results.
    /// </returns>
    public static async Task<IResponseStream> ExecuteBatchRequestAsync(
        this IServiceProvider services,
        OperationRequestBatch requestBatch,
        string? schemaName = null,
        CancellationToken cancellationToken = default)
    {
        var executor =
            await GetRequestExecutorAsync(services, schemaName, cancellationToken)
                .ConfigureAwait(false);

        return await executor
            .ExecuteBatchAsync(requestBatch, cancellationToken)
            .ConfigureAwait(false);
    }
}

﻿using System;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
using Kafka.DotNet.ksqlDB.KSql.Disposables;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context
{
  public class KSqlDBContext : AsyncDisposableObject, IKSqlDBContext
  {
    private readonly KSqlDBContextOptions contextOptions;
    private readonly IServiceCollection serviceCollection;

    public KSqlDBContext(string ksqlDbUrl)
      : this(new KSqlDBContextOptions(ksqlDbUrl))
    {
    }

    public KSqlDBContext(KSqlDBContextOptions contextOptions)
    {
      this.contextOptions = contextOptions ?? throw new ArgumentNullException(nameof(contextOptions));

      serviceCollection = new ServiceCollection();
    }

    private void RegisterDependencies()
    {
      OnConfigureServices(serviceCollection);

      serviceCollection.TryAddScoped<IKSqlQbservableProvider, QbservableProvider>();

      var uri = new Uri(contextOptions.Url);

      serviceCollection.TryAddTransient<IKSqlQueryGenerator, KSqlQueryGenerator>();

      if (!serviceCollection.HasRegistration<IHttpClientFactory>())
        serviceCollection.AddSingleton<IHttpClientFactory, HttpClientFactory>(_ =>
          new HttpClientFactory(uri));
      
      serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryStreamProvider>();
      serviceCollection.TryAddSingleton(contextOptions);
      serviceCollection.TryAddSingleton(contextOptions.QueryStreamParameters);
      serviceCollection.TryAddScoped<IKStreamSetDependencies, KStreamSetDependencies>();
    }

    protected virtual void OnConfigureServices(IServiceCollection serviceCollection)
    {

    }

    private void RegisterHttpClientFactory<TFactory>()
      where TFactory: class, IHttpClientFactory
    {
      serviceCollection.AddSingleton<IHttpClientFactory, TFactory>();
    }

    private void RegisterKSqlDbProvider<TProvider>()
      where TProvider: class, IKSqlDbProvider
    {
      serviceCollection.AddScoped<IKSqlDbProvider, TProvider>();
    }

    private ServiceProvider ServiceProvider { get; set; }

    private bool wasConfigured;

    public IQbservable<TEntity> CreateQueryStream<TEntity>(string streamName = null)
    {
      var serviceScopeFactory = Initialize();

      if (streamName == String.Empty)
        streamName = null;

      var queryStreamContext = new QueryContext
      {
        StreamName = streamName
      };

      return new KQueryStreamSet<TEntity>(serviceScopeFactory, queryStreamContext);
    }

    internal IServiceScopeFactory Initialize()
    {
      if (!wasConfigured)
      {
        wasConfigured = true;

        RegisterDependencies();

        ServiceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions {ValidateScopes = true});
      }

      var serviceScopeFactory = ServiceProvider.GetService<IServiceScopeFactory>();

      return serviceScopeFactory;
    }

    protected override async ValueTask OnDisposeAsync()
    {
      if(ServiceProvider != null)
        await ServiceProvider.DisposeAsync();
    }
  }
}
﻿using System.Reactive.Concurrency;
using Sample.Domain.Models;
using SqlTableDependency.Extensions.Redis.ConnectionMultiplexers;
using SqlTableDependency.Extensions.Redis.SqlTableDependency;

namespace SqlTableDependency.Extensions.Sample.Redis
{
  public class ProductSqlTableDependencyRedisProvider : SqlTableDependencyRedisProvider<Product>
  {
    public ProductSqlTableDependencyRedisProvider(ISqlTableDependencyProvider<Product> sqlTableDependencyProvider, IRedisPublisher redisPublisher, IScheduler scheduler) 
      : base(sqlTableDependencyProvider, redisPublisher, scheduler)
    {
    }
  }
}
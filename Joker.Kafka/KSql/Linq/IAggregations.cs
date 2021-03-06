﻿using System;

namespace Kafka.DotNet.ksqlDB.KSql.Linq
{
  public interface IAggregations
  { 
    int Count();
    long LongCount();
  }

  public interface IAggregations<out TSource> : IAggregations
  {
    int Count(Func<TSource, object> selector);
    long LongCount(Func<TSource, object> selector);

    int Avg(Func<TSource, int?> selector);
    long Avg(Func<TSource, long?> selector);
    decimal Avg(Func<TSource, decimal?> selector);
    decimal Avg(Func<TSource, float?> selector);
    decimal Avg(Func<TSource, double?> selector);
    int Avg(Func<TSource, int> selector);
    long Avg(Func<TSource, long> selector);
    decimal Avg(Func<TSource, float> selector);
    decimal Avg(Func<TSource, double> selector);
    decimal Avg(Func<TSource, decimal> selector);
    
    int Min(Func<TSource, int?> selector);
    long Min(Func<TSource, long?> selector);
    decimal Min(Func<TSource, decimal?> selector);
    decimal Min(Func<TSource, float?> selector);
    decimal Min(Func<TSource, double?> selector);
    int Min(Func<TSource, int> selector);
    long Min(Func<TSource, long> selector);
    decimal Min(Func<TSource, float> selector);
    decimal Min(Func<TSource, double> selector);
    decimal Min(Func<TSource, decimal> selector);
    
    int Max(Func<TSource, int?> selector);
    long Max(Func<TSource, long?> selector);
    decimal Max(Func<TSource, decimal?> selector);
    decimal Max(Func<TSource, float?> selector);
    decimal Max(Func<TSource, double?> selector);
    int Max(Func<TSource, int> selector);
    long Max(Func<TSource, long> selector);
    decimal Max(Func<TSource, float> selector);
    decimal Max(Func<TSource, double> selector);
    decimal Max(Func<TSource, decimal> selector);
    
    int Sum(Func<TSource, int?> selector);
    long Sum(Func<TSource, long?> selector);
    decimal Sum(Func<TSource, decimal?> selector);
    decimal Sum(Func<TSource, float?> selector);
    decimal Sum(Func<TSource, double?> selector);
    int Sum(Func<TSource, int> selector);
    long Sum(Func<TSource, long> selector);
    decimal Sum(Func<TSource, float> selector);
    decimal Sum(Func<TSource, double> selector);
    decimal Sum(Func<TSource, decimal> selector);

    short[] TopK(Func<TSource, short> selector, int k);
    int[] TopK(Func<TSource, int> selector, int k);
    long[] TopK(Func<TSource, long> selector, int k);
    float[] TopK(Func<TSource, float> selector, int k);
    double[] TopK(Func<TSource, double> selector, int k);
    decimal[] TopK(Func<TSource, decimal> selector, int k);

    short[] TopKDistinct(Func<TSource, short> selector, int k);
    int[] TopKDistinct(Func<TSource, int> selector, int k);
    long[] TopKDistinct(Func<TSource, long> selector, int k);
    float[] TopKDistinct(Func<TSource, float> selector, int k);
    double[] TopKDistinct(Func<TSource, double> selector, int k);
    decimal[] TopKDistinct(Func<TSource, decimal> selector, int k);

    string EarliestByOffset(Func<TSource, string> selector);
    int EarliestByOffset(Func<TSource, int> selector);
    long EarliestByOffset(Func<TSource, long> selector);
    float EarliestByOffset(Func<TSource, float> selector);
    double EarliestByOffset(Func<TSource, double> selector);
    decimal EarliestByOffset(Func<TSource, decimal> selector);

    string EarliestByOffsetAllowNulls(Func<TSource, string> selector);
    int? EarliestByOffsetAllowNulls(Func<TSource, int?> selector);
    long? EarliestByOffsetAllowNulls(Func<TSource, long?> selector);
    float? EarliestByOffsetAllowNulls(Func<TSource, float?> selector);
    double? EarliestByOffsetAllowNulls(Func<TSource, double?> selector);
    decimal? EarliestByOffsetAllowNulls(Func<TSource, decimal?> selector);
    
    string LatestByOffset(Func<TSource, string> selector);
    int LatestByOffset(Func<TSource, int> selector);
    long LatestByOffset(Func<TSource, long> selector);
    float LatestByOffset(Func<TSource, float> selector);
    double LatestByOffset(Func<TSource, double> selector);
    decimal LatestByOffset(Func<TSource, decimal> selector);

    string LatestByOffsetAllowNulls(Func<TSource, string> selector);
    int? LatestByOffsetAllowNulls(Func<TSource, int?> selector);
    long? LatestByOffsetAllowNulls(Func<TSource, long?> selector);
    float? LatestByOffsetAllowNulls(Func<TSource, float?> selector);
    double? LatestByOffsetAllowNulls(Func<TSource, double?> selector);
    decimal? LatestByOffsetAllowNulls(Func<TSource, decimal?> selector);
  }
}
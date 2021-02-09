﻿using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.Query;

namespace Kafka.DotNet.ksqlDB.Sample.Models
{
  public class Tweet : Record
  {
    public int Id { get; set; }

    [JsonPropertyName("MESSAGE")]
    public string Message { get; set; }

    
    public double Amount { get; set; }

    public decimal AccountBalance { get; set; }
  }
}
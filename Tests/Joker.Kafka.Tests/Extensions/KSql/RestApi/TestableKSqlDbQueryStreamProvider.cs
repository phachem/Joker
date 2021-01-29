﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.Tests.Helpers;
using Moq;
using Moq.Protected;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi
{
  internal class TestableKSqlDbQueryStreamProvider : KSqlDbQueryStreamProvider
  {
    public TestableKSqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory) 
      : base(httpClientFactory)
    {
    }

    public bool ShouldThrowException { get; set; }

    protected string QueryResponse =
      @"{""queryId"":""59df818e-7d88-436f-95ac-3c59becc9bfb"",""columnNames"":[""ROWTIME"",""MESSAGE"",""ID"",""ISROBOT"",""ACCOUNTBALANCE"",""AMOUNT""],""columnTypes"":[""BIGINT"",""STRING"",""INTEGER"",""BOOLEAN"",""DECIMAL(16, 4)"",""DOUBLE""]}
[1611327570881,""Hello world"",1,true,9999999999999999.1234,4.2E-4]
[1611327671476,""Wall-e"",2,false,1.2000,1.0]";

    protected string ErrorResponse =
      @"{""@type"":""generic_error"",""error_code"":40001,""message"":""Line: 1, Col: 21: SELECT column 'Foo' cannot be resolved.\nStatement: SELECT Message, Id, Foo FROM Tweets\r\nWHERE Message = 'Hello world' EMIT CHANGES LIMIT 2;""}";

    protected override HttpClient OnCreateHttpClient()
    {      

      var handlerMock = new Mock<HttpMessageHandler>();

      handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          nameof(HttpClient.SendAsync),
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage()
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(ShouldThrowException ? ErrorResponse : QueryResponse),
        })
        .Verifiable();

      return new HttpClient(handlerMock.Object)
      {
        BaseAddress = new Uri(TestParameters.KsqlDBUrl)
      };
    }
  }
}
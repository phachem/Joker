﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Exceptions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using Kafka.DotNet.ksqlDB.Tests.Pocos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi
{
  [TestClass]
  public class KSqlDbProviderTests: TestBase
  {  
    private TestableKSqlDbQueryStreamProvider ClassUnderTest { get; set; }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ClassUnderTest = MockingKernel.Get<TestableKSqlDbQueryStreamProvider>();
    }

    [TestMethod]
    public async Task Run_HttpStatusCodeOK_ReturnsTweets()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = ClassUnderTest.Run<Tweet>(queryParameters);

      //Assert
      var receivedTweets = new List<Tweet>();
      await foreach (var tweet in tweets)
      {
        tweet.Should().NotBeNull();
        receivedTweets.Add(tweet);
      }

      receivedTweets.Count.Should().Be(2);
    }

    [TestMethod]
    public async Task Run_HttpStatusCodeOK_StringFieldWasParsed()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

      //Assert
      var tweet = tweets[0];

      tweet.Message.Should().Be("Hello world");
    }

    [TestMethod]
    public async Task Run_HttpStatusCodeOK_BooleanFieldWasParsed()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

      //Assert
      var tweet = tweets[0];

      tweet.IsRobot.Should().BeTrue();
    }

    [TestMethod]
    public async Task Run_HttpStatusCodeOK_DoubleFieldWasParsed()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

      //Assert
      var tweet = tweets[0];

      tweet.Amount.Should().Be(0.00042);
    }

    [TestMethod]
    public async Task Run_HttpStatusCodeOK_DecimalFieldWasParsed()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

      //Assert
      var tweet = tweets[0];

      tweet.AccountBalance.Should().Be(9999999999999999.1234M);
    }

    [TestMethod]
    public async Task Run_HttpStatusCodeOK_BigintRowTimeFieldWasParsed()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

      //Assert
      var tweet = tweets[0];

      tweet.RowTime.Should().Be(1611327570881);
    }

    [TestMethod]
    public async Task Run_HttpStatusCodeOK_IntegerFieldWasParsed()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = await ClassUnderTest.Run<Tweet>(queryParameters).ToListAsync();

      //Assert
      var tweet = tweets[0];

      tweet.Id.Should().Be(1);
    }

    [TestMethod]
    [ExpectedException(typeof(KSqlQueryException))]
    public async Task Run_HttpStatusCodeBadRequest_ThrowsException()
    {
      //Arrange
      ClassUnderTest.ShouldThrowException = true;

      var queryParameters = new QueryStreamParameters();

      //Act
      var tweets = ClassUnderTest.Run<Tweet>(queryParameters);

      //Assert
      await foreach (var tweet in tweets)
      {
        tweet.Should().NotBeNull();
      }
    }

    [TestMethod]
    public async Task Run_Disposed_NothingWasReceived()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();
      var cts = new CancellationTokenSource();

      //Act
      IAsyncEnumerable<Tweet> tweets = ClassUnderTest.Run<Tweet>(queryParameters, cts.Token);
      cts.Cancel();

      //Assert
      var receivedTweets = new List<Tweet>();

      await foreach (var tweet in tweets.WithCancellation(cts.Token))
      {
        receivedTweets.Add(tweet);
      }

      receivedTweets.Should().BeEmpty();
      cts.Dispose();
    }
  }

  [TestClass]
  public class SingleValueKSqlDbProviderTests : TestBase
  {
    [TestMethod]
    public async Task Count_ParseSingleFields_IntegersAreConsumed()
    {
      //Arrange
      var provider = MockingKernel.Get<AggregationsKsqlDbQueryStreamProvider>();
      var queryParameters = new QueryStreamParameters();

      //Act
      var counts = provider.Run<int>(queryParameters);     
      
      //Assert
      (await counts.ToListAsync()).Count.Should().Be(2);
    }

    [TestMethod]
    public async Task NewCount_ParseSingleFields_IntegersAreConsumed()
    {
      //Arrange

      //Act
      var counts = Run(new { Count = 1 });     
      
      //Assert
      (await counts.ToListAsync()).Count.Should().Be(2);
    }
    
    IAsyncEnumerable<T> Run<T>(T anonymousType) {
      var provider = MockingKernel.Get<AggregationsKsqlDbQueryStreamProvider>();
      var queryParameters = new QueryStreamParameters();

      var asyncEnumerable = provider.Run<T>(queryParameters);

      return asyncEnumerable;
    }
  }
}
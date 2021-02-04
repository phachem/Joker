﻿using System;
using Kafka.DotNet.ksqlDB.Sample.Model;

namespace Kafka.DotNet.ksqlDB.Sample.Observers
{
  public class TweetsObserver : IObserver<Tweet>
  {
    public void OnNext(Tweet tweetMessage)
    {
      Console.WriteLine($"{nameof(Tweet)}: {tweetMessage.Id} - {tweetMessage.Message}");
    }

    public void OnError(Exception error)
    {
      Console.WriteLine($"{nameof(Tweet)}: {error.Message}");
    }

    public void OnCompleted()
    {
      Console.WriteLine($"{nameof(Tweet)}: completed successfully");
    }
  }
}
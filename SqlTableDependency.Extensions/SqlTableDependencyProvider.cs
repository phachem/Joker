﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SqlTableDependency.Extensions.Disposables;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.Abstracts;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace SqlTableDependency.Extensions
{
  public abstract class SqlTableDependencyProvider<TEntity> : DisposableObject, ISqlTableDependencyProvider
      where TEntity : class, new()
  {
    #region Fields

    private readonly string connectionString;
    private readonly IScheduler scheduler;

    #endregion

    #region Constructors

    protected SqlTableDependencyProvider(ConnectionStringSettings connectionStringSettings, IScheduler scheduler)
      : this(connectionStringSettings.ConnectionString, scheduler)
    {
    }

    protected SqlTableDependencyProvider(string connectionString, IScheduler scheduler)
    {
      if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

      this.connectionString = connectionString;
      this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    #endregion

    #region Properties

    #region WhenEntityRecordChanges

    private Subject<RecordChangedEventArgs<TEntity>> whenEntityRecordChangesSubject;

    public IObservable<RecordChangedEventArgs<TEntity>> WhenEntityRecordChanges
    {
      get
      {
        if (whenEntityRecordChangesSubject == null)
        {
          whenEntityRecordChangesSubject = new Subject<RecordChangedEventArgs<TEntity>>();
        }

        return whenEntityRecordChangesSubject.AsObservable();
      }
    }

    #endregion

    #region TableName

    protected virtual string TableName => typeof(TEntity).Name;

    #endregion

    #region IsDatabaseAvailable

    private readonly TimeSpan testConnectionTimeout = TimeSpan.FromSeconds(2);

    protected virtual bool IsDatabaseAvailable
    {
      get
      {
        var connected = TestConnection(connectionString, testConnectionTimeout);

        return connected;
      }
    }

    #endregion

    #region ReconnectionTimeSpan

    public virtual TimeSpan ReconnectionTimeSpan => TimeSpan.FromSeconds(5);

    #endregion

    #endregion

    #region Methods

    #region SubscribeToEntityChanges

    public void SubscribeToEntityChanges()
    {
      TrySubscribeToTableChanges();
    }

    #endregion

    #region TryReconnect

    private readonly SerialDisposable reconnectSubscription = new SerialDisposable();

    private void TryReconnect()
    {
      reconnectSubscription.Disposable = Observable.Timer(ReconnectionTimeSpan, ReconnectionTimeSpan, scheduler)
        .Where(c => IsDatabaseAvailable)
        .Take(1)
        .Subscribe(_ =>
        {
          OnBeforeServiceBrokerSubscription();

          TrySubscribeToTableChanges();
        }, error => TryReconnect());
    }

    #endregion

    #region OnBeforeServiceBrokerSubscription

    protected virtual void OnBeforeServiceBrokerSubscription()
    {
    }

    #endregion

    #region CreateSqlTableDependency

    protected virtual ITableDependency<TEntity> CreateSqlTableDependency(IModelToTableMapper<TEntity> modelToTableMapper)
    {
      var sqlTableDependency = new SqlTableDependency<TEntity>(connectionString, TableName, mapper: modelToTableMapper);

      return sqlTableDependency;
    }

    #endregion

    #region TrySubscribeToTableChanges

    private ITableDependency<TEntity> sqlTableDependency;

    private void TrySubscribeToTableChanges()
    {
      TryStopLastConnection();

      var modelToTableMapper = OnInitializeMapper(new ModelToTableMapper<TEntity>());

      try
      {
        sqlTableDependency = CreateSqlTableDependency(modelToTableMapper);

        sqlTableDependency.OnChanged += SqlTableDependencyOnChanged;
        sqlTableDependency.OnError += SqlTableDependencyOnError;
        sqlTableDependency.OnStatusChanged += SqlTableDependencyOnStatusChanged;

        sqlTableDependency.Start();

        OnConnected();
      }
      catch (Exception error)
      {
        OnError(error);

        TryReconnect();
      }
    }

    #endregion

    #region OnConnected

    protected virtual void OnConnected()
    {
    }

    #endregion

    #region OnError

    protected virtual void OnError(Exception error)
    {
    }

    #endregion

    #region SqlTableDependencyOnChanged

    private void SqlTableDependencyOnChanged(object sender, RecordChangedEventArgs<TEntity> eventArgs)
    {
      var entity = eventArgs.Entity;

      switch (eventArgs.ChangeType)
      {
        case ChangeType.Insert:
          OnInserted(entity);
          break;
        case ChangeType.Update:
          OnUpdated(entity);
          break;
        case ChangeType.Delete:
          OnDeleted(entity);
          break;
      }

      whenEntityRecordChangesSubject.OnNext(eventArgs);
    }

    #endregion

    #region SqlTableDependencyOnStatusChanged

    protected virtual void SqlTableDependencyOnStatusChanged(object sender, StatusChangedEventArgs e)
    {
    }

    #endregion

    #region SqlTableDependencyOnError

    protected virtual void SqlTableDependencyOnError(object sender, ErrorEventArgs e)
    {
      OnError(e.Error);

      TryReconnect();
    }

    #endregion

    #region OnInitializeMapper

    protected virtual ModelToTableMapper<TEntity> OnInitializeMapper(ModelToTableMapper<TEntity> modelToTableMapper)
    {
      return modelToTableMapper;
    }

    #endregion

    #region OnInserted

    protected virtual void OnInserted(TEntity entity)
    {
    }

    #endregion

    #region OnUpdated

    protected virtual void OnUpdated(TEntity entity)
    {
    }

    #endregion

    #region OnDeleted

    protected virtual void OnDeleted(TEntity entity)
    {
    }

    #endregion

    #region TryStopLastConnection

    protected void TryStopLastConnection()
    {
      if (sqlTableDependency == null)
        return;

      sqlTableDependency.OnError -= SqlTableDependencyOnError;
      sqlTableDependency.OnStatusChanged -= SqlTableDependencyOnStatusChanged;
      sqlTableDependency.OnChanged -= SqlTableDependencyOnChanged;

      try
      {
        sqlTableDependency.Dispose();
      }
      catch (Exception e)
      {
        OnError(e);
      }
    }

    #endregion

    #region OnDispose

    protected override void OnDispose()
    {
      base.OnDispose();

      using (reconnectSubscription)
      using (whenEntityRecordChangesSubject)
      {
      }

      TryStopLastConnection();
    }

    #endregion

    #region TestConnection

    public static bool TestConnection(string connectionString, TimeSpan testConnectionTimeout)
    {
      var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
      {
        ConnectionString = connectionString,
        ConnectTimeout = (int)testConnectionTimeout.TotalSeconds
      };

      using (var connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
      {
        try
        {
          connection.Open();

          return true;
        }
        catch
        {
          return false;
        }
      }
    }

    #endregion

    #endregion
  }
}
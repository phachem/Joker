﻿using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal class KSqlFunctionVisitor : KSqlVisitor
  {
    public KSqlFunctionVisitor(StringBuilder stringBuilder, bool useTableAlias)
      : base(stringBuilder, useTableAlias)
    {
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
      var methodInfo = methodCallExpression.Method;

      if (methodCallExpression.Object == null
          && methodInfo.DeclaringType.Name == nameof(KSqlFunctionsExtensions))
      {
        switch (methodInfo.Name)
        {          
          case nameof(KSqlFunctionsExtensions.Dynamic):
            if(methodCallExpression.Arguments[1] is ConstantExpression constantExpression)
              Append($"{constantExpression.Value}");
            else
              Visit(methodCallExpression.Arguments[1]);
            break;
          case nameof(KSqlFunctionsExtensions.Like):
            Visit(methodCallExpression.Arguments[1]);
            Append(" LIKE ");
            Visit(methodCallExpression.Arguments[2]);
            break;
          case nameof(KSqlFunctionsExtensions.Trim):
          case nameof(KSqlFunctionsExtensions.Abs):
          case nameof(KSqlFunctionsExtensions.Ceil):
          case nameof(KSqlFunctionsExtensions.Floor):
          case nameof(KSqlFunctionsExtensions.Sign):
            Append($"{methodInfo.Name.ToUpper()}(");
            Visit(methodCallExpression.Arguments[1]);
            Append(")");
            break;          
          case nameof(KSqlFunctionsExtensions.Random):
            Append($"{methodInfo.Name.ToUpper()}()");
            break;
          case nameof(KSqlFunctionsExtensions.LPad):
          case nameof(KSqlFunctionsExtensions.Round):
          case nameof(KSqlFunctionsExtensions.RPad):
          case nameof(KSqlFunctionsExtensions.Substring):
            Append($"{methodInfo.Name.ToUpper()}");
            PrintFunctionArguments(methodCallExpression.Arguments.Skip(1));
            break;
        }
      }
      else base.VisitMethodCall(methodCallExpression);

      return methodCallExpression;
    }
  }
}
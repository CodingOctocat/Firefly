using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Firefly.Models.Responses;

using Microsoft.EntityFrameworkCore;

namespace Firefly.Extensions;

public static class CccfQueryableExtensions
{
    private static MethodInfo AnyMethod { get; } = typeof(Enumerable)
        .GetMethods()
        .First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2)
        .MakeGenericMethod(typeof(string));

    private static Expression DbFunctionsArg { get; } = Expression.Constant(EF.Functions);

    private static MethodInfo LikeMethod { get; } = typeof(DbFunctionsExtensions)
        .GetMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])!;

    public static IQueryable<Cccf> FilterByLikeProperty(this IQueryable<Cccf> queryable, Expression<Func<Cccf, IEnumerable<string>>> propertyExpression, string match)
    {
        if (String.IsNullOrWhiteSpace(match))
        {
            return queryable;
        }

        if (match.StartsWith('='))
        {
            return queryable.Where(BuildAnyEqualsPredicate(propertyExpression, match[1..]));
        }

        bool hasWildcard = match.Contains('*') || match.Contains('?');

        if (hasWildcard)
        {
            match = match.Replace('*', '%').Replace('?', '_');
        }
        else
        {
            match = $"%{match}%";
        }

        return queryable.Where(BuildAnyLikePredicate(propertyExpression, match));
    }

    public static IQueryable<Cccf> FilterByLikeProperty(this IQueryable<Cccf> queryable, Expression<Func<Cccf, string>> propertyExpression, string match)
    {
        if (String.IsNullOrWhiteSpace(match))
        {
            return queryable;
        }

        if (match.StartsWith('='))
        {
            return queryable.Where(BuildEqualsPredicate(propertyExpression, match[1..]));
        }

        bool hasWildcard = match.Contains('*') || match.Contains('?');

        if (hasWildcard)
        {
            match = match.Replace('*', '%').Replace('?', '_');
        }
        else
        {
            match = $"%{match}%";
        }

        return queryable.Where(BuildLikePredicate(propertyExpression, match));
    }

    private static Expression<Func<Cccf, bool>> BuildAnyEqualsPredicate(Expression<Func<Cccf, IEnumerable<string>>> propertyExpression, string match)
    {
        var parameter = propertyExpression.Parameters[0]; // x
        var member = propertyExpression.Body; // x.SomeCollection

        // x.SomeCollection.Any(y => y == match)
        var elementParam = Expression.Parameter(typeof(string), "y");
        var equalsExpression = Expression.Equal(elementParam, Expression.Constant(match));
        var anyCall = Expression.Call(AnyMethod, member, Expression.Lambda(equalsExpression, elementParam));

        return Expression.Lambda<Func<Cccf, bool>>(anyCall, parameter);
    }

    private static Expression<Func<Cccf, bool>> BuildAnyLikePredicate(Expression<Func<Cccf, IEnumerable<string>>> propertyExpression, string match)
    {
        var parameter = propertyExpression.Parameters[0]; // x
        var member = propertyExpression.Body; // x.SomeCollection

        // x.SomeCollection.Any(y => EF.Functions.Like(y, match, @"\"))
        var elementParam = Expression.Parameter(typeof(string), "y");

        var likeExpression = Expression.Call(
            LikeMethod,
            DbFunctionsArg,
            elementParam,
            Expression.Constant(match),
            Expression.Constant(@"\"));

        var anyCall = Expression.Call(AnyMethod, member, Expression.Lambda(likeExpression, elementParam));

        return Expression.Lambda<Func<Cccf, bool>>(anyCall, parameter);
    }

    private static Expression<Func<Cccf, bool>> BuildEqualsPredicate(Expression<Func<Cccf, string>> propertyExpression, string match)
    {
        var parameter = propertyExpression.Parameters[0]; // 获取表达式中的参数 (x)
        var member = propertyExpression.Body; // 获取属性表达式
        var matchConstant = Expression.Constant(match); // 创建匹配值的常量表达式

        // // 构建 == 的调用表达式
        var equalsExpression = Expression.Equal(member, matchConstant);

        return Expression.Lambda<Func<Cccf, bool>>(equalsExpression, parameter);
    }

    private static Expression<Func<Cccf, bool>> BuildLikePredicate(Expression<Func<Cccf, string>> propertyExpression, string match)
    {
        var parameter = propertyExpression.Parameters[0]; // 获取表达式中的参数 (x)
        var member = (MemberExpression)propertyExpression.Body; // 获取属性表达式

        // 构建 EF.Functions.Like 的调用表达式
        var likeExpression = Expression.Call(
            LikeMethod!,
            DbFunctionsArg,
            member,
            Expression.Constant(match),
            Expression.Constant(@"\"));

        return Expression.Lambda<Func<Cccf, bool>>(likeExpression, parameter);
    }
}

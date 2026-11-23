using System.Linq.Expressions;

namespace Tawzeef.Domain.Common;

/// <summary>Combines two `Expression&lt;Func&lt;T, bool&gt;&gt;` into one flat expression tree
/// (rebinding the second's parameter onto the first's) instead of nesting an `Expression.Invoke` —
/// EF Core can translate the result to SQL the same way it already translates a single hand-written
/// `&amp;&amp;`/`||` lambda; `Invoke` generally can't be translated. Used by
/// PublicJobSearchService's keyword search to OR together a dynamic, synonym-expanded list of
/// `Title.Contains(term)` checks built at runtime, where the number of terms isn't known until the
/// search request arrives.</summary>
public static class PredicateCombinator
{
    public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        => Combine(left, right, Expression.AndAlso);

    public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        => Combine(left, right, Expression.OrElse);

    private static Expression<Func<T, bool>> Combine<T>(
        Expression<Func<T, bool>> left, Expression<Func<T, bool>> right, Func<Expression, Expression, Expression> merge)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftBody = new ReplaceParameterVisitor(left.Parameters[0], parameter).Visit(left.Body);
        var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(merge(leftBody, rightBody), parameter);
    }

    private class ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParameter ? newParameter : base.VisitParameter(node);
    }
}

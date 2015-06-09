using System;
using System.Linq.Expressions;

namespace Axe.ExpressionBuilders
{
    public interface IExpressionBuilder
    {
        Expression<Func<TEntity, TEntity>> BuildExpression<TEntity>(FieldRing fieldRing, AxeProfile profile)
            where TEntity : class, new();
    }
}

using System.Linq.Expressions;

namespace Bookings.Domain.Common;

public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);

    public static implicit operator Expression<Func<T, bool>>(Specification<T> spec) =>
        spec.ToExpression();
}

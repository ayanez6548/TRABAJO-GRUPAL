using System.Linq.Expressions;
namespace NorthWind.Entities.Abstractions;

public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ConditionExpression { get; }
    public bool IsSatisfiedBy(T entityInstance) => ConditionExpression.Compile()(entityInstance);
}

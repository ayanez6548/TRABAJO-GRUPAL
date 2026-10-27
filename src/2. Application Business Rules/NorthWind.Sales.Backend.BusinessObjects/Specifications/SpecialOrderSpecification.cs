using System.Linq.Expressions;
using NorthWind.Entities.Abstractions;
using NorthWind.Sales.Backend.BusinessObjects.Aggregates;
namespace NorthWind.Sales.Backend.BusinessObjects.Specifications;
public sealed class SpecialOrderSpecification : Specification<OrderAggregate>
{
    public override Expression<Func<OrderAggregate, bool>> ConditionExpression => order => order.OrderDetails.Count > 3;
}

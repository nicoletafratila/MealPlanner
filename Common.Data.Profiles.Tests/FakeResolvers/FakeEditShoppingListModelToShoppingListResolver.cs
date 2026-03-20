using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Tests.FakeResolvers
{
    public class FakeEditShoppingListModelToShoppingListResolver
        : IMemberValueResolver<
            ShoppingListEditModel,
            ShoppingList,
            IList<ShoppingListProductEditModel>?,
            IList<ShoppingListProduct>>
    {
        public bool WasCalled { get; private set; }
        public IList<ShoppingListProduct> ReturnedValue { get; set; } = [];

        public IList<ShoppingListProduct> Resolve(
            ShoppingListEditModel source,
            ShoppingList destination,
            IList<ShoppingListProductEditModel>? sourceValue,
            IList<ShoppingListProduct>? destValue,
            ResolutionContext context)
        {
            WasCalled = true;
            return ReturnedValue;
        }
    }
}
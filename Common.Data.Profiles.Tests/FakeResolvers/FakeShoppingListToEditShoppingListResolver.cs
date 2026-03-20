using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Tests.FakeResolvers
{
    public class FakeShoppingListToEditShoppingListResolver
        : IMemberValueResolver<
            ShoppingList,
            ShoppingListEditModel,
            IList<ShoppingListProduct>?,
            IList<ShoppingListProductEditModel>>
    {
        public bool WasCalled { get; private set; }
        public IList<ShoppingListProductEditModel> ReturnedValue { get; set; } = [];

        public IList<ShoppingListProductEditModel> Resolve(
            ShoppingList source,
            ShoppingListEditModel destination,
            IList<ShoppingListProduct>? sourceValue,
            IList<ShoppingListProductEditModel>? destValue,
            ResolutionContext context)
        {
            WasCalled = true;
            return ReturnedValue;
        }
    }
}
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL
{
    public class PhoneType :
        ObjectGraphType<Phone>
    {
        public PhoneType()
        {
            Name = "phone";
            Description = "A phone number.";

            Field<IntGraphType>("id")
                .Description("A unique identifier for the phone number.")
                .Resolve(context => context.Source?.Id);

            Field<StringGraphType>("number")
                .Description("The phone number.")
                .Resolve(context => context.Source?.Number);

            Field<PhoneEnumType>("type")
                .Description("The type of phone number.  One of 'home', 'work', 'mobile', or 'other'.")
                .Resolve(context => context.Source?.Type);
        }
    }
}
using System;
using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.QueryBuilders
{
    public class CompanyQueryBuilder :
        IQueryBuilder<Company>
    {
        private readonly IQueryBuilder<Email> _emailQueryBuilder;
        private readonly IQueryBuilder<Phone> _phoneQueryBuilder;

        public CompanyQueryBuilder(
            IQueryBuilder<Email> emailQueryBuilder,
            IQueryBuilder<Phone> phoneQueryBuilder)
        {
            _emailQueryBuilder = emailQueryBuilder;
            _phoneQueryBuilder = phoneQueryBuilder;
        }

        public SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode context, string alias)
        {
            query.Select($"{alias}.Id");
            query.SplitOn<Company>("Id");

            var fields = context.GetSelectedFields();

            if (fields?.Keys.Any(k => k.StringValue.Equals("name", StringComparison.OrdinalIgnoreCase)) ?? false)
            {
                query.Select($"{alias}.Name");
            }

            var emailsKey =
                fields?.Keys.FirstOrDefault(k => k.StringValue.Equals("emails", StringComparison.OrdinalIgnoreCase));
            if (emailsKey != (GraphQLName?)null)
            {
                var companyEmailAlias = $"{alias}CompanyEmail";
                var emailAlias = $"{alias}Email";
                query
                    .LeftJoin($"CompanyEmail {companyEmailAlias} ON {alias}.Id = {companyEmailAlias}.PersonId")
                    .LeftJoin($"Email {emailAlias} ON {companyEmailAlias}.EmailId = {emailAlias}.Id");
                query = _emailQueryBuilder.Build(query, fields![emailsKey], emailAlias);
            }

            var phonesKey =
                fields?.Keys.FirstOrDefault(k => k.StringValue.Equals("phones", StringComparison.OrdinalIgnoreCase));
            if (phonesKey != (GraphQLName?)null)
            {
                var companyPhoneAlias = $"{alias}CompanyPhone";
                var phoneAlias = $"{alias}Phone";
                query
                    .LeftJoin($"CompanyPhone {companyPhoneAlias} ON {alias}.Id = {companyPhoneAlias}.PersonId")
                    .LeftJoin($"Phone {phoneAlias} ON {companyPhoneAlias}.PhoneId = {phoneAlias}.Id");
                query = _phoneQueryBuilder.Build(query, fields![phonesKey], phoneAlias);
            }

            return query;
        }
    }
}

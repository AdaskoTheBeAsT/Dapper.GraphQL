using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest
{
    public class UpdateTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public UpdateTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "UPDATE person succeeds")]
        public void UpdatePerson()
        {
            var person = new Person
            {
                FirstName = "Douglas"
            };
            Person previousPerson = null;

            try
            {
                var graphql = @"
{
    person {
        id
        firstName
    }
}";

                var selectionSet = _fixture.BuildGraphQlSelection(graphql);

                // Update the person with Id = 2 with a new FirstName
                using (var db = _fixture.GetDbConnection())
                {
                    previousPerson = AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                        .From<Person>()
                        .Select("Id", "FirstName")
                        .Where("FirstName = @firstName", new { firstName = "Doug" })
                        .Execute<Person>(db, selectionSet)
                        .FirstOrDefault();

                    AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                        .Update(person)
                        .Where("Id = @id", new { id = previousPerson.Id })
                        .Execute(db);

                    // Get the same person back
                    person = AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                        .From<Person>()
                        .Select("Id", "FirstName")
                        .Where("Id = @id", new { id = previousPerson.Id })
                        .Execute<Person>(db, selectionSet)
                        .FirstOrDefault();
                }

                // Ensure we got a person and their name was indeed changed
                Assert.NotNull(person);
                Assert.Equal("Douglas", person.FirstName);
            }
            finally
            {
                if (previousPerson != null)
                {
                    using (var db = _fixture.GetDbConnection())
                    {
                        person = new Person
                        {
                            FirstName = previousPerson.FirstName
                        };

                        // Put the entity back to the way it was
                        AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                            .Update<Person>(person)
                            .Where("Id = @id", new { id = 2 })
                            .Execute(db);
                    }
                }
            }
        }

        [Fact(DisplayName = "UPDATE person asynchronously succeeds")]
        public async Task UpdatePersonAsync()
        {
            var person = new Person
            {
                FirstName = "Douglas"
            };
            Person previousPerson = null;

            try
            {
                // Update the person with Id = 2 with a new FirstName
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    var graphql = @"
{
    person {
        id
        firstName
    }
}";

                    var selectionSet = _fixture.BuildGraphQlSelection(graphql);

                    var previousPeople = await AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                        .From<Person>()
                        .Select("Id", "FirstName")
                        .Where("FirstName = @firstName", new { firstName = "Doug" })
                        .ExecuteAsync<Person>(db, selectionSet);

                    previousPerson = previousPeople.FirstOrDefault();

                    await AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                        .Update(person)
                        .Where("Id = @id", new { id = previousPerson.Id })
                        .ExecuteAsync(db);

                    // Get the same person back
                    var people = await AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                        .From<Person>()
                        .Select("Id", "FirstName")
                        .Where("Id = @id", new { id = previousPerson.Id })
                        .ExecuteAsync<Person>(db, selectionSet);
                    person = people
                        .FirstOrDefault();
                }

                // Ensure we got a person and their name was indeed changed
                Assert.NotNull(person);
                Assert.Equal("Douglas", person.FirstName);
            }
            finally
            {
                if (previousPerson != null)
                {
                    using (var db = _fixture.GetDbConnection())
                    {
                        db.Open();

                        person = new Person
                        {
                            FirstName = previousPerson.FirstName
                        };

                        // Put the entity back to the way it was
                        await AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                            .Update<Person>(person)
                            .Where("Id = @id", new { id = 2 })
                            .ExecuteAsync(db);
                    }
                }
            }
        }
    }
}
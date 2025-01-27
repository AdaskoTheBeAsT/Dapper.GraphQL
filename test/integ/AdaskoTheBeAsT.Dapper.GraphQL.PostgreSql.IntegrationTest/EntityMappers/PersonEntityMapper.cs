using System;
using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.EntityMappers
{
    public class PersonEntityMapper :
        DeduplicatingEntityMapper<Person>
    {
        private CompanyEntityMapper? _companyEntityMapper;
        private PersonEntityMapper? _personEntityMapper;

        public PersonEntityMapper()
        {
            // Deduplicate entities using MergedToPersonId instead of Id.
            PrimaryKey = p => p.MergedToPersonId;
        }

        public override Person? Map(EntityMapContext context)
        {
            // Avoid creating the mappers until they're used
            // NOTE: this avoids an infinite loop (had these been created in the ctor)
            if (_companyEntityMapper == null)
            {
                _companyEntityMapper = new CompanyEntityMapper();
            }

            if (_personEntityMapper == null)
            {
                _personEntityMapper = new PersonEntityMapper();
            }

            // NOTE: Order is very important here.  We must map the objects in
            // the same order they were queried in the QueryBuilder.

            // Start with the person, and deduplicate
            var person = Deduplicate(context.Start<Person>());
            var company = context.Next<Company>("companies", _companyEntityMapper);
            var email = context.Next<Email>("emails");
            var phone = context.Next<Phone>("phones");
            var supervisor = context.Next<Person>("supervisor", _personEntityMapper);
            var careerCounselor = context.Next<Person>("careerCounselor", _personEntityMapper);

            if (person != null)
            {
                if (company != null &&

                    // Eliminate duplicates
                    !person.Companies.Any(c => c.Id == company.Id))
                {
                    person.Companies.Add(company);
                }

                if (email != null &&

                    // Eliminate duplicates
                    !person.Emails.Any(e => string.Equals(e.Address, email.Address, StringComparison.OrdinalIgnoreCase)))
                {
                    person.Emails.Add(email);
                }

                if (phone != null &&

                    // Eliminate duplicates
                    !person.Phones.Any(p => string.Equals(p.Number, phone.Number, StringComparison.OrdinalIgnoreCase)))
                {
                    person.Phones.Add(phone);
                }

                person.Supervisor = person.Supervisor ?? supervisor;
                person.CareerCounselor = person.CareerCounselor ?? careerCounselor;
            }

            return person;
        }
    }
}

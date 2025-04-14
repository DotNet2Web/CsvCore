using System;
using Bogus;

namespace CsvCore.Specs.Extensions;

public static class BogusPersonExtensions
{
    public static DateOnly RandomDateOfBirth(this Person person)
    {
        return new DateOnly(person.DateOfBirth.Year, person.DateOfBirth.Month, person.DateOfBirth.Day);
    }
}

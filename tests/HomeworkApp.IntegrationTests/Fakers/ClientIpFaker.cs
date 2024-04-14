using AutoBogus;
using Bogus;

namespace HomeworkApp.IntegrationTests.Fakers;

public static class ClientIpFaker
{
    private static readonly object Lock = new();

    private static readonly Faker<string> Faker = new AutoFaker<string>();

    public static IEnumerable<string> Generate(int count = 1)
    {
        lock (Lock)
        {
            return Faker.Generate(count).ToArray();
        }
    }
}
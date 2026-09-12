namespace Services.Helpers
{
  internal class TinGeneratorHelper
  {
    public static string GenerateTIN()
    {
      int number = Random.Shared.Next(0, 1_000_000);
      return $"TIN-{number:D6}";
    }
  }
}

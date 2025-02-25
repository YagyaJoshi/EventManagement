namespace EventManagement.BusinessLogic.Helpers
{
    public static class CurrencyHelper
    {
        public static decimal CalculateDefaultCurrencyAmount(decimal totalAmountInDisplayCurrency, decimal displayCurrencyRate)
        {
            return Math.Round(totalAmountInDisplayCurrency * displayCurrencyRate, 2);
        }
    }
}

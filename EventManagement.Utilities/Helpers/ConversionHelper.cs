namespace EventManagement.Utilities.Helpers
{
    public class ConversionHelper
    {
        public static List<int> ConvertStringToArray(string accessibilityInfo)
        {
            if (string.IsNullOrEmpty(accessibilityInfo))
            {
                return new List<int>();
            }

            // Remove brackets and split by comma
            string[] parts = accessibilityInfo.Trim('[', ']').Split(',');

            // Convert each part to integer and return as a list
            return parts.Select(part => int.Parse(part.Trim())).ToList();
        }

        public static List<string> ConvertStringToList(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<string>();
            }

            // Remove brackets and split by comma
            string[] parts = input.Trim('[', ']').Split(',');

            // Remove single quotes and whitespace from each part
            return parts.Select(part => part.Trim('\'', ' ')).ToList();
        }
    }
}

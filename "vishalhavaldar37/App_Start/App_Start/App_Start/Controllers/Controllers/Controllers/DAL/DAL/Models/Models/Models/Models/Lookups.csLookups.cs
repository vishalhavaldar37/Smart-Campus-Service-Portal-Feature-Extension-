namespace SmartCampusPortal.Models
{
    public static class Lookups
    {
        public static readonly string[] ServiceTypes = { "IT Support", "Library", "Maintenance", "Transport" };
        public static readonly string[] Priorities = { "Low", "Medium", "High" };
        public static readonly string[] Statuses = { "Pending", "In Progress", "Completed" };

        /// <summary>Returns the matching canonical value (case-insensitive) or null.</summary>
        public static string Match(string[] list, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            foreach (string item in list)
            {
                if (string.Equals(item, value.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }
    }
}

namespace MediScope.Common.Models.Enums
{
    public static class BloodGroup
    {
        public const string APositive = "A+";
        public const string ANegative = "A-";
        public const string BPositive = "B+";
        public const string BNegative = "B-";
        public const string ABPositive = "AB+";
        public const string ABNegative = "AB-";
        public const string OPositive = "O+";
        public const string ONegative = "O-";

        public static readonly IReadOnlyList<string> All = new[]
        {
            APositive, ANegative, BPositive, BNegative,
            ABPositive, ABNegative, OPositive, ONegative
        };

        public static bool IsValid(string? value) => All.Contains(value);
    }
}
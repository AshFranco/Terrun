namespace Terrun.Engine.Tracks;

public sealed record RuleViolation(string Rule, double Value, double Limit);

// LLD §4.1 step 3 (CTZ-31a): an activity faster than the minimum pace, or with too much of its distance in invalid legs,
// goes to human review instead of adding territory (MT-04). The rule names are stable: they are stored in
// activity_review.violated_rules and shown to moderators
public static class AntiCheatJudge
{
    public const string PaceTooFast = "min_pace";
    public const string TooMuchInvalidDistance = "invalid_distance";

    public static IReadOnlyList<RuleViolation> Judge(CleaningResult result, CleaningRules rules)
    {
        var violations = new List<RuleViolation>();
        if (result.AvgPaceSPerKm is { } pace && pace < rules.MinPaceSPerKm)
        {
            violations.Add(new RuleViolation(PaceTooFast, Math.Round(pace, 1), rules.MinPaceSPerKm));
        }

        if (result.InvalidDistancePct > rules.MaxInvalidDistancePct)
        {
            violations.Add(new RuleViolation(TooMuchInvalidDistance, Math.Round(result.InvalidDistancePct, 1), rules.MaxInvalidDistancePct));
        }

        return violations;
    }
}

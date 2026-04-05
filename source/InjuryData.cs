namespace InjurySystem;

public enum BodyPart
{
    Head,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
    Torso
}

public enum Severity
{
    Healthy = 0,
    Minor = 1,
    Severe = 2
}

public class InjuryState
{
    public Severity Head { get; set; } = Severity.Healthy;
    public Severity LeftArm { get; set; } = Severity.Healthy;
    public Severity RightArm { get; set; } = Severity.Healthy;
    public Severity LeftLeg { get; set; } = Severity.Healthy;
    public Severity RightLeg { get; set; } = Severity.Healthy;
    public Severity Torso { get; set; } = Severity.Healthy;

    public Severity GetSeverity(BodyPart part) => part switch
    {
        BodyPart.Head => Head,
        BodyPart.LeftArm => LeftArm,
        BodyPart.RightArm => RightArm,
        BodyPart.LeftLeg => LeftLeg,
        BodyPart.RightLeg => RightLeg,
        BodyPart.Torso => Torso,
        _ => Severity.Healthy
    };

    public void SetSeverity(BodyPart part, Severity severity)
    {
        switch (part)
        {
            case BodyPart.Head: Head = severity; break;
            case BodyPart.LeftArm: LeftArm = severity; break;
            case BodyPart.RightArm: RightArm = severity; break;
            case BodyPart.LeftLeg: LeftLeg = severity; break;
            case BodyPart.RightLeg: RightLeg = severity; break;
            case BodyPart.Torso: Torso = severity; break;
        }
    }

    public void HealAll()
    {
        Head = Severity.Healthy;
        LeftArm = Severity.Healthy;
        RightArm = Severity.Healthy;
        LeftLeg = Severity.Healthy;
        RightLeg = Severity.Healthy;
        Torso = Severity.Healthy;
    }

    public void HealPart(BodyPart part)
    {
        SetSeverity(part, Severity.Healthy);
    }

    public bool HasAnyInjury()
    {
        return Head != Severity.Healthy
            || LeftArm != Severity.Healthy
            || RightArm != Severity.Healthy
            || LeftLeg != Severity.Healthy
            || RightLeg != Severity.Healthy
            || Torso != Severity.Healthy;
    }

    public Severity GetWorstLegInjury()
    {
        return (Severity)System.Math.Max((int)LeftLeg, (int)RightLeg);
    }

    public Severity GetWorstArmInjury()
    {
        return (Severity)System.Math.Max((int)LeftArm, (int)RightArm);
    }
}

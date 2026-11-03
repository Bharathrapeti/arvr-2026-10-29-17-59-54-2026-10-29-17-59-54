namespace AREscapeRoom.Core
{
    public enum DoorRequirement
    {
        None = 0,

        // Existing project value
        AutoProximity = 1,

        // Standard names used by the current scripts
        RequiresKey = 2,
        RequiresPassword = 3,
        RequiresGems = 4,
        RequiresColorSequence = 5,
        RequiresShapes = 6,
        RequiresFinalValidation = 7,

        // Backward-compatible aliases
        Key = RequiresKey,
        Password = RequiresPassword,
        AllGems = RequiresGems,
        ColorSequence = RequiresColorSequence,
        AllShapes = RequiresShapes,
        FinalVault = RequiresFinalValidation
    }
}
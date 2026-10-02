public enum PrimaryItems
{
    // No item
    None,

    // Items
    RocketLauncher,
    GrapplingHook,
    RedirectionOrb,
}

public static class PrimaryItemUtility
{
    public static bool ItemLocksPlayerCamera(PrimaryItems item)
    {
        switch (item) {
            case PrimaryItems.None:
                return false;
            case PrimaryItems.RocketLauncher:
                return true;
            case PrimaryItems.GrapplingHook:
                return true;
            case PrimaryItems.RedirectionOrb:
                return true;

            default:
                return false;
        }
    }
}

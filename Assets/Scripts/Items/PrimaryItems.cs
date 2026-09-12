public enum PrimaryItems
{
    // No item
    None,

    // Items
    RocketLauncher,
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

            default:
                return false;
        }
    }
}

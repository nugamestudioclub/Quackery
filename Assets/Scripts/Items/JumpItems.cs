public enum JumpItems
{
    // No item
    None,

    // Items
    PropellerHat,
}

public static class JumpItemUtility
{
    public static bool ItemLocksPlayerCamera(JumpItems item)
    {
        switch (item) {
            case JumpItems.None:
                return false;
            case JumpItems.PropellerHat:
                return false;

            default:
                return false;
        }
    }
}

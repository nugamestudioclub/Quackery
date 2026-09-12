public enum SecondaryItems
{
    // No item
    None,

    // Items
    // Nothing yet, add items here...
}

public static class SecondaryItemUtility
{
    public static bool ItemLocksPlayerCamera(SecondaryItems item)
    {
        switch (item) {
            case SecondaryItems.None:
                return false;

            default:
                return false;
        }
    }
}

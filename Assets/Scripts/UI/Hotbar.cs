using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    private PlayerItemController playerItems;

    [SerializeField] private ItemMugshotDatabase mugshotsDB;

    [SerializeField] private Transform[] primarySlotsSel;
    [SerializeField] private Transform[] primarySlotsDesel;
    [SerializeField] private Transform[] primarySlotsItemImage;
    [SerializeField] private Transform jumpSlotSel;
    [SerializeField] private Transform jumpSlotDesel;
    [SerializeField] private Transform jumpSlotItemImage;
    int lastActiveHotbarSlot = -1;
    bool hadJumpItem = false;

    void Start()
    {
        playerItems = FindFirstObjectByType<PlayerItemController>();

        foreach (Transform slot in primarySlotsSel) {
            slot.gameObject.SetActive(false);
        }
        foreach (Transform slot in primarySlotsItemImage) {
            slot.gameObject.SetActive(false);
        }
        jumpSlotSel.gameObject.SetActive(false);
        jumpSlotItemImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (
            playerItems == null ||
            (primarySlotsSel.Length != primarySlotsDesel.Length) ||
            (primarySlotsSel.Length != primarySlotsItemImage.Length)
        ) {
            // Exit early if fields are messed up
            return;
        }

        int hotbarSlotActive = playerItems.GetActiveHotbarSlot();
        if (hotbarSlotActive != lastActiveHotbarSlot) {
            if (lastActiveHotbarSlot > 0 && lastActiveHotbarSlot <= primarySlotsSel.Length) {
                primarySlotsSel[lastActiveHotbarSlot - 1].gameObject.SetActive(false);
                primarySlotsDesel[lastActiveHotbarSlot - 1].gameObject.SetActive(true);
            }
            lastActiveHotbarSlot = hotbarSlotActive;

            if (hotbarSlotActive > 0 && hotbarSlotActive <= primarySlotsSel.Length) {
                primarySlotsSel[lastActiveHotbarSlot - 1].gameObject.SetActive(true);
                primarySlotsDesel[lastActiveHotbarSlot - 1].gameObject.SetActive(false);
            }
        }

        PrimaryItems[] primaryItems = playerItems.GetPrimaryItems();
        int idx = 0;
        foreach (PrimaryItems item in primaryItems) {
            if (item != PrimaryItems.None) {
                primarySlotsItemImage[idx].gameObject.SetActive(true);
                Sprite setSprite = null;
                switch (item) {
                    case PrimaryItems.RocketLauncher:
                        setSprite = mugshotsDB.rocketLauncher;
                        break;
                    case PrimaryItems.GrapplingHook:
                        setSprite = mugshotsDB.grapplingHook;
                        break;
                    default:
                        break;
                }
                if (setSprite != null) {
                    primarySlotsItemImage[idx].GetComponent<Image>().sprite = setSprite;
                }
            }
            else {
                primarySlotsItemImage[idx].gameObject.SetActive(false);
            }
            idx++;
        }


        bool nowHasJumpItem = playerItems.HasJumpItem();
        if (hadJumpItem != nowHasJumpItem) {
            hadJumpItem = nowHasJumpItem;
            if (nowHasJumpItem) {
                jumpSlotSel.gameObject.SetActive(true);
                jumpSlotDesel.gameObject.SetActive(false);
                jumpSlotItemImage.gameObject.SetActive(true);
                Sprite setSprite = null;
                switch (playerItems.GetJumpItem()) {
                    case JumpItems.PropellerHat:
                        setSprite = mugshotsDB.propellerHat;
                        break;
                    default:
                        break;
                }
                if (setSprite != null) {
                    jumpSlotItemImage.GetComponent<Image>().sprite = setSprite;
                }
            }
            else {
                jumpSlotSel.gameObject.SetActive(false);
                jumpSlotDesel.gameObject.SetActive(true);
                jumpSlotItemImage.gameObject.SetActive(false);
            }
        }
    }
}

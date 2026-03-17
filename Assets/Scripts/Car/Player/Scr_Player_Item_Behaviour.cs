using UnityEngine;

public class Scr_Player_Item_Behaviour : MonoBehaviour
{
    // reference item handler so we know what item we are holding
    private Scr_Item_Handler scr_ItemHandler;

    // what item are we holding
    private string itemHeld;

    // laser fire toggle
    private bool fireLaserBurst = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get the item handler component
        scr_ItemHandler = GetComponent<Scr_Item_Handler>();

        

        // get item held
        itemHeld = scr_ItemHandler.getItemHeld();

    }

    // Update is called once per frame
    void Update()
    {
        // update item held
        itemHeld = scr_ItemHandler.getItemHeld();

        // if this is true, fire laser, skip rest of the if statement
        if (fireLaserBurst == true)
        {
            scr_ItemHandler.UseItemLaser();
            return;

        }

        // if we press control key, use our held item
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // DEBUG LOG, track if we are pressing left control key
            Debug.Log("Left Control Key Pressed! Attempting to use item: " + itemHeld);

            if (itemHeld == "Nitro") 
            {
                scr_ItemHandler.UseItemNitro();
            }

            if (itemHeld == "Health Pack") 
            {
                scr_ItemHandler.UseItemHealthPack();
            }

            if (itemHeld == "Rocket") 
            {
                scr_ItemHandler.UseItemRocket();
            }

            if (itemHeld == "Missile") 
            {
                scr_ItemHandler.UseItemMissile();
            }

            if (itemHeld == "Laser") 
            {
                
                // toggle bool for laser fire, as its a 3 round burst
                fireLaserBurst = true;

                scr_ItemHandler.UseItemLaser();
            }

            if (itemHeld == "Flamethrower") 
            {
                scr_ItemHandler.UseItemFlamethrower();
            }

            if (itemHeld == "Shield") 
            { 
                scr_ItemHandler.UseItemShield();
            }

            if (itemHeld == "Ghosts") 
            {
                scr_ItemHandler.UseItemGhosts();
            }

            if (itemHeld == "Ion Beam") 
            {
                scr_ItemHandler.UseItemIonBeam();
            }
        }
    }

    public bool setFireLaserBurstToggle(bool laserFireToggle)
    {
        fireLaserBurst = laserFireToggle;
        return fireLaserBurst;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework.Constraints;

public class Scr_UI_Position_Displayer : MonoBehaviour
{
    // racer position display text
    [SerializeField] private TextMeshProUGUI racerPosition;

    // racer speed display text
    [SerializeField] private TextMeshProUGUI racerSpeed;

    // racer Item display text
    [SerializeField] private TextMeshProUGUI racerItem;

    // racer health display text
    [SerializeField] private TextMeshProUGUI racerHealth;

    // track Object position
    private int position;

    // track object speed
    private float speed;

    // track object item
    private string itemHeld;

    // track car health
    private int carHealth;

    // car max health
    private int carMaxHealth;

    // conversion ratio
    [SerializeField] private float conversionRatio;

    // object racer to keep track of
    [SerializeField] private GameObject Racer;

    // reference the race manager game object
    [SerializeField] private GameObject raceCheckpointManager;

    // reference the race checkpoint manager script to get our position
    private scr_RaceCheckpoints scr_RaceCheckpoints;

    // my race progress script to see if our racer has completed the race
    private scr_My_Race_Progress scr_MyRaceProgress;

    // item handler script to see what item our racer currently has
    private Scr_Item_Handler scr_MyItemHandler;

    // car health handler script to see what health our racer currently has
    private Scr_Car_Health scr_MyCarHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get components
        scr_RaceCheckpoints = raceCheckpointManager.GetComponent<scr_RaceCheckpoints>();
        scr_MyRaceProgress = Racer.GetComponent<scr_My_Race_Progress>();
        scr_MyItemHandler = Racer.GetComponent<Scr_Item_Handler>();
        scr_MyCarHealth = Racer.GetComponent<Scr_Car_Health>();

        speed = 0;

        // check if list is empty
        if (scr_RaceCheckpoints.Racers.Count > 0) {

            // get reference object 
            position = scr_RaceCheckpoints.GetRacerPosition(Racer) + 1;

            // record speed
            speed = Vector3.Magnitude(Racer.GetComponent<Rigidbody>().linearVelocity);

            // record item held
            itemHeld = scr_MyItemHandler.getItemHeld();

            // get car health
            carHealth = scr_MyCarHealth.GetCurrentHealth();

            // get max car health
            carMaxHealth = scr_MyCarHealth.GetMaxHealth();

            // update text to read the position / list length
            racerPosition.text = "Pos: " + position.ToString() + scr_RaceCheckpoints.Racers.Count.ToString();

            racerSpeed.text = "Speed: " + ((int)(speed * conversionRatio)).ToString() + " Km/hr";

            racerItem.text = "Item: " + itemHeld;

            racerHealth.text = "Health: " + carHealth.ToString();
        }

        

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // check if list is empty
        if (scr_RaceCheckpoints.Racers.Count > 0)
        {
            
            // check if we completed the race or not
            if (scr_MyRaceProgress.completedRace)
            {
                // race has been complete

                // get reference object - use the racer completion list
                position = scr_RaceCheckpoints.GetRacerCompletionPosition(Racer);

                // update text to read the position / list length
                racerPosition.text = "Pos: " + position.ToString() + " / " + scr_RaceCheckpoints.Racers.Count.ToString();
            }
            else 
            {
                
                // get reference object - use the racers list
                position = scr_RaceCheckpoints.GetRacerPosition(Racer);

                // update text to read the position / list length
                racerPosition.text = "Pos: " + position.ToString() + " / " + scr_RaceCheckpoints.Racers.Count.ToString();

            }

            // speed display
            speed = Vector3.Magnitude(Racer.GetComponent<Rigidbody>().linearVelocity);

            racerSpeed.text = "Speed: " + ((int)(speed * conversionRatio)).ToString() + " Km/hr";

            // item display
            itemHeld = scr_MyItemHandler.getItemHeld();

            racerItem.text = "Item: " + itemHeld;

            // health display
            carHealth = scr_MyCarHealth.GetCurrentHealth();

            racerHealth.text = "Health: " + carHealth.ToString();

        }


    }


}

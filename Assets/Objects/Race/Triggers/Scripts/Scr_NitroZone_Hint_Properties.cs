using UnityEngine;

public class Scr_NitroZone_Hint_Properties : MonoBehaviour
{
    // required checkpoint target to consider this nitro hint zone for AI
    [SerializeField] private int aiNitroTargetCheckpointRequirment;

    // get checkpoint target requirement for AI
    public int GetAINitroTargetCheckpointRequirment()
    {
        return aiNitroTargetCheckpointRequirment;
    }
}

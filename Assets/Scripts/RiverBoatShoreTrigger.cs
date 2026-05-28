using UnityEngine;

public class RiverBoatShoreTrigger : MonoBehaviour
{
    public RiverBoatGameController controller;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<RiverBoatShoreMarker>() == null)
        {
            return;
        }

        if (controller != null)
        {
            controller.ShoreTouched();
        }
    }
}

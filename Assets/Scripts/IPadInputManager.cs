using UnityEngine;

public class IPadInputManager : MonoBehaviour
{
    public ArduinoSerialInput arduinoInput;

    public void PressTile(int tileID)
    {
        if (arduinoInput == null)
        {
            arduinoInput = FindObjectOfType<ArduinoSerialInput>();
        }
        Debug.Log("iPad tile pressed: " + tileID);

        switch (tileID)
        {
            case 0:
                arduinoInput.SendMessage("HandleIPadInput", "a");
                break;
            case 1:
                arduinoInput.SendMessage("HandleIPadInput", "b");
                break;
            case 2:
                arduinoInput.SendMessage("HandleIPadInput", "c");
                break;
            case 3:
                arduinoInput.SendMessage("HandleIPadInput", "d");
                break;
            case 4:
                arduinoInput.SendMessage("HandleIPadInput", "e");
                break;
            case 5:
                arduinoInput.SendMessage("HandleIPadInput", "f");
                break;
            case 6:
                arduinoInput.SendMessage("HandleIPadInput", "g");
                break;
            case 7:
                arduinoInput.SendMessage("HandleIPadInput", "h");
                break;
            case 8:
                arduinoInput.SendMessage("HandleIPadInput", "i");
                break;
            case 9:
                arduinoInput.SendMessage("HandleIPadInput", "j");
                break;
            case 10:
                arduinoInput.SendMessage("HandleIPadInput", "k");
                break;
            case 11:
                arduinoInput.SendMessage("HandleIPadInput", "l");
                break;
            case 12:
                arduinoInput.SendMessage("HandleIPadInput", "m");
                break;
            case 13:
                arduinoInput.SendMessage("HandleIPadInput", "n");
                break;
        }
    }
}
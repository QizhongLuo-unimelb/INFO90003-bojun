using UnityEngine;

public class IPadInputManager : MonoBehaviour
{
    public static IPadInputManager Instance { get; private set; }

    public ArduinoSerialManager arduinoInput;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PressTile(int tileID)
    {
        Debug.Log("iPad tile pressed: " + tileID);

        if (tileID < 0 || tileID > 13)
        {
            Debug.LogWarning("Unknown iPad tile: " + tileID);
            return;
        }

        EnsureArduinoInput();

        if (arduinoInput == null)
        {
            Debug.LogWarning("No ArduinoSerialManager found for iPad input.");
            return;
        }

        char inputLetter = (char)('a' + tileID);
        arduinoInput.HandleIPadInput(inputLetter.ToString());
    }

    void EnsureArduinoInput()
    {
        if (arduinoInput == null)
        {
            arduinoInput = ArduinoSerialManager.Instance;
        }

        if (arduinoInput == null)
        {
            arduinoInput = FindFirstObjectByType<ArduinoSerialManager>();
        }

        if (arduinoInput == null)
        {
            GameObject managerObject = new GameObject("ArduinoSerialManager");
            arduinoInput = managerObject.AddComponent<ArduinoSerialManager>();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

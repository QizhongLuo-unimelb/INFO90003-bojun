using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine.SceneManagement;

public class ArduinoSerialManager : MonoBehaviour
{
    public static ArduinoSerialManager Instance { get; private set; }

    [Header("macOS Serial Settings")]
    public string portName = "/dev/cu.usbmodem101";
    public int baudRate = 9600;

    [Header("Movement Target")]
    public StepNodePlayerController player;

    private SerialPort serialPort;
    private Thread serialThread;
    private bool isRunning;

    private readonly object lockObject = new object();
    private readonly Queue<string> pendingInputs = new Queue<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate ArduinoSerialManager found. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (Instance != this)
        {
            return;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<StepNodePlayerController>();
        }

        OpenSerialPort();
    }

    void Update()
    {
        while (TryDequeueInput(out string input))
        {
            HandleSerialInput(input);
        }
    }

    bool TryDequeueInput(out string input)
    {
        lock (lockObject)
        {
            if (pendingInputs.Count > 0)
            {
                input = pendingInputs.Dequeue();
                return true;
            }
        }

        input = "";
        return false;
    }

    void OpenSerialPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            return;
        }

        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 100;
            serialPort.NewLine = "\n";
            serialPort.Open();

            isRunning = true;
            serialThread = new Thread(ReadSerialData);
            serialThread.IsBackground = true;
            serialThread.Start();

            Debug.Log("Arduino connected on " + portName);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to open serial port " + portName + ": " + e.Message);
        }
    }

    void ReadSerialData()
    {
        while (isRunning)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    string data = serialPort.ReadLine();
                    data = data.Trim().ToLower();

                    if (!string.IsNullOrEmpty(data))
                    {
                        lock (lockObject)
                        {
                            pendingInputs.Enqueue(data);
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                // Normal timeout, ignore.
            }
            catch (Exception e)
            {
                Debug.LogWarning("Serial read error: " + e.Message);
            }
        }
    }

    void HandleSerialInput(string input)
    {
        Debug.Log("Arduino input: " + input);

        if (player == null)
        {
            player = FindFirstObjectByType<StepNodePlayerController>();
        }

        char inputLetter = input.Trim().ToLowerInvariant()[0];

        if (inputLetter < 'a' || inputLetter > 'n')
        {
            Debug.LogWarning("Unknown Arduino input: " + input);
            return;
        }

        if (player != null)
        {
            player.TryMoveToInputLetter(inputLetter);
            return;
        }

        if (!TryHandleSceneInput(inputLetter))
        {
            Debug.LogWarning("Arduino input received, but no scene handler was found for: " + inputLetter);
        }
    }

    public void HandleIPadInput(string input)
    {
        Debug.Log("iPad input: " + input);

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<StepNodePlayerController>();
        }

        char inputLetter = input.Trim().ToLowerInvariant()[0];

        if (inputLetter < 'a' || inputLetter > 'n')
        {
            Debug.LogWarning("Unknown iPad input: " + input);
            return;
        }

        if (player != null)
        {
            player.TryMoveToInputLetterFromIPad(inputLetter);
            return;
        }

        if (!TryHandleSceneInput(inputLetter))
        {
            Debug.LogWarning("iPad input received, but no scene handler was found for: " + inputLetter);
        }
    }

    bool TryHandleSceneInput(char inputLetter)
    {
        if (SceneManager.GetActiveScene().name == GameRunState.BeginSceneName)
        {
            GameRunState.ResetRun();
            GameRunState.BeginRun();
            SceneManager.LoadScene(GameRunState.MainSceneName);
            return true;
        }

        if (inputLetter == 'l')
        {
            TestMailInteractionController mailController = FindFirstObjectByType<TestMailInteractionController>();
            if (mailController != null)
            {
                mailController.TriggerClearMail();
                return true;
            }
        }

        if (inputLetter == 'm')
        {
            RiverBoatGameController boatController = FindFirstObjectByType<RiverBoatGameController>();
            if (boatController != null)
            {
                boatController.FocusForSeconds();
                return true;
            }
        }

        if (inputLetter == 'n')
        {
            PhotoNotificationPreviewController previewController = FindFirstObjectByType<PhotoNotificationPreviewController>();
            if (previewController != null)
            {
                previewController.PreviewForSeconds();
                return true;
            }
        }

        return false;
    }

    void OnApplicationQuit()
    {
        CloseSerialPort();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void CloseSerialPort()
    {
        isRunning = false;

        if (serialThread != null && serialThread.IsAlive)
        {
            serialThread.Join(500);
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }
    }
}

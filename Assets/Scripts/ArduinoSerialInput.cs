using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ArduinoSerialInput : MonoBehaviour
{
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

    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<StepNodePlayerController>();
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

    public void HandleSerialInput(string input)
    {
        Debug.Log("Arduino input: " + input);

        if (player == null)
        {
            player = FindObjectOfType<StepNodePlayerController>();
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

        player = FindObjectOfType<StepNodePlayerController>();

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
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
        }
    }

    bool TryHandleSceneInput(char inputLetter)
    {
        if (inputLetter == 'l')
        {
            TestMailInteractionController mailController = FindObjectOfType<TestMailInteractionController>();
            if (mailController != null)
            {
                mailController.TriggerClearMail();
                return true;
            }
        }

        if (inputLetter == 'm')
        {
            RiverBoatGameController boatController = FindObjectOfType<RiverBoatGameController>();
            if (boatController != null)
            {
                boatController.FocusForSeconds();
                return true;
            }
        }

        if (inputLetter == 'n')
        {
            PhotoNotificationPreviewController previewController = FindObjectOfType<PhotoNotificationPreviewController>();
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
        CloseSerialPort();
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

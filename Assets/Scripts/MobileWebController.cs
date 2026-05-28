using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class MobileWebController : MonoBehaviour
{
    public int port = 8080;
    public IPadInputManager iPadInputManager;

    private TcpListener server;
    private Thread serverThread;
    private bool running = false;

    private Queue<int> inputQueue = new Queue<int>();
    private readonly object lockObj = new object();
    private static MobileWebController instance;

    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        running = true;
        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();

        Debug.Log("iPad controller server started on port " + port);
    }

    void Update()
    {
        if (iPadInputManager == null)
        {
            iPadInputManager = FindObjectOfType<IPadInputManager>();
        }

        lock (lockObj)
        {
            while (inputQueue.Count > 0)
            {
                int tileID = inputQueue.Dequeue();
                Debug.Log("iPad clicked tile: " + tileID);

                if (iPadInputManager != null)
                {
                    iPadInputManager.PressTile(tileID);
                }
                else
                {
                    Debug.LogWarning("No IPadInputManager found in current scene.");
                }
            }
        }
    }

    void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            while (running)
            {
                TcpClient client = server.AcceptTcpClient();
                HandleClient(client);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Server error: " + e.Message);
        }
    }

    void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[4096];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (request.StartsWith("GET /trigger"))
        {
            int idIndex = request.IndexOf("id=");
            if (idIndex >= 0)
            {
                string idText = request.Substring(idIndex + 3);
                idText = idText.Split(' ')[0];
                idText = idText.Split('&')[0];

                if (int.TryParse(idText, out int tileID))
                {
                    lock (lockObj)
                    {
                        inputQueue.Enqueue(tileID);
                    }
                }
            }

            SendResponse(stream, "OK", "text/plain");
        }
        else
        {
            SendResponse(stream, GetHtmlPage(), "text/html");
        }

        stream.Close();
        client.Close();
    }

    void SendResponse(NetworkStream stream, string content, string contentType)
    {
        byte[] body = Encoding.UTF8.GetBytes(content);

        string header =
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: " + contentType + "; charset=utf-8\r\n" +
            "Content-Length: " + body.Length + "\r\n" +
            "Connection: close\r\n\r\n";

        byte[] headerBytes = Encoding.UTF8.GetBytes(header);

        stream.Write(headerBytes, 0, headerBytes.Length);
        stream.Write(body, 0, body.Length);
    }

    string GetHtmlPage()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<style>
body {
    margin: 0;
    background: #7ba85b;
    font-family: Arial, sans-serif;
    text-align: center;
}

h1 {
    margin: 12px;
    font-size: 28px;
}

.board {
    position: relative;
    width: 390px;
    height: 540px;
    margin: 20px auto;
}

.tile {
    position: absolute;
    width: 58px;
    height: 58px;
    border: none;
    background: #eeeeee;
    color: #333;
    font-size: 20px;
    font-weight: bold;
    border-radius: 6px;
    transition: 0.2s;
}

.tile:active {
    background: #ffd76a;
    transform: scale(0.95);
}

.current {
    background: #8ee08e;
}

.locked {
    opacity: 0.35;
}

.valid {
    background: #ffffff;
    box-shadow: 0 0 12px #fff176;
}
</style>
</head>

<body>
<h1>Ignore Me</h1>
<p id='status'>Start from 0</p>

<div class='board'>
    <button class='tile current' id='t0' onclick='tryMove(0)'>0</button>
    <button class='tile valid' id='t1' onclick='tryMove(1)'>1</button>
    <button class='tile locked' id='t2' onclick='tryMove(2)'>2</button>
    <button class='tile locked' id='t3' onclick='tryMove(3)'>3</button>
    <button class='tile locked' id='t4' onclick='tryMove(4)'>4</button>
    <button class='tile locked' id='t5' onclick='tryMove(5)'>5</button>
    <button class='tile locked' id='t6' onclick='tryMove(6)'>6</button>
    <button class='tile locked' id='t7' onclick='tryMove(7)'>7</button>
    <button class='tile locked' id='t8' onclick='tryMove(8)'>8</button>
    <button class='tile locked' id='t9' onclick='tryMove(9)'>9</button>
    <button class='tile locked' id='t10' onclick='tryMove(10)'>10</button>
    <button class='tile locked' id='t11' onclick='tryMove(11)'>11</button>
    <button class='tile locked' id='t12' onclick='tryMove(12)'>12</button>
    <button class='tile locked' id='t13' onclick='tryMove(13)'>13</button>
</div>

<script>
let currentTile = 0;
let stepCount = 0;
let finished = false;
let rotated = false;

let lockedBySpecialScene = false;
let lockTimer = null;

// 旋转前布局
const normalLayout = {
    0:  [70, 440],
    1:  [130, 440],
    2:  [130, 360],
    3:  [190, 360],
    11: [250, 360],
    4:  [190, 280],
    5:  [250, 280],
    12: [310, 280],
    13: [130, 200],
    6:  [250, 200],
    9:  [130, 120],
    8:  [190, 120],
    7:  [250, 120],
    10: [250, 40]
};

// 旋转后布局
const rotatedLayout = {
    10: [130, 40],
    7:  [190, 40],
    9:  [190, 200],

    8:  [190, 120],

    6:  [250, 40],
    13: [250, 200],

    0:  [70, 440],
    1:  [130, 440],
    2:  [130, 360],
    3:  [190, 360],
    4:  [190, 280],
    5:  [250, 280],
    11: [250, 360],
    12: [310, 280]
};

// 旋转前相邻关系
const normalNeighbours = {
    0:  [1],
    1:  [2],
    2:  [1, 3],
    3:  [2, 4, 11],
    4:  [3, 5],
    5:  [4, 6, 11, 12],
    6:  [5, 7],
    7:  [6, 8, 10],
    8:  [7, 9],
    9:  [8, 13],
    10: [7],
    11: [3, 5],
    12: [5],
    13: [9]
};

// 旋转后相邻关系
const rotatedNeighbours = {
    0:  [1],
    1:  [2],
    2:  [1, 3],
    3:  [2, 4, 11],
    4:  [3, 5, 9],
    5:  [4, 12, 13],
    6:  [7],
    7:  [6, 8, 10],
    8:  [7, 9],
    9:  [4, 8, 13],
    10: [7],
    11: [3],
    12: [5],
    13: [5, 9]
};

function getLayout() {
    return rotated ? rotatedLayout : normalLayout;
}

function getNeighbours() {
    return rotated ? rotatedNeighbours : normalNeighbours;
}

function tryMove(id) {
    if (finished) return;

    if (lockedBySpecialScene) {
        document.getElementById('status').innerText =
            'Special scene is active. Please wait...';
        return;
    }

    let neighbours = getNeighbours();

    if (!neighbours[currentTile].includes(id)) {
        document.getElementById('status').innerText =
            'Invalid move. Choose an adjacent tile.';
        return;
    }

    currentTile = id;
    stepCount++;

    send(id);

    if (id === 12 && !rotated) {
        rotated = true;
        applyLayout();
    }

    updateTiles();

    if (id === 10) {
        finished = true;
        document.getElementById('status').innerText =
            'Finished! Total steps: ' + stepCount;
        lockAllTiles();
        return;
    }

    if (id === 11 || id === 12 || id === 13) {
        startSpecialSceneLock(31);
        return;
    }

    document.getElementById('status').innerText =
        'Current tile: ' + id + ' | Steps: ' + stepCount;
}

function startSpecialSceneLock(seconds) {
    lockedBySpecialScene = true;

    lockAllTiles(false);

    document.getElementById('t' + currentTile)
    .classList.remove('current');

    let remaining = seconds;

    document.getElementById('status').innerText =
        'Special scene active. Wait ' + remaining + 's';

    lockTimer = setInterval(function () {

        remaining--;

        document.getElementById('status').innerText =
            'Special scene active. Wait ' + remaining + 's';

        if (remaining <= 0) {

            clearInterval(lockTimer);

            lockedBySpecialScene = false;

            updateTiles();

            document.getElementById('status').innerText =
                'Back to maze. Current tile: ' +
                currentTile +
                ' | Steps: ' +
                stepCount;
        }

    }, 1000);
}

function send(id) {
    fetch('/trigger?id=' + id);
}

function applyLayout() {
    let layout = getLayout();

    for (let i = 0; i <= 13; i++) {
        let btn = document.getElementById('t' + i);
        if (!btn) continue;

        let pos = layout[i];
        if (!pos) continue;

        btn.style.left = pos[0] + 'px';
        btn.style.top = pos[1] + 'px';
    }
}

function updateTiles() {
    let neighbours = getNeighbours();

    for (let i = 0; i <= 13; i++) {
        let btn = document.getElementById('t' + i);
        if (!btn) continue;

        btn.classList.remove('current');
        btn.classList.remove('valid');
        btn.classList.add('locked');
    }

    document.getElementById('t' + currentTile).classList.remove('locked');
    document.getElementById('t' + currentTile).classList.add('current');

    neighbours[currentTile].forEach(id => {
        let btn = document.getElementById('t' + id);
        if (!btn) return;

        btn.classList.remove('locked');
        btn.classList.add('valid');
    });
}

function lockAllTiles(showEnd = true) {
    for (let i = 0; i <= 13; i++) {
        let btn = document.getElementById('t' + i);
        if (!btn) continue;

        btn.classList.remove('valid');
        btn.classList.remove('current');
        btn.classList.add('locked');
    }

    if (showEnd) {
        document.getElementById('t10').classList.remove('locked');
        document.getElementById('t10').classList.add('current');
    }
}

applyLayout();
updateTiles();
</script>
</body>
</html>";
    }

    void OnApplicationQuit()
    {
        running = false;

        if (server != null)
        {
            server.Stop();
        }

        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Abort();
        }
    }
}
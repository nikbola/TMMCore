using BepInEx;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using TMMCore.Types;
using Newtonsoft.Json;

namespace TMMCore
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        private static Plugin _instance;

        private static Dictionary<string, UIAction> _uiActionDict = new Dictionary<string, UIAction>();
        private static Queue<Action> _logQueue = new Queue<Action>();
        private static Queue<string> _ipcQueue = new Queue<string>();
        private Thread _serverThread;
        private TcpClient _client;
        private NetworkStream _stream;

        internal static Plugin Get() => _instance;

        internal static void RegisterUIAction(string callingAssembly, UIAction action)
        {
            string id = Guid.NewGuid().ToString();
            action.modName = callingAssembly;
            _uiActionDict.Add(id, action);

            var ipcPayload = new IPCPayload();
            ipcPayload.label = action.label;
            ipcPayload.id = id;
            ipcPayload.modName = action.modName;

            if (action is ButtonAction)
                ipcPayload.actionType = ActionType.Button;
            else if (action is ToggleAction)
                ipcPayload.actionType = ActionType.Toggle;
            else if (action is SliderAction slider)
            {
                ipcPayload.actionType = ActionType.Slider;
                ipcPayload.min = slider.min;
                ipcPayload.max = slider.max;
            }
            string jsonPayload = JsonConvert.SerializeObject(ipcPayload);
            
            //_instance.Logger.LogInfo(callingAssembly + " registered " + nameof(UIElements) + " of type \"" + ipcPayload.actionType + "\"");

            _ipcQueue.Enqueue(jsonPayload);
        }

        private void Awake()
        {
            Logger.LogInfo("Plugin started");
            Logger.LogInfo("Running TCP server");

            _serverThread = new Thread(StartTcpServer);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        private void Update()
        {
            while (_logQueue.Count > 0)
                _logQueue.Dequeue()?.Invoke();

            while (_ipcQueue.Count > 0 && _client != null && _client.Connected)
            {
                string messageToSend = _ipcQueue.Dequeue();
                SendMessageToElectron(messageToSend);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                UIElements.Button("Test Button", onClick: () =>
                {
                    Logger.LogInfo("Clicked Button!");
                });
            }
        }

        private void StartTcpServer()
        {
            try
            {
                TcpListener server = new TcpListener(IPAddress.Any, 8181);
                server.Start();
                Logger.LogInfo("TCP server started at 127.0.0.1:8181");
                Logger.LogInfo("Awaiting client connection");

                while (true)
                {
                    _client = server.AcceptTcpClient();
                    _stream = _client.GetStream();
                    Logger.LogInfo("Client connected!");

                    ThreadPool.QueueUserWorkItem(HandleClient, _client);
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"Error starting TCP server: {ex.Message}");
            }
        }

        void HandleClient(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _logQueue.Enqueue(() =>
                    {
                        Logger.LogInfo("Received from Electron: " + message);
                    });

                    byte[] response = Encoding.UTF8.GetBytes("Message received: " + message);
                    stream.Write(response, 0, response.Length);
                }
            }
            catch (Exception ex)
            {
                _logQueue.Enqueue(() =>
                {
                    Logger.LogInfo($"Error handling client: {ex.Message}");
                });
            }
            finally
            {
                client.Close();
                _logQueue.Enqueue(() =>
                {
                    Logger.LogInfo("Client disconnected.");
                });
            }
        }

        private void SendMessageToElectron(string message)
        {
            if (_stream != null && _client.Connected)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _stream.Write(data, 0, data.Length);
                    Logger.LogInfo("Sent message to Electron: " + message);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to send message to Electron: " + ex.Message);
                }
            }
        }

        private void OnDestroy()
        {
            if (_serverThread != null && _serverThread.IsAlive)
            {
                _serverThread.Abort();
                Logger.LogInfo("TCP server thread aborted.");
            }

            if (_client != null)
            {
                _client.Close();
                Logger.LogInfo("TCP client closed.");
            }
        }
    }
}

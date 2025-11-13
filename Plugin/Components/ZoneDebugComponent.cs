using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MoreBotsAPI.Components
{
    internal class ZoneDebugComponent : MonoBehaviour, IDisposable
    {
        private GameWorld gameWorld;
        private BotSpawner botSpawner;
        private Player localPlayer;

        private GUIStyle guiStyle;
        private float nextUpdateTime;
        private bool updateGuiPending = false;

        private Dictionary<string, ZoneData> zones = new Dictionary<string, ZoneData>();
        private List<string> deadList = new List<string>();
        protected ManualLogSource Logger;
        float screenScale = 1.0f;

#if DEBUG
        long memAllocUpdate = 0;
        long memAllocGui = 0;
        float lastMemOutUpdate = 0;
        float lastMemOutGui = 0;
        float memTimeframe = 5.0f;
#endif

        private ZoneDebugComponent()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void Awake()
        {
            botSpawner = (Singleton<IBotGame>.Instance).BotsController.BotSpawner;
            gameWorld = Singleton<GameWorld>.Instance;
            localPlayer = gameWorld.MainPlayer;

            var BotZones = new List<BotZone>(FindObjectsByType<BotZone>(UnityEngine.FindObjectsSortMode.None));

            foreach (BotZone zone in BotZones)
            {
                if (!zones.TryGetValue(zone.NameZone, out var zoneData))
                {
                    zoneData = new ZoneData();
                    zones.Add(zone.NameZone, zoneData);
                }

                zoneData.SetData(zone);
            }

            Logger.LogInfo("ZoneDebugComponent enabled");

            // If DLSS or FSR are enabled, set a screen scale value
            if (CameraClass.Instance.SSAA.isActiveAndEnabled)
            {
                screenScale = (float)CameraClass.Instance.SSAA.GetOutputWidth() / (float)CameraClass.Instance.SSAA.GetInputWidth();
                Logger.LogDebug($"DLSS or FSR is enabled, scale screen offsets by {screenScale}");
            }
        }

        public void Dispose()
        {
            Logger.LogInfo("ZoneDebugComponent disabled");
            Destroy(this);
        }

        private void CreateGuiStyle()
        {
            guiStyle = new GUIStyle(GUI.skin.box);
            guiStyle.alignment = TextAnchor.MiddleLeft;
            guiStyle.fontSize = 12;
            guiStyle.margin = new RectOffset(3, 3, 3, 3);
            guiStyle.richText = true;
        }

        private void OnGUI()
        {
            if (!Plugin.DrawBotZones.Value)
            {
                return;
            }

#if DEBUG
            long startMem = GC.GetTotalMemory(false);
#endif

            if (guiStyle == null)
            {
                CreateGuiStyle();
            }

            foreach (var zone in zones)
            {
                var zoneData = zone.Value.Data;


                // Make sure we have a GuiContent and GuiRect object for this bot
                if (zone.Value.GuiContent == null)
                {
                    zone.Value.GuiContent = new GUIContent();
                    zone.Value.GuiContent.text = zoneData.NameZone;
                }
                if (zone.Value.GuiRect == null)
                {
                    zone.Value.GuiRect = new Rect();
                }

                // Only draw the bot data if it's visible on screen
                Vector3 aboveBotHeadPos = zoneData.CenterOfSpawnPoints + (Vector3.up * 1.5f);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(aboveBotHeadPos);
                if (screenPos.z > 0)
                {
                    Vector2 guiSize = guiStyle.CalcSize(zone.Value.GuiContent);
                    zone.Value.GuiRect.x = (screenPos.x * screenScale) - (guiSize.x / 2);
                    zone.Value.GuiRect.y = Screen.height - ((screenPos.y * screenScale) + guiSize.y);
                    zone.Value.GuiRect.size = guiSize;

                    GUI.Box(zone.Value.GuiRect, zone.Value.GuiContent, guiStyle);
                }
            }

#if DEBUG
            memAllocGui += GC.GetTotalMemory(false) - startMem;
            if (Time.time - lastMemOutGui > memTimeframe)
            {
                Logger.LogDebug($"GUI Memory Allocated ({memTimeframe}s): {Math.Floor(memAllocGui / 1024f)} KiB");
                memAllocGui = 0;
                lastMemOutGui = Time.time;
            }
#endif
        }

        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;

                if (gameWorld.gameObject.GetComponent<ZoneDebugComponent>() == null)
                {
                    gameWorld.gameObject.AddComponent<ZoneDebugComponent>();
                }
            }
        }

        public static void Disable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetComponent<ZoneDebugComponent>()?.Dispose();
            }
        }

        internal class ZoneData
        {
            public void SetData(BotZone botData)
            {
                LastUpdate = Time.time;
                Data = botData;
            }

            public float LastUpdate;
            public BotZone Data;
            public GUIContent GuiContent;
            public Rect GuiRect;
        }
    }
}

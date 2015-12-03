using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;

namespace AegisTimer
{
    internal class Program
    {
        private static float aegistime, timeleft = 0, deathTime = 0;
        private static int sec = 0, min = 0;
        private static bool aegispicked = false, _loaded = false, roshdead = false;
        private static Color BColor = new Color(0xC0111111);
        private static Color BOColor = new Color(0xFF444444);
        private static double ratiox = HUDInfo.ScreenSizeX() / 1920, ratioy = HUDInfo.ScreenSizeY() / 1080;
        private static Vector2 size = new Vector2((float)(200*ratiox), (float)(60 *ratioy));
        public static double startposx = 1718.4 * ratiox, startposy = 356 * ratioy;
        private static readonly Dictionary<string, DotaTexture> TextureCache = new Dictionary<string, DotaTexture>();
        private static Font FontArray;
        private static Item aegis;
        private static readonly Menu Menu = new Menu("Aegis Timer", "aegistimer", true);
        public static void Main()
        {
            Menu.AddItem(new MenuItem("aegistimeronoff", "Aegis Timer").SetValue(true));
            Menu.AddToMainMenu();
            FontArray = new Font(
                    Drawing.Direct3DDevice9,
                    new FontDescription
                    {
                        FaceName = "Tahoma",
                        Height = 15,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });
            Game.OnFireEvent += Game_OnFireEvent;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void Game_OnFireEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name == "dota_roshan_kill")
            {
                deathTime = Game.GameTime;
                roshdead = true;
            }
            aegis = ObjectMgr.GetEntities<Item>().Find(item => item.Name == "item_aegis" && item.IsValid);
            if (args.GameEvent.Name == "spec_item_pickup" && roshdead && aegis != null && aegis.Owner != null)
            {
                aegistime = Game.GameTime;
                aegispicked = true;
            }
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init
            if (!_loaded)
            {
                if (!Game.IsInGame)
                {
                    return;
                }
                _loaded = true;

            }

            if (!Game.IsInGame)
            {
                _loaded = false;
                PrintInfo("> Aegis Timer unLoaded");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }
            #endregion
            if (aegispicked)
            {
                timeleft = 300 - (Game.GameTime - aegistime);
            }
            aegis = ObjectMgr.GetEntities<Item>().Find(item => item.Name == "item_aegis" && item.IsValid);
            if (aegis != null)
            {
                if (aegis.Owner.IsAlive && timeleft > 0)
                {
                    min = (int)Math.Floor(timeleft / 60);
                    sec = (int)Math.Floor(timeleft % 60);
                }
                else aegispicked = false;
            }
            else aegispicked = false;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_loaded) return;

            if (aegispicked && Menu.Item("aegistimeronoff").GetValue<bool>())
            {
                Drawing.DrawRect(new Vector2((float)startposx, (float)startposy), size, BColor);
                Drawing.DrawRect(new Vector2((float)startposx, (float)startposy), size, BOColor, true);
                Drawing.DrawRect(new Vector2((float)startposx - 1, (float)startposy - 1), size + new Vector2(2, 2), Color.Black, true);
                Drawing.DrawRect(new Vector2((float)(startposx + 15 * ratiox), (float)(startposy + 10 * ratioy)), new Vector2((float)(80 * ratiox), (float)(40 * ratioy)), GetTexture("materials/ensage_ui/items/aegis.vmat"));
                Drawing.DrawText("RECLAIMED IN:", new Vector2((float)(startposx + 100*ratiox), (float)(startposy + 12*ratioy)), Color.White, FontFlags.DropShadow);
                if(sec < 10) Drawing.DrawText("" + min.ToString() + ":0" + sec.ToString(), new Vector2((float)(startposx + 100 * ratiox), (float)(startposy + 25 * ratioy)), Color.White, FontFlags.DropShadow);
                else Drawing.DrawText("" + min.ToString() + ":" + sec.ToString(), new Vector2((float)(startposx + 100 * ratiox), (float)(startposy + 25 * ratioy)), Color.White, FontFlags.DropShadow);
            }
        }
        private static DotaTexture GetTexture(string name)
        {
            if (TextureCache.ContainsKey(name)) return TextureCache[name];

            return TextureCache[name] = Drawing.GetTexture(name);
        }
        public static void DrawShadowText(string stext, int x, int y, Color color, Font f)
        {
            f.DrawText(null, stext, x + 1, y + 1, Color.Black);
            f.DrawText(null, stext, x, y, color);
        }
        #region Helpers
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
        #endregion
    }
}
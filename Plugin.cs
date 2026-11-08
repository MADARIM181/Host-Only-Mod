using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace MeuModHostOnly
{
    [BepInPlugin("com.meuusuario.cianofobico", "Mod Cianofobico Host Only", "1.0.0")]
    [BepInProcess("Among Us.exe")]
    public class PluginMain : BasePlugin
    {
        public static bool CianofobicoAtivo = false;
        public static bool AcusouCiano = false;

        public override void Load()
        {
            Harmony harmony = new Harmony("com.meuusuario.cianofobico");
            harmony.PatchAll();
            Log.LogInfo("Mod Cianofobico carregado com sucesso!");
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatPatch
    {
        public static bool Prefix(ChatController __instance)
        {
            string texto = __instance.textArea.text.ToLower().Trim();

            if (!PlayerControl.LocalPlayer.AmHost) return true;

            if (texto == "/cmd")
            {
                EnviarChat("<color=#00FFFF>--- COMANDOS HOST ONLY ---</color>\n/cianofobico on | off\n/cmd");
                __instance.textArea.Clear();
                return false;
            }

            if (texto == "/cianofobico on")
            {
                PluginMain.CianofobicoAtivo = true;
                PluginMain.AcusouCiano = false;
                EnviarChat("<color=#00FFFF>[SISTEMA]: Cianofóbico ATIVADO!</color>");
                __instance.textArea.Clear();
                return false;
            }

            if (texto == "/cianofobico off")
            {
                PluginMain.CianofobicoAtivo = false;
                PluginMain.AcusouCiano = false;
                EnviarChat("<color=#FF0000>[SISTEMA]: Cianofóbico DESATIVADO.</color>");
                __instance.textArea.Clear();
                return false;
            }

            if (PluginMain.CianofobicoAtivo && texto.Contains("ciano"))
            {
                PluginMain.AcusouCiano = true;
            }

            return true;
        }

        private static void EnviarChat(string msg)
        {
            if (DestroyableSingleton<ChatController>.Instance != null)
            {
                DestroyableSingleton<ChatController>.Instance.AddChat(PlayerControl.LocalPlayer, msg);
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public static class ExilePatch
    {
        public static void Postfix(ExileController __instance)
        {
            if (!PluginMain.CianofobicoAtivo) return;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead && (player.Data.ColorId == 10 || player.Data.ColorId == 1))
                {
                    if (PluginMain.AcusouCiano)
                    {
                        string msgVitoria = "<color=#00FFFF>Cianofóbico tirou o ciano!\nOs tripulantes ganharam a partida!</color>";
                        
                        if (DestroyableSingleton<ChatController>.Instance != null)
                        {
                            DestroyableSingleton<ChatController>.Instance.AddChat(PlayerControl.LocalPlayer, msgVitoria);
                        }

                        if (ShipStatus.Instance != null)
                        {
                            ShipStatus.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                        }
                    }
                    else
                    {
                        if (DestroyableSingleton<ChatController>.Instance != null)
                        {
                            DestroyableSingleton<ChatController>.Instance.AddChat(PlayerControl.LocalPlayer, "<color=#FF0000>[SISTEMA COM ERROS]: Ciano ejetado sem acusação prévia no chat!</color>");
                        }
                    }
                    break;
                }
            }
        }
    }
}

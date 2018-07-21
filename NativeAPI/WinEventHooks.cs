using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sextant.NativeAPI
{
    public static class WinEventHooks
    {
        public delegate void WinEventCallback(
            IntPtr windowHandle, uint eventType,
            int objectId, int childId,
            uint thread, uint timestamp);

        [DllImport("user32.dll")]
        public extern static IntPtr SetWinEventHook(
           uint eventMin, uint eventMax, 
           IntPtr hmodWinEventProc, 
           WinEventCallback callback, 
           uint idProcess, uint idThread, 
           uint dwFlags);

        [DllImport("user32.dll")]
        public extern static bool UnhookWinEvent(IntPtr hWinEventHook);

        public const uint WINEVENT_OUTOFCONTEXT = 0u;

        public const uint EVENT_MIN = 0x00000001u;
        public const uint EVENT_MAX = 0x7FFFFFFFu;

        public const uint EVENT_OBJECT_CLOAKED                          = 0x8017u;
        public const uint EVENT_OBJECT_CONTENTSCROLLED                  = 0x8015u;
        public const uint EVENT_OBJECT_CREATE                           = 0x8000u;
        public const uint EVENT_OBJECT_DEFACTIONCHANGE                  = 0x8011u;
        public const uint EVENT_OBJECT_DESCRIPTIONCHANGE                = 0x800Du;
        public const uint EVENT_OBJECT_DESTROY                          = 0x8001u;
        public const uint EVENT_OBJECT_DRAGSTART                        = 0x8021u;
        public const uint EVENT_OBJECT_DRAGCANCEL                       = 0x8022u;
        public const uint EVENT_OBJECT_DRAGCOMPLETE                     = 0x8023u;
        public const uint EVENT_OBJECT_DRAGENTER                        = 0x8024u;
        public const uint EVENT_OBJECT_DRAGLEAVE                        = 0x8025u;
        public const uint EVENT_OBJECT_DRAGDROPPED                      = 0x8026u;
        public const uint EVENT_OBJECT_END                              = 0x80FFu;
        public const uint EVENT_OBJECT_FOCUS                            = 0x8005u;
        public const uint EVENT_OBJECT_HELPCHANGE                       = 0x8010u;
        public const uint EVENT_OBJECT_HIDE                             = 0x8003u;
        public const uint EVENT_OBJECT_HOSTEDOBJECTSINVALIDATED         = 0x8020u;
        public const uint EVENT_OBJECT_IME_HIDE                         = 0x8028u;
        public const uint EVENT_OBJECT_IME_SHOW                         = 0x8027u;
        public const uint EVENT_OBJECT_IME_CHANGE                       = 0x8029u;
        public const uint EVENT_OBJECT_INVOKED                          = 0x8013u;
        public const uint EVENT_OBJECT_LIVEREGIONCHANGED                = 0x8019u;
        public const uint EVENT_OBJECT_LOCATIONCHANGE                   = 0x800Bu;
        public const uint EVENT_OBJECT_NAMECHANGE                       = 0x800Cu;
        public const uint EVENT_OBJECT_PARENTCHANGE                     = 0x800Fu;
        public const uint EVENT_OBJECT_REORDER                          = 0x8004u;
        public const uint EVENT_OBJECT_SELECTION                        = 0x8006u;
        public const uint EVENT_OBJECT_SELECTIONADD                     = 0x8007u;
        public const uint EVENT_OBJECT_SELECTIONREMOVE                  = 0x8008u;
        public const uint EVENT_OBJECT_SELECTIONWITHIN                  = 0x8009u;
        public const uint EVENT_OBJECT_SHOW                             = 0x8002u;
        public const uint EVENT_OBJECT_STATECHANGE                      = 0x800Au;
        public const uint EVENT_OBJECT_TEXTEDIT_CONVERSIONTARGETCHANGED = 0x8030u;
        public const uint EVENT_OBJECT_TEXTSELECTIONCHANGED             = 0x8014u;
        public const uint EVENT_OBJECT_UNCLOAKED                        = 0x8018u;
        public const uint EVENT_OBJECT_VALUECHANGE                      = 0x800Eu;
        public const uint EVENT_SYSTEM_ALERT                            = 0x0002u;
        public const uint EVENT_SYSTEM_ARRANGMENTPREVIEW                = 0x8016u;
        public const uint EVENT_SYSTEM_CAPTUREEND                       = 0x0009u;
        public const uint EVENT_SYSTEM_CAPTURESTART                     = 0x0008u;
        public const uint EVENT_SYSTEM_CONTEXTHELPEND                   = 0x000Du;
        public const uint EVENT_SYSTEM_CONTEXTHELPSTART                 = 0x000Cu;
        public const uint EVENT_SYSTEM_DESKTOPSWITCH                    = 0x0020u;
        public const uint EVENT_SYSTEM_DIALOGEND                        = 0x0011u;
        public const uint EVENT_SYSTEM_DIALOGSTART                      = 0x0010u;
        public const uint EVENT_SYSTEM_DRAGDROPEND                      = 0x000Fu;
        public const uint EVENT_SYSTEM_DRAGDROPSTART                    = 0x000Eu;
        public const uint EVENT_SYSTEM_END                              = 0x00FFu;
        public const uint EVENT_SYSTEM_FOREGROUND                       = 0x0003u;
        public const uint EVENT_SYSTEM_MENUPOPUPEND                     = 0x0007u;
        public const uint EVENT_SYSTEM_MENUPOPUPSTART                   = 0x0006u;
        public const uint EVENT_SYSTEM_MENUEND                          = 0x0005u;
        public const uint EVENT_SYSTEM_MENUSTART                        = 0x0004u;
        public const uint EVENT_SYSTEM_MINIMIZEEND                      = 0x0017u;
        public const uint EVENT_SYSTEM_MINIMIZESTART                    = 0x0016u;
        public const uint EVENT_SYSTEM_MOVESIZEEND                      = 0x000Bu;
        public const uint EVENT_SYSTEM_MOVESIZESTART                    = 0x000Au;
        public const uint EVENT_SYSTEM_SCROLLINGEND                     = 0x0013u;
        public const uint EVENT_SYSTEM_SCROLLINGSTART                   = 0x0012u;
        public const uint EVENT_SYSTEM_SOUND                            = 0x0001u;
        public const uint EVENT_SYSTEM_SWITCHEND                        = 0x0015u;
        public const uint EVENT_SYSTEM_SWITCHSTART                      = 0x0014u;
    }
}

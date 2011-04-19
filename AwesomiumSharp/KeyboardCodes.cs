using System;
using System.Collections.Generic;
using System.Text;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public enum VirtualKey
    {
        // AK_BACK (08) BACKSPACE key
		BACK = 0x08,
		
		// AK_TAB (09) TAB key
		TAB = 0x09,
		
		// AK_CLEAR (0C) CLEAR key
		CLEAR = 0x0C,
		
		// AK_RETURN (0D)
		RETURN = 0x0D,
		
		// AK_SHIFT (10) SHIFT key
		SHIFT = 0x10,
		
		// AK_CONTROL (11) CTRL key
		CONTROL = 0x11,
		
		// AK_MENU (12) ALT key
		MENU = 0x12,
		
		// AK_PAUSE (13) PAUSE key
		PAUSE = 0x13,
		
		// AK_CAPITAL (14) CAPS LOCK key
		CAPITAL = 0x14,
		
		// AK_KANA (15) Input Method Editor (IME) Kana mode
		KANA = 0x15,
		
		// AK_HANGUEL (15) IME Hanguel mode (maintained for compatibility, use AK_HANGUL)
		// AK_HANGUL (15) IME Hangul mode
		HANGUL = 0x15,
		
		// AK_JUNJA (17) IME Junja mode
		JUNJA = 0x17,
		
		// AK_FINAL (18) IME final mode
		FINAL = 0x18,
		
		// AK_HANJA (19) IME Hanja mode
		HANJA = 0x19,
		
		// AK_KANJI (19) IME Kanji mode
		KANJI = 0x19,
		
		// AK_ESCAPE (1B) ESC key
		ESCAPE = 0x1B,
		
		// AK_CONVERT (1C) IME convert
		CONVERT = 0x1C,
		
		// AK_NONCONVERT (1D) IME nonconvert
		NONCONVERT = 0x1D,
		
		// AK_ACCEPT (1E) IME accept
		ACCEPT = 0x1E,
		
		// AK_MODECHANGE (1F) IME mode change request
		MODECHANGE = 0x1F,
		
		// AK_SPACE (20) SPACEBAR
		SPACE = 0x20,
		
		// AK_PRIOR (21) PAGE UP key
		PRIOR = 0x21,
		
		// AK_NEXT (22) PAGE DOWN key
		NEXT = 0x22,
		
		// AK_END (23) END key
		END = 0x23,
		
		// AK_HOME (24) HOME key
		HOME = 0x24,
		
		// AK_LEFT (25) LEFT ARROW key
		LEFT = 0x25,
		
		// AK_UP (26) UP ARROW key
		UP = 0x26,
		
		// AK_RIGHT (27) RIGHT ARROW key
		RIGHT = 0x27,
		
		// AK_DOWN (28) DOWN ARROW key
		DOWN = 0x28,
		
		// AK_SELECT (29) SELECT key
		SELECT = 0x29,
		
		// AK_PRINT (2A) PRINT key
		PRINT = 0x2A,
		
		// AK_EXECUTE (2B) EXECUTE key
		EXECUTE = 0x2B,
		
		// AK_SNAPSHOT (2C) PRINT SCREEN key
		SNAPSHOT = 0x2C,
		
		// AK_INSERT (2D) INS key
		INSERT = 0x2D,
		
		// AK_DELETE (2E) DEL key
		DELETE = 0x2E,
		
		// AK_HELP (2F) HELP key
		HELP = 0x2F,
		
		// (30) 0 key
		NUM_0 = 0x30,
		
		// (31) 1 key
		NUM_1 = 0x31,
		
		// (32) 2 key
		NUM_2 = 0x32,
		
		// (33) 3 key
		NUM_3 = 0x33,
		
		// (34) 4 key
		NUM_4 = 0x34,
		
		// (35) 5 key,
		NUM_5 = 0x35,
		
		// (36) 6 key
		NUM_6 = 0x36,
		
		// (37) 7 key
		NUM_7 = 0x37,
		
		// (38) 8 key
		NUM_8 = 0x38,
		
		// (39) 9 key
        NUM_9 = 0x39,
		
		// (41) A key
		A = 0x41,
		
		// (42) B key
		B = 0x42,
		
		// (43) C key
		C = 0x43,
		
		// (44) D key
		D = 0x44,
		
		// (45) E key
		E = 0x45,
		
		// (46) F key
		F = 0x46,
		
		// (47) G key
		G = 0x47,
		
		// (48) H key
		H = 0x48,
		
		// (49) I key
		I = 0x49,
		
		// (4A) J key
		J = 0x4A,
		
		// (4B) K key
		K = 0x4B,
		
		// (4C) L key
		L = 0x4C,
		
		// (4D) M key
		M = 0x4D,
		
		// (4E) N key
		N = 0x4E,
		
		// (4F) O key
		O = 0x4F,
		
		// (50) P key
		P = 0x50,
		
		// (51) Q key
		Q = 0x51,
		
		// (52) R key
		R = 0x52,
		
		// (53) S key
		S = 0x53,
		
		// (54) T key
		T = 0x54,
		
		// (55) U key
		U = 0x55,
		
		// (56) V key
		V = 0x56,
		
		// (57) W key
		W = 0x57,
		
		// (58) X key
		X = 0x58,
		
		// (59) Y key
		Y = 0x59,
		
		// (5A) Z key
		Z = 0x5A,
		
		// AK_LWIN (5B) Left Windows key (Microsoft Natural keyboard)
		LWIN = 0x5B,
		
		// AK_RWIN (5C) Right Windows key (Natural keyboard)
		RWIN = 0x5C,
		
		// AK_APPS (5D) Applications key (Natural keyboard)
		APPS = 0x5D,
		
		// AK_SLEEP (5F) Computer Sleep key
		SLEEP = 0x5F,
		
		// AK_NUMPAD0 (60) Numeric keypad 0 key
		NUMPAD0 = 0x60,
		
		// AK_NUMPAD1 (61) Numeric keypad 1 key
		NUMPAD1 = 0x61,
		
		// AK_NUMPAD2 (62) Numeric keypad 2 key
		NUMPAD2 = 0x62,
		
		// AK_NUMPAD3 (63) Numeric keypad 3 key
		NUMPAD3 = 0x63,
		
		// AK_NUMPAD4 (64) Numeric keypad 4 key
		NUMPAD4 = 0x64,
		
		// AK_NUMPAD5 (65) Numeric keypad 5 key
		NUMPAD5 = 0x65,
		
		// AK_NUMPAD6 (66) Numeric keypad 6 key
		NUMPAD6 = 0x66,
		
		// AK_NUMPAD7 (67) Numeric keypad 7 key
		NUMPAD7 = 0x67,
		
		// AK_NUMPAD8 (68) Numeric keypad 8 key
		NUMPAD8 = 0x68,
		
		// AK_NUMPAD9 (69) Numeric keypad 9 key
		NUMPAD9 = 0x69,
		
		// AK_MULTIPLY (6A) Multiply key
		MULTIPLY = 0x6A,
		
		// AK_ADD (6B) Add key
		ADD = 0x6B,
		
		// AK_SEPARATOR (6C) Separator key
		SEPARATOR = 0x6C,
		
		// AK_SUBTRACT (6D) Subtract key
		SUBTRACT = 0x6D,
		
		// AK_DECIMAL (6E) Decimal key
		DECIMAL = 0x6E,
		
		// AK_DIVIDE (6F) Divide key
		DIVIDE = 0x6F,
		
		// AK_F1 (70) F1 key
		F1 = 0x70,
		
		// AK_F2 (71) F2 key
		F2 = 0x71,
		
		// AK_F3 (72) F3 key
		F3 = 0x72,
		
		// AK_F4 (73) F4 key
		F4 = 0x73,
		
		// AK_F5 (74) F5 key
		F5 = 0x74,
		
		// AK_F6 (75) F6 key
		F6 = 0x75,
		
		// AK_F7 (76) F7 key
		F7 = 0x76,
		
		// AK_F8 (77) F8 key
		F8 = 0x77,
		
		// AK_F9 (78) F9 key
		F9 = 0x78,
		
		// AK_F10 (79) F10 key
		F10 = 0x79,
		
		// AK_F11 (7A) F11 key
		F11 = 0x7A,
		
		// AK_F12 (7B) F12 key
		F12 = 0x7B,
		
		// AK_F13 (7C) F13 key
		F13 = 0x7C,
		
		// AK_F14 (7D) F14 key
		F14 = 0x7D,
		
		// AK_F15 (7E) F15 key
		F15 = 0x7E,
		
		// AK_F16 (7F) F16 key
		F16 = 0x7F,
		
		// AK_F17 (80H) F17 key
		F17 = 0x80,
		
		// AK_F18 (81H) F18 key
		F18 = 0x81,
		
		// AK_F19 (82H) F19 key
		F19 = 0x82,
		
		// AK_F20 (83H) F20 key
		F20 = 0x83,
		
		// AK_F21 (84H) F21 key
		F21 = 0x84,
		
		// AK_F22 (85H) F22 key
		F22 = 0x85,
		
		// AK_F23 (86H) F23 key
		F23 = 0x86,
		
		// AK_F24 (87H) F24 key
		F24 = 0x87,
		
		// AK_NUMLOCK (90) NUM LOCK key
		NUMLOCK = 0x90,
		
		// AK_SCROLL (91) SCROLL LOCK key
		SCROLL = 0x91,
		
		// AK_LSHIFT (A0) Left SHIFT key
		LSHIFT = 0xA0,
		
		// AK_RSHIFT (A1) Right SHIFT key
		RSHIFT = 0xA1,
		
		// AK_LCONTROL (A2) Left CONTROL key
		LCONTROL = 0xA2,
		
		// AK_RCONTROL (A3) Right CONTROL key
		RCONTROL = 0xA3,
		
		// AK_LMENU (A4) Left MENU key
		LMENU = 0xA4,
		
		// AK_RMENU (A5) Right MENU key
		RMENU = 0xA5,
		
		// AK_BROWSER_BACK (A6) Windows 2000/XP: Browser Back key
		BROWSER_BACK = 0xA6,
		
		// AK_BROWSER_FORWARD (A7) Windows 2000/XP: Browser Forward key
		BROWSER_FORWARD = 0xA7,
		
		// AK_BROWSER_REFRESH (A8) Windows 2000/XP: Browser Refresh key
		BROWSER_REFRESH = 0xA8,
		
		// AK_BROWSER_STOP (A9) Windows 2000/XP: Browser Stop key
		BROWSER_STOP = 0xA9,
		
		// AK_BROWSER_SEARCH (AA) Windows 2000/XP: Browser Search key
		BROWSER_SEARCH = 0xAA,
		
		// AK_BROWSER_FAVORITES (AB) Windows 2000/XP: Browser Favorites key
		BROWSER_FAVORITES = 0xAB,
		
		// AK_BROWSER_HOME (AC) Windows 2000/XP: Browser Start and Home key
		BROWSER_HOME = 0xAC,
		
		// AK_VOLUME_MUTE (AD) Windows 2000/XP: Volume Mute key
		VOLUME_MUTE = 0xAD,
		
		// AK_VOLUME_DOWN (AE) Windows 2000/XP: Volume Down key
		VOLUME_DOWN = 0xAE,
		
		// AK_VOLUME_UP (AF) Windows 2000/XP: Volume Up key
		VOLUME_UP = 0xAF,
		
		// AK_MEDIA_NEXT_TRACK (B0) Windows 2000/XP: Next Track key
		MEDIA_NEXT_TRACK = 0xB0,
		
		// AK_MEDIA_PREV_TRACK (B1) Windows 2000/XP: Previous Track key
		MEDIA_PREV_TRACK = 0xB1,
		
		// AK_MEDIA_STOP (B2) Windows 2000/XP: Stop Media key
		MEDIA_STOP = 0xB2,
		
		// AK_MEDIA_PLAY_PAUSE (B3) Windows 2000/XP: Play/Pause Media key
		MEDIA_PLAY_PAUSE = 0xB3,
		
		// AK_LAUNCH_MAIL (B4) Windows 2000/XP: Start Mail key
		MEDIA_LAUNCH_MAIL = 0xB4,
		
		// AK_LAUNCH_MEDIA_SELECT (B5) Windows 2000/XP: Select Media key
		MEDIA_LAUNCH_MEDIA_SELECT = 0xB5,
		
		// AK_LAUNCH_APP1 (B6) Windows 2000/XP: Start Application 1 key
		MEDIA_LAUNCH_APP1 = 0xB6,
		
		// AK_LAUNCH_APP2 (B7) Windows 2000/XP: Start Application 2 key
		MEDIA_LAUNCH_APP2 = 0xB7,
		
		// AK_OEM_1 (BA) Used for miscellaneous characters, it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ',:' key
		OEM_1 = 0xBA,
		
		// AK_OEM_PLUS (BB) Windows 2000/XP: For any country/region, the '+' key
		OEM_PLUS = 0xBB,
		
		// AK_OEM_COMMA (BC) Windows 2000/XP: For any country/region, the ',' key
		OEM_COMMA = 0xBC,
		
		// AK_OEM_MINUS (BD) Windows 2000/XP: For any country/region, the '-' key
		OEM_MINUS = 0xBD,
		
		// AK_OEM_PERIOD (BE) Windows 2000/XP: For any country/region, the '.' key
		OEM_PERIOD = 0xBE,
		
		// AK_OEM_2 (BF) Used for miscellaneous characters, it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '/?' key
		OEM_2 = 0xBF,
		
		// AK_OEM_3 (C0) Used for miscellaneous characters, it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '`~' key
		OEM_3 = 0xC0,
		
		// AK_OEM_4 (DB) Used for miscellaneous characters, it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '[{' key
		OEM_4 = 0xDB,
		
		// AK_OEM_5 (DC) Used for miscellaneous characters, it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '\|' key
		OEM_5 = 0xDC,
		
		// AK_OEM_6 (DD) Used for miscellaneous characters, it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ']}' key
		OEM_6 = 0xDD,
		
		// AK_OEM_7 (DE) Used for miscellaneous characters, it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the 'single-quote/double-quote' key
		OEM_7 = 0xDE,
		
		// AK_OEM_8 (DF) Used for miscellaneous characters, it can vary by keyboard.
		OEM_8 = 0xDF,
		
		// AK_OEM_102 (E2) Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
		OEM_102 = 0xE2,
		
		// AK_PROCESSKEY (E5) Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
		PROCESSKEY = 0xE5,
		
		// AK_PACKET (E7) Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes. The AK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT,SendInput, WM_KEYDOWN, and WM_KEYUP
		PACKET = 0xE7,
		
		// AK_ATTN (F6) Attn key
		ATTN = 0xF6,
		
		// AK_CRSEL (F7) CrSel key
		CRSEL = 0xF7,
		
		// AK_EXSEL (F8) ExSel key
		EXSEL = 0xF8,
		
		// AK_EREOF (F9) Erase EOF key
		EREOF = 0xF9,
		
		// AK_PLAY (FA) Play key
		PLAY = 0xFA,
		
		// AK_ZOOM (FB) Zoom key
		ZOOM = 0xFB,
		
		// AK_NONAME (FC) Reserved for future use
		NONAME = 0xFC,
		
		// AK_PA1 (FD) PA1 key
		PA1 = 0xFD,
		
		// AK_OEM_CLEAR (FE) Clear key
		OEM_CLEAR = 0xFE,
		
		UNKNOWN = 0,
    };
}

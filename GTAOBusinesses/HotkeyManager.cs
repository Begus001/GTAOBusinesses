using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GTAOBusinesses
{
	public class HotkeyManager
	{
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		private readonly IntPtr hwnd;
		private const int id = 0x42069911;

		public readonly int NumActions;

		public Tuple<ModifierKey, VirtualKey>[] Bindings { get; }
		private readonly Tuple<ModifierKey, VirtualKey>[] defaults;

		public delegate void HotkeyPressedEventHandler(HotkeyEventArgs e);
		public event HotkeyPressedEventHandler HotkeyPressed;

		public string SaveLocation { get; set; }

		public HotkeyManager(Window win, string saveLocation)
		{
			hwnd = new WindowInteropHelper(win).EnsureHandle();
			HwndSource source = HwndSource.FromHwnd(hwnd);
			source.AddHook(HwndHook);

			NumActions = Enum.GetNames(typeof(HotkeyAction)).Length;

			Bindings = new Tuple<ModifierKey, VirtualKey>[NumActions];
			defaults = new Tuple<ModifierKey, VirtualKey>[NumActions];

			defaults[(int)HotkeyAction.Pause] = Tuple.Create(ModifierKey.CTRL, VirtualKey.NumPad0);
			defaults[(int)HotkeyAction.SoloSession] = Tuple.Create(ModifierKey.CTRL, VirtualKey.Add);
			defaults[(int)HotkeyAction.ResupplyBunker] = Tuple.Create(ModifierKey.CTRL, VirtualKey.NumPad1);
			defaults[(int)HotkeyAction.ResupplyCocaine] = Tuple.Create(ModifierKey.CTRL, VirtualKey.NumPad2);
			defaults[(int)HotkeyAction.ResupplyMeth] = Tuple.Create(ModifierKey.CTRL, VirtualKey.NumPad3);
			defaults[(int)HotkeyAction.ResupplyCounterfeit] = Tuple.Create(ModifierKey.CTRL, VirtualKey.NumPad4);
			defaults[(int)HotkeyAction.SellBunker] = Tuple.Create(ModifierKey.CTRL | ModifierKey.ALT, VirtualKey.NumPad1);
			defaults[(int)HotkeyAction.SellCocaine] = Tuple.Create(ModifierKey.CTRL | ModifierKey.ALT, VirtualKey.NumPad2);
			defaults[(int)HotkeyAction.SellMeth] = Tuple.Create(ModifierKey.CTRL | ModifierKey.ALT, VirtualKey.NumPad3);
			defaults[(int)HotkeyAction.SellCounterfeit] = Tuple.Create(ModifierKey.CTRL | ModifierKey.ALT, VirtualKey.NumPad4);
			defaults[(int)HotkeyAction.AFK] = Tuple.Create(ModifierKey.SHIFT, VirtualKey.Add);
			defaults[(int)HotkeyAction.ResupplyAcid] = Tuple.Create(ModifierKey.CTRL, VirtualKey.NumPad5);
			defaults[(int)HotkeyAction.SellAcid] = Tuple.Create(ModifierKey.CTRL | ModifierKey.ALT, VirtualKey.NumPad5);
			defaults[(int)HotkeyAction.FlyHeli] = Tuple.Create(ModifierKey.CTRL, VirtualKey.Divide);
			defaults[(int)HotkeyAction.FlyPlane] = Tuple.Create(ModifierKey.CTRL, VirtualKey.Multiply);

			SaveLocation = saveLocation;

			if (!File.Exists(saveLocation))
			{
				SetDefault();
				Save();
			}
			else
			{
				Load();
			}
		}

		public void Set(HotkeyAction action, ModifierKey mod, VirtualKey vk)
		{
			if (!RegisterHotKey(hwnd, (int)action, (uint)mod, (uint)vk))
				return;

			Thread.Sleep(10);

			Tuple<ModifierKey, VirtualKey> key = Tuple.Create(mod, vk);

			Bindings[(uint)action] = Tuple.Create(mod, vk);
		}

		public void Unset(HotkeyAction action)
		{
			if (Bindings[(uint)action] != null)
				Bindings[(uint)action] = null;

			UnregisterHotKey(hwnd, (int)action);
		}

		public void UnregisterAll()
		{
			for (int i = 0; i < NumActions; i++)
				UnregisterHotKey(hwnd, (int)i);
		}

		public void ReregisterAll()
		{
			for (int i = 0; i < NumActions; i++)
			{
				if (Bindings[i] == null)
					continue;
				RegisterHotKey(hwnd, (int)i, (uint)Bindings[i].Item1, (uint)Bindings[i].Item2);
			}
		}

		public void Save()
		{
			StreamWriter w = new StreamWriter(SaveLocation, false);
			for (int i = 0; i < NumActions; i++)
			{
				if (Bindings[i] == null)
				{
					w.WriteLine();
				}
				else
				{
					w.Write(((uint)Bindings[i].Item1).ToString() + ' ');
					w.WriteLine(((uint)Bindings[i].Item2).ToString());
				}
			}

			w.Close();
		}

		public void Load()
		{
			StreamReader r = new StreamReader(File.Open(SaveLocation, FileMode.Open));
			bool error = false, write = false;
			ModifierKey mod;
			VirtualKey key;

			for (uint i = 0; i < NumActions; i++)
			{
				try
				{
					string str = r.ReadLine();
					if (str == "")
						continue;
					if (str == null)
					{
						mod = defaults[i].Item1;
						key = defaults[i].Item2;
						write = true;
					}
					else
					{
						mod = (ModifierKey)Convert.ToUInt32(str.Split(' ')[0]);
						key = (VirtualKey)Convert.ToUInt32(str.Split(' ')[1]);
					}
				}
				catch
				{
					error = true;
					if (defaults[i] != null)
					{
						mod = defaults[i].Item1;
						key = defaults[i].Item2;
					}
					else
					{
						continue;
					}
				}

				Set((HotkeyAction)i, mod, key);
			}

			r.Close();

			if (write)
				Save();

			if (error)
			{
				if (File.Exists(SaveLocation + ".bak"))
					File.Delete(SaveLocation + ".bak");
				File.Move(SaveLocation, SaveLocation + ".bak");
				Save();
				MessageBox.Show(string.Format("Some hotkeys ({0}) could not be loaded and have been reset to their default setting! A backup has been created.", SaveLocation), "Keymap corrupted", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		public void SetDefault()
		{
			for (int i = 0; i < NumActions; i++)
			{
				if (Bindings[i] != null)
					Unset((HotkeyAction)i);
				if (defaults[i] != null)
					Set((HotkeyAction)i, defaults[i].Item1, defaults[i].Item2);
			}
		}

		private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			const int WM_HOTKEY = 0x0312;
			if (msg == WM_HOTKEY)
			{
				ModifierKey mod = (ModifierKey)((uint)lParam & 0xFFFF);
				VirtualKey vk = (VirtualKey)(((uint)lParam >> 16) & 0xFFFF);

				HotkeyPressed.Invoke(new HotkeyEventArgs { key = vk, mod = mod, action = (HotkeyAction)wParam.ToInt32() });
				handled = true;
			}
			return IntPtr.Zero;
		}

		public static string ModToString(ModifierKey mod)
		{
			string str = "";
			if (((uint)mod & 2) != 0)
			{
				str += ModifierKey.CTRL.ToString();
				str += "+";
			}
			if (((uint)mod & 4) != 0)
			{
				str += ModifierKey.SHIFT.ToString();
				str += "+";
			}
			if (((uint)mod & 1) != 0)
			{
				str += ModifierKey.ALT.ToString();
				str += "+";
			}
			return str;
		}
	}

	public class HotkeyEventArgs : EventArgs
	{
		public VirtualKey key;
		public ModifierKey mod;
		public HotkeyAction action;
	}

	public enum HotkeyAction : uint
	{
		Pause = 0,
		SoloSession = 1,
		ResupplyBunker = 2,
		ResupplyCocaine = 3,
		ResupplyMeth = 4,
		ResupplyCounterfeit = 5,
		SellBunker = 6,
		SellCocaine = 7,
		SellMeth = 8,
		SellCounterfeit = 9,
		KillProcess = 10,
		AFK = 11,
		ResupplyAcid = 12,
		SellAcid = 13,
		FlyHeli = 14,
		FlyPlane = 15,
	}

	public enum ModifierKey : uint
	{
		NONE = 0x0000,
		ALT = 0x0001,
		CTRL = 0x0002,
		SHIFT = 0x0004,
		WIN = 0x0008,
	}

	public enum VirtualKey : uint
	{
		LeftButton = 0x01,
		RightButton = 0x02,
		Cancel = 0x03,
		MiddleButton = 0x04,
		ExtraButton1 = 0x05,
		ExtraButton2 = 0x06,
		Back = 0x08,
		Tab = 0x09,
		Clear = 0x0C,
		Return = 0x0D,
		Shift = 0x10,
		Control = 0x11,
		Menu = 0x12,
		Pause = 0x13,
		CapsLock = 0x14,
		Kana = 0x15,
		Hangeul = 0x15,
		Hangul = 0x15,
		Junja = 0x17,
		Final = 0x18,
		Hanja = 0x19,
		Kanji = 0x19,
		Escape = 0x1B,
		Convert = 0x1C,
		NonConvert = 0x1D,
		Accept = 0x1E,
		ModeChange = 0x1F,
		Space = 0x20,
		Prior = 0x21,
		Next = 0x22,
		End = 0x23,
		Home = 0x24,
		Left = 0x25,
		Up = 0x26,
		Right = 0x27,
		Down = 0x28,
		Select = 0x29,
		Print = 0x2A,
		Execute = 0x2B,
		Snapshot = 0x2C,
		Insert = 0x2D,
		Delete = 0x2E,
		Help = 0x2F,
		D0 = 0x30,
		D1 = 0x31,
		D2 = 0x32,
		D3 = 0x33,
		D4 = 0x34,
		D5 = 0x35,
		D6 = 0x36,
		D7 = 0x37,
		D8 = 0x38,
		D9 = 0x39,
		A = 0x41,
		B = 0x42,
		C = 0x43,
		D = 0x44,
		E = 0x45,
		F = 0x46,
		G = 0x47,
		H = 0x48,
		I = 0x49,
		J = 0x4A,
		K = 0x4B,
		L = 0x4C,
		M = 0x4D,
		N = 0x4E,
		O = 0x4F,
		P = 0x50,
		Q = 0x51,
		R = 0x52,
		S = 0x53,
		T = 0x54,
		U = 0x55,
		V = 0x56,
		W = 0x57,
		X = 0x58,
		Y = 0x59,
		Z = 0x5A,
		LeftWindows = 0x5B,
		RightWindows = 0x5C,
		Application = 0x5D,
		Sleep = 0x5F,
		NumPad0 = 0x60,
		NumPad1 = 0x61,
		NumPad2 = 0x62,
		NumPad3 = 0x63,
		NumPad4 = 0x64,
		NumPad5 = 0x65,
		NumPad6 = 0x66,
		NumPad7 = 0x67,
		NumPad8 = 0x68,
		NumPad9 = 0x69,
		Multiply = 0x6A,
		Add = 0x6B,
		Separator = 0x6C,
		Subtract = 0x6D,
		Decimal = 0x6E,
		Divide = 0x6F,
		F1 = 0x70,
		F2 = 0x71,
		F3 = 0x72,
		F4 = 0x73,
		F5 = 0x74,
		F6 = 0x75,
		F7 = 0x76,
		F8 = 0x77,
		F9 = 0x78,
		F10 = 0x79,
		F11 = 0x7A,
		F12 = 0x7B,
		F13 = 0x7C,
		F14 = 0x7D,
		F15 = 0x7E,
		F16 = 0x7F,
		F17 = 0x80,
		F18 = 0x81,
		F19 = 0x82,
		F20 = 0x83,
		F21 = 0x84,
		F22 = 0x85,
		F23 = 0x86,
		F24 = 0x87,
		NumLock = 0x90,
		ScrollLock = 0x91,
		NEC_Equal = 0x92,
		Fujitsu_Jisho = 0x92,
		Fujitsu_Masshou = 0x93,
		Fujitsu_Touroku = 0x94,
		Fujitsu_Loya = 0x95,
		Fujitsu_Roya = 0x96,
		LeftShift = 0xA0,
		RightShift = 0xA1,
		LeftControl = 0xA2,
		RightControl = 0xA3,
		LeftMenu = 0xA4,
		RightMenu = 0xA5,
		BrowserBack = 0xA6,
		BrowserForward = 0xA7,
		BrowserRefresh = 0xA8,
		BrowserStop = 0xA9,
		BrowserSearch = 0xAA,
		BrowserFavorites = 0xAB,
		BrowserHome = 0xAC,
		VolumeMute = 0xAD,
		VolumeDown = 0xAE,
		VolumeUp = 0xAF,
		MediaNextTrack = 0xB0,
		MediaPrevTrack = 0xB1,
		MediaStop = 0xB2,
		MediaPlayPause = 0xB3,
		LaunchMail = 0xB4,
		LaunchMediaSelect = 0xB5,
		LaunchApplication1 = 0xB6,
		LaunchApplication2 = 0xB7,
		OEM1 = 0xBA,
		OEMPlus = 0xBB,
		OEMComma = 0xBC,
		OEMMinus = 0xBD,
		OEMPeriod = 0xBE,
		OEM2 = 0xBF,
		OEM3 = 0xC0,
		OEM4 = 0xDB,
		OEM5 = 0xDC,
		OEM6 = 0xDD,
		OEM7 = 0xDE,
		OEM8 = 0xDF,
		OEMAX = 0xE1,
		OEM102 = 0xE2,
		ICOHelp = 0xE3,
		ICO00 = 0xE4,
		ProcessKey = 0xE5,
		ICOClear = 0xE6,
		Packet = 0xE7,
		OEMReset = 0xE9,
		OEMJump = 0xEA,
		OEMPA1 = 0xEB,
		OEMPA2 = 0xEC,
		OEMPA3 = 0xED,
		OEMWSCtrl = 0xEE,
		OEMCUSel = 0xEF,
		OEMATTN = 0xF0,
		OEMFinish = 0xF1,
		OEMCopy = 0xF2,
		OEMAuto = 0xF3,
		OEMENLW = 0xF4,
		OEMBackTab = 0xF5,
		ATTN = 0xF6,
		CRSel = 0xF7,
		EXSel = 0xF8,
		EREOF = 0xF9,
		Play = 0xFA,
		Zoom = 0xFB,
		Noname = 0xFC,
		PA1 = 0xFD,
		OEMClear = 0xFE
	}
}

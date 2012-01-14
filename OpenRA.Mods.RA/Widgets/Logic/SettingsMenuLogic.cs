#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.FileFormats.Graphics;
using OpenRA.GameRules;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets.Logic
{
	public class SettingsMenuLogic
	{
		Widget bg;

		public SettingsMenuLogic()
		{
			bg = Ui.Root.GetWidget<BackgroundWidget>("SETTINGS_MENU");
			var tabs = bg.GetWidget<ContainerWidget>("TAB_CONTAINER");

			// Tabs
			tabs.GetWidget<ButtonWidget>("GENERAL").OnClick = () => FlipToTab("GENERAL_PANE");
			tabs.GetWidget<ButtonWidget>("AUDIO").OnClick = () => FlipToTab("AUDIO_PANE");
			tabs.GetWidget<ButtonWidget>("DISPLAY").OnClick = () => FlipToTab("DISPLAY_PANE");
			tabs.GetWidget<ButtonWidget>("KEYS").OnClick = () => FlipToTab("KEYS_PANE");
			tabs.GetWidget<ButtonWidget>("DEBUG").OnClick = () => FlipToTab("DEBUG_PANE");
			FlipToTab("GENERAL_PANE");

			// General
			var general = bg.GetWidget("GENERAL_PANE");

			var name = general.GetWidget<TextFieldWidget>("NAME");
			name.Text = Game.Settings.Player.Name;
			name.OnLoseFocus = () =>
			{
				name.Text = name.Text.Trim();

				if (name.Text.Length == 0)
					name.Text = Game.Settings.Player.Name;
				else
					Game.Settings.Player.Name = name.Text;
			};
			name.OnEnterKey = () => { name.LoseFocus(); return true; };

			var edgescrollCheckbox = general.GetWidget<CheckboxWidget>("EDGE_SCROLL");
			edgescrollCheckbox.IsChecked = () => Game.Settings.Game.ViewportEdgeScroll;
			edgescrollCheckbox.OnClick = () => Game.Settings.Game.ViewportEdgeScroll ^= true;

			var edgeScrollSlider = general.GetWidget<SliderWidget>("EDGE_SCROLL_AMOUNT");
			edgeScrollSlider.Value = Game.Settings.Game.ViewportEdgeScrollStep;
			edgeScrollSlider.OnChange += x => Game.Settings.Game.ViewportEdgeScrollStep = x;

			var inversescroll = general.GetWidget<CheckboxWidget>("INVERSE_SCROLL");
			inversescroll.IsChecked = () => Game.Settings.Game.MouseScroll == MouseScrollType.Inverted;
			inversescroll.OnClick = () => Game.Settings.Game.MouseScroll = (Game.Settings.Game.MouseScroll == MouseScrollType.Inverted) ?
												MouseScrollType.Standard : MouseScrollType.Inverted;

			var teamchatCheckbox = general.GetWidget<CheckboxWidget>("TEAMCHAT_TOGGLE");
			teamchatCheckbox.IsChecked = () => Game.Settings.Game.TeamChatToggle;
			teamchatCheckbox.OnClick = () => Game.Settings.Game.TeamChatToggle ^= true;

			var showShellmapCheckbox = general.GetWidget<CheckboxWidget>("SHOW_SHELLMAP");
			showShellmapCheckbox.IsChecked = () => Game.Settings.Game.ShowShellmap;
			showShellmapCheckbox.OnClick = () => Game.Settings.Game.ShowShellmap ^= true;

			// Audio
			var audio = bg.GetWidget("AUDIO_PANE");

			var soundslider = audio.GetWidget<SliderWidget>("SOUND_VOLUME");
			soundslider.OnChange += x => Sound.SoundVolume = x;
			soundslider.Value = Sound.SoundVolume;

			var musicslider = audio.GetWidget<SliderWidget>("MUSIC_VOLUME");
			musicslider.OnChange += x => Sound.MusicVolume = x;
			musicslider.Value = Sound.MusicVolume;

			// Display
			var display = bg.GetWidget("DISPLAY_PANE");
			var gs = Game.Settings.Graphics;

			var windowModeDropdown = display.GetWidget<DropDownButtonWidget>("MODE_DROPDOWN");
			windowModeDropdown.OnMouseDown = _ => ShowWindowModeDropdown(windowModeDropdown, gs);
			windowModeDropdown.GetText = () => gs.Mode == WindowMode.Windowed ?
				"Windowed" : gs.Mode == WindowMode.Fullscreen ? "Fullscreen" : "Pseudo-Fullscreen";

			display.GetWidget("WINDOW_RESOLUTION").IsVisible = () => gs.Mode == WindowMode.Windowed;
			var windowWidth = display.GetWidget<TextFieldWidget>("WINDOW_WIDTH");
			windowWidth.Text = gs.WindowedSize.X.ToString();

			var windowHeight = display.GetWidget<TextFieldWidget>("WINDOW_HEIGHT");
			windowHeight.Text = gs.WindowedSize.Y.ToString();

			var pixelDoubleCheckbox = display.GetWidget<CheckboxWidget>("PIXELDOUBLE_CHECKBOX");
			pixelDoubleCheckbox.IsChecked = () => gs.PixelDouble;
			pixelDoubleCheckbox.OnClick = () =>
			{
				gs.PixelDouble ^= true;
				Game.viewport.Zoom = gs.PixelDouble ? 2 : 1;
			};

			// Keys
			var keys = bg.GetWidget("KEYS_PANE");

			var keyConfig = Game.Settings.Keys;

			var useClassicMouseStyleCheckbox = keys.GetWidget<CheckboxWidget>("USE_CLASSIC_MOUSE_STYLE_CHECKBOX");
			useClassicMouseStyleCheckbox.IsChecked = () => keyConfig.UseClassicMouseStyle;
			useClassicMouseStyleCheckbox.OnClick = () => keyConfig.UseClassicMouseStyle ^= true;

			var modifierToBuildDropdown = keys.GetWidget<DropDownButtonWidget>("MODIFIERTOBUILD_DROPDOWN");
			modifierToBuildDropdown.OnMouseDown = _
				=> ShowHotkeyModifierDropdown(modifierToBuildDropdown, keyConfig.ModifierToBuild, m => keyConfig.ModifierToBuild = m);
			modifierToBuildDropdown.GetText = ()
				=> keyConfig.ModifierToBuild == Modifiers.None ? "<Hotkey>"
					: keyConfig.ModifierToBuild == Modifiers.Alt ? "Alt + <Hotkey>"
					: "Ctrl + <Hotkey>";

			var modifierToCycleDropdown = keys.GetWidget<DropDownButtonWidget>("MODIFIERTOCYCLE_DROPDOWN");
			modifierToCycleDropdown.OnMouseDown = _
				=> ShowHotkeyModifierDropdown(modifierToCycleDropdown, keyConfig.ModifierToCycle, m => keyConfig.ModifierToCycle = m);
			modifierToCycleDropdown.GetText = ()
				=> keyConfig.ModifierToCycle == Modifiers.None ? "<Hotkey>"
					: keyConfig.ModifierToCycle == Modifiers.Alt ? "Alt + <Hotkey>"
					: "Ctrl + <Hotkey>";

			var modifierToSelectTabDropdown = keys.GetWidget<DropDownButtonWidget>("MODIFIERTOSELECTTAB_DROPDOWN");
			modifierToSelectTabDropdown.OnMouseDown = _
				=> ShowHotkeyModifierDropdown(modifierToSelectTabDropdown, keyConfig.ModifierToSelectTab,
								m => keyConfig.ModifierToSelectTab = m);
			modifierToSelectTabDropdown.GetText = ()
				=> keyConfig.ModifierToSelectTab == Modifiers.None ? "<Hotkey>"
					: keyConfig.ModifierToSelectTab == Modifiers.Alt ? "Alt + <Hotkey>"
					: "Ctrl + <Hotkey>";

			var specialHotkeyList = keys.GetWidget<ScrollPanelWidget>("SPECIALHOTKEY_LIST");

			var specialHotkeyTemplate = specialHotkeyList.GetWidget<ScrollItemWidget>("SPECIALHOTKEY_TEMPLATE");

			var item11 = ScrollItemWidget.Setup(specialHotkeyTemplate, () => false, () => {});
			item11.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Select Defense Tab on Build Palette:";
			SetupKeyBinding( item11.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.DefenseTabKey, k => keyConfig.DefenseTabKey = k );
			specialHotkeyList.AddChild(item11);

			var item12 = ScrollItemWidget.Setup(specialHotkeyTemplate, () => false, () => {});
			item12.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Move Viewport to Base:";
			SetupKeyBinding( item12.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.FocusBaseKey, k => keyConfig.FocusBaseKey = k );
			specialHotkeyList.AddChild(item12);

			var item13 = ScrollItemWidget.Setup(specialHotkeyTemplate, () => false, () => {});
			item13.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Switch to Sell-Cursor:";
			SetupKeyBinding( item13.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.SellKey, k => keyConfig.SellKey = k );
			specialHotkeyList.AddChild(item13);

			var item14 = ScrollItemWidget.Setup(specialHotkeyTemplate, () => false, () => {});
			item14.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Switch to Power-Down-Cursor:";
			SetupKeyBinding( item14.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.PowerDownKey, k => keyConfig.PowerDownKey = k );
			specialHotkeyList.AddChild(item14);

			var item15 = ScrollItemWidget.Setup(specialHotkeyTemplate, () => false, () => {});
			item15.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Switch to Repair-Cursor:";
			SetupKeyBinding( item15.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.RepairKey, k => keyConfig.RepairKey = k );
			specialHotkeyList.AddChild(item15);

			var item16 = ScrollItemWidget.Setup(specialHotkeyTemplate, () => false, () => {});
			item16.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Place Normal-Building:";
			SetupKeyBinding( item16.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.PlaceNormalBuildingKey, k => keyConfig.PlaceNormalBuildingKey = k );
			specialHotkeyList.AddChild(item16);

			var item17 = ScrollItemWidget.Setup(specialHotkeyTemplate, () => false, () => {});
			item17.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Place Defense-Building:";
			SetupKeyBinding( item17.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.PlaceDefenseBuildingKey, k => keyConfig.PlaceDefenseBuildingKey = k );
			specialHotkeyList.AddChild(item17);

			var unitCommandHotkeyList = keys.GetWidget<ScrollPanelWidget>("UNITCOMMANDHOTKEY_LIST");

			var unitCommandHotkeyTemplate = unitCommandHotkeyList.GetWidget<ScrollItemWidget>("UNITCOMMANDHOTKEY_TEMPLATE");

			var item21 = ScrollItemWidget.Setup(unitCommandHotkeyTemplate, () => false, () => {});
			item21.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Attack Move:";
			SetupKeyBinding( item21.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.AttackMoveKey, k => keyConfig.AttackMoveKey = k );
			unitCommandHotkeyList.AddChild(item21);

			var item22 = ScrollItemWidget.Setup(unitCommandHotkeyTemplate, () => false, () => {});
			item22.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Stop:";
			SetupKeyBinding( item22.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.StopKey, k => keyConfig.StopKey = k );
			unitCommandHotkeyList.AddChild(item22);

			var item23 = ScrollItemWidget.Setup(unitCommandHotkeyTemplate, () => false, () => {});
			item23.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Scatter:";
			SetupKeyBinding( item23.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.ScatterKey, k => keyConfig.ScatterKey = k );
			unitCommandHotkeyList.AddChild(item23);

			var item24 = ScrollItemWidget.Setup(unitCommandHotkeyTemplate, () => false, () => {});
			item24.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Cycle Stance:";
			SetupKeyBinding( item24.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.StanceCycleKey, k => keyConfig.StanceCycleKey = k );
			unitCommandHotkeyList.AddChild(item24);

			var item25 = ScrollItemWidget.Setup(unitCommandHotkeyTemplate, () => false, () => {});
			item25.GetWidget<LabelWidget>("FUNCTION").GetText = () => "Deploy:";
			SetupKeyBinding( item25.GetWidget<TextFieldWidget>("HOTKEY"),
			() => keyConfig.DeployKey, k => keyConfig.DeployKey = k );
			unitCommandHotkeyList.AddChild(item25);

			// Debug
			var debug = bg.GetWidget("DEBUG_PANE");

			var perfgraphCheckbox = debug.GetWidget<CheckboxWidget>("PERFDEBUG_CHECKBOX");
			perfgraphCheckbox.IsChecked = () => Game.Settings.Debug.PerfGraph;
			perfgraphCheckbox.OnClick = () => Game.Settings.Debug.PerfGraph ^= true;

			var checkunsyncedCheckbox = debug.GetWidget<CheckboxWidget>("CHECKUNSYNCED_CHECKBOX");
			checkunsyncedCheckbox.IsChecked = () => Game.Settings.Debug.SanityCheckUnsyncedCode;
			checkunsyncedCheckbox.OnClick = () => Game.Settings.Debug.SanityCheckUnsyncedCode ^= true;

			bg.GetWidget<ButtonWidget>("BUTTON_CLOSE").OnClick = () =>
			{
				int x, y;
				int.TryParse(windowWidth.Text, out x);
				int.TryParse(windowHeight.Text, out y);
				gs.WindowedSize = new int2(x,y);
				Game.Settings.Save();
				Ui.CloseWindow();
			};
		}

		string open = null;

		bool FlipToTab(string id)
		{
			if (open != null)
				bg.GetWidget(open).Visible = false;

			open = id;
			bg.GetWidget(open).Visible = true;
			return true;
		}

		public static bool ShowWindowModeDropdown(DropDownButtonWidget dropdown, GraphicSettings s)
		{
			var options = new Dictionary<string, WindowMode>()
			{
				{ "Pseudo-Fullscreen", WindowMode.PseudoFullscreen },
				{ "Fullscreen", WindowMode.Fullscreen },
				{ "Windowed", WindowMode.Windowed },
			};

			Func<string, ScrollItemWidget, ScrollItemWidget> setupItem = (o, itemTemplate) =>
			{
				var item = ScrollItemWidget.Setup(itemTemplate,
					() => s.Mode == options[o],
					() => s.Mode = options[o]);
				item.GetWidget<LabelWidget>("LABEL").GetText = () => o;
				return item;
			};

			dropdown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 500, options.Keys, setupItem);
			return true;
		}

		public static bool ShowHotkeyModifierDropdown(DropDownButtonWidget dropdown, Modifiers m, Action<Modifiers> am)
		{
			var options = new Dictionary<string, Modifiers>()
			{
				{ "<Hotkey>", Modifiers.None },
				{ "Alt + <Hotkey>", Modifiers.Alt  },
				{ "Ctrl + <Hotkey>", Modifiers.Ctrl },
			};

			Func<string, ScrollItemWidget, ScrollItemWidget> setupItem = (o, itemTemplate) =>
			{
				var item = ScrollItemWidget.Setup(itemTemplate,
					() => m == options[o],
					() => am(options[o]));
				item.GetWidget<LabelWidget>("LABEL").GetText = () => o;
				return item;
			};

			dropdown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 500, options.Keys.ToList(), setupItem);
			return true;
		}

		void SetupKeyBinding(TextFieldWidget textBox, Func<string> getValue, Action<string> setValue)
		{
			textBox.Text = getValue();

			textBox.OnLoseFocus = () =>
			{
				textBox.Text.Trim();
				if (textBox.Text.Length == 0)
				textBox.Text = getValue();
				else
				setValue(textBox.Text);
			};

			textBox.OnEnterKey = () => { textBox.LoseFocus(); return true; };
		}
	}
}

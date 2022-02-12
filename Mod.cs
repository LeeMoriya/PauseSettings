using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using Menu;
using Music;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;

//Remove PublicityStunt requirement
//--------------------------------------------------------------------------------------
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
        public string AssemblyName { get; }
    }
}
//--------------------------------------------------------------------------------------

public class PauseSettings : PartialityMod
{
    // Update URL - don't touch!
    // You can go to this in a browser (it's safe), but you might not understand the result.
    // This URL is specific to this mod, and identifies it on AUDB.
    public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/4/2";
    // Version - increase this by 1 when you upload a new version of the mod.
    // The first upload should be with version 0, the next version 1, the next version 2, etc.
    // If you ever lose track of the version you're meant to be using, ask Pastebin.
    public int version = 1;
    // Public key in base64 - don't touch!
    public string keyE = "AQAB";
    public string keyN = "lDaM5h0hJUvZcIdiWXH4qfdia/V8UWzikqRIiC9jVGA87jMrafo4EWOTk0MMIQZWHVy+msVzvEAVR3V45wZShFu7ylUndroL5u4zyqHfVeAeDIALfBrM3J4BIM1rMi4wieYdLIF6t2Uj4GVH7iU59AIfobew1vICUILu9Zib/Aw2QY6Nc+0Cz6Lw3xh7DL/trIMaW7yQfYRZUaEZBHelN2JGyUjKkbby4vL6gySfGlVl1OH0hYYhrhNwnQrOow8WXFMIu/WyTA3cY3wqkjd4/WRJ+EvYtMKTwfG+TZiHGst9Bg1ZTFfvEvrTFiPadTf19iUnfyL/QJaTAD8qe+rba5KwirIElovqFpYNH9tAr7SpjixjbT3Igmz+SlqGa9wSbm1QWt/76QqpyAYV/b5G/VzbytoZrhkEVdGuaotD4tXh462AhK5xoigB8PEt+T3nWuPdoZlVo5hRCxoNleH4yxLpVv8C7TpQgQHDqzHMcEX79xjiYiCvigCq7lLEdxUD0fhnxSYVK0O+y7T+NXkk3is/XqJxdesgyYUMT81MSou9Ur/2nv9H8IvA9QeIqso05hK3c496UOaRJS27WJhrxABtU+HHtxo9SifmXjisDj3IV46uTeVp5bivDTu1yBymgnU8qli/xmwWxKvOisi9ZOZsg4vFHaY31gdUBWOz4dU=";
    // ------------------------------------------------

    public static SettingsContainer settingsContainer;
    public static Vector2 sliderPos = new Vector2();
    public static RainWorldGame pauseGame;
    public static bool pause = false;
    public static bool musicMuted = false;
    public static bool sfxMuted = false;
    public static float musVol;
    public static float sfxVol;
    public PauseSettings()
    {
        this.ModID = "PauseSettings";
        this.Version = "1.1";
        this.author = "LeeMoriya";
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PSSpriteLoader.LoadSprites();
        Hook();
    }
    public static void Hook()
    {
        On.Menu.PauseMenu.ctor += PauseMenu_ctor;
        On.Menu.PauseMenu.Singal += PauseMenu_Singal;
        On.Menu.PauseMenu.ShutDownProcess += PauseMenu_ShutDownProcess;
    }

    private static void PauseMenu_ShutDownProcess(On.Menu.PauseMenu.orig_ShutDownProcess orig, PauseMenu self)
    {
        orig.Invoke(self);
        if (settingsContainer != null)
        {
            settingsContainer = null;
        }
    }

    private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, PauseMenu self, MenuObject sender, string message)
    {
        orig.Invoke(self, sender, message);
        if (message == "SETTINGS")
        {
            if (settingsContainer == null)
            {
                settingsContainer = new SettingsContainer(self, self.pages[0], new Vector2(240f, -5f), new Vector2());
                self.pages[0].subObjects.Add(settingsContainer);
                self.PlaySound(SoundID.MENU_Player_Join_Game);
            }
            else
            {
                settingsContainer.RemoveSprites();
                self.pages[0].RemoveSubObject(settingsContainer);
                self.PlaySound(SoundID.MENU_MultipleChoice_Clicked);
                settingsContainer = null;
            }
        }
    }

    private static void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, PauseMenu self, ProcessManager manager, RainWorldGame game)
    {
        orig.Invoke(self, manager, game);
        SymbolButton button = new SymbolButton(self, self.pages[0], "Menu_Symbol_Show_List", "SETTINGS", new Vector2(self.exitButton.pos.x - 40f, self.exitButton.pos.y + 2f));
        sliderPos = button.pos + new Vector2(-700f, 0f);
        self.pages[0].subObjects.Add(button);
    }
}

public class SettingsContainer : RectangularMenuObject, Slider.ISliderOwner
{
    public HorizontalSlider sfxSlider;
    public HorizontalSlider musicSlider;
    public MuteButton musicMuteButton;
    public MuteButton sfxMuteButton;
    public SimpleButton controlsButton;

    public SettingsContainer(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base(menu, owner, pos, size)
    {
        sfxSlider = new HorizontalSlider(menu, this, "Sound Effects", new Vector2(PauseSettings.sliderPos.x, 20f), new Vector2(100f, 0f), Slider.SliderID.SfxVol, false);
        this.subObjects.Add(sfxSlider);
        musicSlider = new HorizontalSlider(menu, this, "Music", new Vector2(sfxSlider.pos.x + 270f, 20f), new Vector2(100f, 0f), Slider.SliderID.MusicVol, false);
        this.subObjects.Add(musicSlider);
        musicMuteButton = new MuteButton(menu, this, "Futile_White", "MUTEMUSIC", new Vector2(musicSlider.pos.x - 35.01f, 20f), new Vector2(30f, 30f));
        musicMuteButton.muted = PauseSettings.musicMuted;
        this.subObjects.Add(musicMuteButton);
        sfxMuteButton = new MuteButton(menu, this, "Futile_White", "MUTESFX", new Vector2(sfxSlider.pos.x - 35.01f, 20f), new Vector2(30f, 30f));
        sfxMuteButton.muted = PauseSettings.sfxMuted;
        this.subObjects.Add(sfxMuteButton);
    }

    public override void Update()
    {
        base.Update();
        if (PauseSettings.musicMuted)
        {
            this.musicSlider.buttonBehav.greyedOut = true;
            this.menu.manager.rainWorld.options.musicVolume = 0.00001f;
        }
        else
        {
            this.musicSlider.buttonBehav.greyedOut = false;
        }
        if (PauseSettings.sfxMuted)
        {
            this.sfxSlider.buttonBehav.greyedOut = true;
            this.menu.manager.rainWorld.options.soundEffectsVolume = 0.00001f;
        }
        else
        {
            this.sfxSlider.buttonBehav.greyedOut = false;
        }
    }

    public override void Singal(MenuObject sender, string message)
    {
        base.Singal(sender, message);
        if(message == "MUTEMUSIC")
        {
            if (PauseSettings.musicMuted)
            {
                this.musicSlider.floatValue = PauseSettings.musVol;
                PauseSettings.musicMuted = false;
                (sender as MuteButton).muted = false;
                menu.PlaySound(SoundID.MENU_Checkbox_Uncheck);
            }
            else
            {
                PauseSettings.musVol = this.musicSlider.floatValue;
                PauseSettings.musicMuted = true;
                (sender as MuteButton).muted = true;
                menu.PlaySound(SoundID.MENU_Checkbox_Check);
            }
        }
        if (message == "MUTESFX")
        {
            if (PauseSettings.sfxMuted)
            {
                this.sfxSlider.floatValue = PauseSettings.sfxVol;
                PauseSettings.sfxMuted = false;
                (sender as MuteButton).muted = false;
                menu.PlaySound(SoundID.MENU_Checkbox_Uncheck);
            }
            else
            {
                PauseSettings.sfxVol = this.sfxSlider.floatValue;
                PauseSettings.sfxMuted = true;
                (sender as MuteButton).muted = true;
                menu.PlaySound(SoundID.MENU_Checkbox_Check);
            }
        }
    }

    public void SliderSetValue(Slider slider, float f)
    {
        switch (slider.ID)
        {
            case Slider.SliderID.SfxVol:
                menu.manager.rainWorld.options.soundEffectsVolume = f;
                PauseSettings.sfxVol = this.sfxSlider.floatValue;
                break;
            case Slider.SliderID.MusicVol:
                if( f == 0) { f = 0.00001f; }
                menu.manager.rainWorld.options.musicVolume = f;
                PauseSettings.musVol = this.musicSlider.floatValue;
                break;
        }
        if (menu.manager.musicPlayer == null)
        {
            menu.manager.musicPlayer = new MusicPlayer(menu.manager);
            menu.manager.sideProcesses.Add(menu.manager.musicPlayer);
        }
    }

    public float ValueOfSlider(Slider slider)
    {
        switch (slider.ID)
        {
            case Slider.SliderID.SfxVol:
                return menu.manager.rainWorld.options.soundEffectsVolume;
            case Slider.SliderID.MusicVol:
                return menu.manager.rainWorld.options.musicVolume;
            default:
                return 0f;
        }
    }
}
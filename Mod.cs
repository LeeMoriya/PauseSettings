using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using Menu;
using Music;

public class PauseSettings : PartialityMod
{
    public static SettingsContainer settingsContainer;
    public static Vector2 sliderPos = new Vector2();
    public PauseSettings()
    {
        this.ModID = "PauseSettings";
        this.Version = "1.0";
        this.author = "LeeMoriya";
    }

    public override void OnEnable()
    {
        base.OnEnable();
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
        if(settingsContainer != null)
        {
            settingsContainer = null;
        }
    }

    private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, PauseMenu self, MenuObject sender, string message)
    {
        orig.Invoke(self, sender, message);
        if(message == "SETTINGS")
        {
            if(settingsContainer == null)
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
        sliderPos = button.pos + new Vector2(-680f, 0f);
        self.pages[0].subObjects.Add(button);
    }
}

public class SettingsContainer : RectangularMenuObject, Slider.ISliderOwner
{
    public HorizontalSlider sfxSlider;
    public HorizontalSlider musicSlider;

    public SettingsContainer(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base(menu, owner, pos, size)
    {
        sfxSlider = new HorizontalSlider(menu, this, "Sound Effects", new Vector2(PauseSettings.sliderPos.x, 20f), new Vector2(100f, 0f), Slider.SliderID.SfxVol, false);
        this.subObjects.Add(sfxSlider);
        musicSlider = new HorizontalSlider(menu, this, "Music", new Vector2(sfxSlider.pos.x + 250f, 20f), new Vector2(100f, 0f), Slider.SliderID.MusicVol, false);
        this.subObjects.Add(musicSlider);
    }

    public void SliderSetValue(Slider slider, float f)
    {
        switch (slider.ID)
        {
            case Slider.SliderID.SfxVol:
                menu.manager.rainWorld.options.soundEffectsVolume = f;
                break;
            case Slider.SliderID.MusicVol:
                menu.manager.rainWorld.options.musicVolume = f;
                break;
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
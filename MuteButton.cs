using UnityEngine;

//Modified version of the SymbolButton that scales to Sprite size and supports custom colors
namespace Menu
{
    public class MuteButton : ButtonTemplate
	{
		public RoundedRect roundedRect;
		public string signalText;
		public FSprite symbolSprite;
		public bool maintainOutlineColorWhenGreyedOut;
		public bool muted;
		public MuteButton(Menu menu, MenuObject owner, string symbolName, string singalText, Vector2 pos, Vector2 size) : base(menu, owner, pos, size)
		{
			this.muted = false;
			this.signalText = singalText;
			this.roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), this.size, true);
			this.subObjects.Add(this.roundedRect);
			this.symbolSprite = new FSprite(symbolName, true);
			this.Container.AddChild(this.symbolSprite);
		}

		public override Color MyColor(float timeStacker)
		{
			if (!this.buttonBehav.greyedOut)
			{
				float num = Mathf.Lerp(this.buttonBehav.lastCol, this.buttonBehav.col, timeStacker);
				num = Mathf.Max(num, Mathf.Lerp(this.buttonBehav.lastFlash, this.buttonBehav.flash, timeStacker));
				HSLColor from = HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.MediumGrey), Menu.MenuColor(Menu.MenuColors.White), num);
				return HSLColor.Lerp(from, Menu.MenuColor(Menu.MenuColors.Black), this.black).rgb;
			}
			if (this.maintainOutlineColorWhenGreyedOut)
			{
				return Menu.MenuRGB(Menu.MenuColors.DarkGrey);
			}
			return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), Menu.MenuColor(Menu.MenuColors.Black), this.black).rgb;
		}

		public override void Update()
		{
			base.Update();
			this.buttonBehav.Update();
			this.roundedRect.addSize = new Vector2(4f, 4f) * (this.buttonBehav.sizeBump + 0.5f * Mathf.Sin(this.buttonBehav.extraSizeBump * 3.1415927f)) * ((!this.buttonBehav.clicked) ? 1f : 0f);

		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			if (muted && this.symbolSprite.element.name != "MuteIcon")
			{
				this.symbolSprite.SetElementByName("MuteIcon");
			}
			if (!muted && this.symbolSprite.element.name != "SpeakerIcon")
			{
				this.symbolSprite.SetElementByName("SpeakerIcon");
			}
			float num = 0.5f - 0.5f * Mathf.Sin(Mathf.Lerp(this.buttonBehav.lastSin, this.buttonBehav.sin, timeStacker) / 30f * 3.1415927f * 2f);
			num *= this.buttonBehav.sizeBump;
			this.symbolSprite.color = this.muted ? Menu.MenuRGB(Menu.MenuColors.DarkGrey) : Menu.MenuRGB(Menu.MenuColors.White);
			this.symbolSprite.x = this.DrawX(timeStacker) + base.DrawSize(timeStacker).x / 2f;
			this.symbolSprite.y = this.DrawY(timeStacker) + base.DrawSize(timeStacker).y / 2f;
			Color color = this.muted ? Menu.MenuRGB(Menu.MenuColors.Black) : Menu.MenuRGB(Menu.MenuColors.White);
			for (int i = 0; i < 9; i++)
			{
				this.roundedRect.sprites[i].color = color;
			}
		}

		public void UpdateSymbol(string newSymbolName)
		{
			this.symbolSprite.element = Futile.atlasManager.GetElementWithName(newSymbolName);
		}

		public override void RemoveSprites()
		{
			this.symbolSprite.RemoveFromContainer();
			base.RemoveSprites();
		}

		public override void Clicked()
		{
			this.Singal(this, this.signalText);
		}
	}
}
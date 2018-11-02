﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DbgMenus
{
    public class DbgMenuItemResolutionChange : DbgMenuItem
    {
        private List<DisplayMode> supportedModes;
        private int modeIndex = 0;

        public DbgMenuItemResolutionChange()
        {
            supportedModes = Main.GetAllResolutions();
            modeIndex = supportedModes.IndexOf(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
            UpdateText();
        }

        private void UpdateText()
        {
            Text = $"Display Mode: {supportedModes[modeIndex].Width}x{supportedModes[modeIndex].Height} " +
                $"({GetSurfaceFormatFriendlyName(supportedModes[modeIndex].Format)})";
        }

        public override void OnIncrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = modeIndex;
            modeIndex += incrementAmount;

            //If upper bound reached
            if (modeIndex >= supportedModes.Count)
            {
                //If already at end and just tapped button
                if (prevIndex == supportedModes.Count - 1 && !isRepeat)
                    modeIndex = 0; //Wrap Around
                else
                    modeIndex = supportedModes.Count - 1; //Stop
            }

            GFX.Display.Mode = supportedModes[modeIndex];

            UpdateText();
        }

        public override void OnDecrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = modeIndex;
            modeIndex -= incrementAmount;

            //If upper bound reached
            if (modeIndex < 0)
            {
                //If already at end and just tapped button
                if (prevIndex == 0 && !isRepeat)
                    modeIndex = supportedModes.Count - 1; //Wrap Around
                else
                    modeIndex = 0; //Stop
            }

            GFX.Display.Mode = supportedModes[modeIndex];

            UpdateText();
        }

        public override void OnResetDefault()
        {
            base.OnResetDefault();
        }

        private string GetSurfaceFormatFriendlyName(SurfaceFormat f)
        {
            switch(f)
            {
                default:
                    return f.ToString();
            }
        }
    }
}

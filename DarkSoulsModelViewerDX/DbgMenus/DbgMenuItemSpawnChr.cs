﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DbgMenus
{
    public class DbgMenuItemSpawnChr : DbgMenuItem
    {
        public static List<int> IDList = new List<int>();
        private static bool NeedsTextUpdate = false;

        public int IDIndex = 0;

        public DbgMenuItemSpawnChr()
        {
            UpdateSpawnIDs();
            UpdateText();
        }

        public static void UpdateSpawnIDs()
        {
            var chrFiles = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"\chr\"), @"*.chrbnd")
                .Select(Path.GetFileNameWithoutExtension);
            IDList = new List<int>();
            var IDSet = new HashSet<int>();
            foreach (var cf in chrFiles)
            {
                if (int.TryParse(cf.Substring(1, 4), out int id))
                {
                    IDList.Add(id);
                    IDSet.Add(id);
                }
            }

            var chrFilesDCX = Directory.GetFiles(InterrootLoader.GetInterrootPath(@"\chr\"), @"*.chrbnd.dcx")
                .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension);
            foreach (var cf in chrFilesDCX)
            {
                if (int.TryParse(cf.Substring(1, 4), out int id))
                {
                    if (!IDSet.Contains(id))
                        IDList.Add(id);
                }
            }
            NeedsTextUpdate = true;
        }

        private void UpdateText()
        {
            if (IDList.Count == 0)
            {
                IDIndex = 0;
                Text = $"Click to Spawn CHR [Invalid Data Root Selected]";
            }
            else
            {
                if (IDIndex >= IDList.Count)
                    IDIndex = IDList.Count - 1;

                Text = $"Click to Spawn CHR [ID: <c{IDList[IDIndex]:D4}>]";
            }
        }

        public override void OnIncrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = IDIndex;
            IDIndex += incrementAmount;

            //If upper bound reached
            if (IDIndex >= IDList.Count)
            {
                //If already at end and just tapped button
                if (prevIndex == IDList.Count - 1 && !isRepeat)
                    IDIndex = 0; //Wrap Around
                else
                    IDIndex = IDList.Count - 1; //Stop
            }

            UpdateText();
        }

        public override void OnDecrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = IDIndex;
            IDIndex -= incrementAmount;

            //If upper bound reached
            if (IDIndex < 0)
            {
                //If already at end and just tapped button
                if (prevIndex == 0 && !isRepeat)
                    IDIndex = IDList.Count - 1; //Wrap Around
                else
                    IDIndex = 0; //Stop
            }

            UpdateText();
        }

        public override void OnResetDefault()
        {
            IDIndex = 0;
            UpdateText();
        }

        public override void OnClick()
        {
            GFX.ModelDrawer.AddChr(IDList[IDIndex], GFX.World.GetSpawnPointInFrontOfCamera(distance: 5,
                faceBackwards: false, lockPitch: true, alignToFloor: true));
        }

        public override void UpdateUI()
        {
            if (NeedsTextUpdate)
            {
                UpdateText();
                NeedsTextUpdate = false;
            }
        }

        public override void OnRequestTextRefresh()
        {
            UpdateSpawnIDs();
            UpdateText();
        }
    }

}
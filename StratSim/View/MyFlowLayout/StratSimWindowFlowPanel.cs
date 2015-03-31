using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MyFlowLayout;

namespace StratSim.View.MyFlowLayout
{
    public class StratSimWindowFlowPanel : WindowFlowPanel
    {
        int alexMcCormickBackgroundState;
        int easterEggBackgroundImage;

        public StratSimWindowFlowPanel()
            : base()
        {
            alexMcCormickBackgroundState = 0;
            easterEggBackgroundImage = 0;
        }

        protected override void SetBackground()
        {
            this.BackgroundImage = Properties.Resources.RedBull;
        }

        /// <summary>
        /// Easter egg for changing the background of the panel
        /// </summary>
        internal void AlexMcCormickEasterEgg(Keys k)
        {
            switch (alexMcCormickBackgroundState)
            {
                case 0:
                case 1:
                    {
                        if (k == Keys.Up) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 2:
                case 3:
                    {
                        if (k == Keys.Down) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 4:
                case 6:
                    {
                        if (k == Keys.Left) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 5:
                case 7:
                    {
                        if (k == Keys.Right) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 8:
                    {
                        if (k == Keys.B) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 9:
                    {
                        if (k == Keys.A)
                        {
                            switch (easterEggBackgroundImage)
                            {
                                case 0: this.BackgroundImage = Properties.Resources.MercedesAMG; break;
                                case 1: this.BackgroundImage = Properties.Resources.Ferrari; break;
                                case 2: this.BackgroundImage = Properties.Resources.Lotus; break;
                                case 3: this.BackgroundImage = Properties.Resources.Mclaren; break;
                                case 4: this.BackgroundImage = Properties.Resources.ForceIndia; break;
                                case 5: this.BackgroundImage = Properties.Resources.Sauber; break;
                                case 6: this.BackgroundImage = Properties.Resources.TorroRosso; break;
                                case 7: this.BackgroundImage = Properties.Resources.Williams; break;
                                case 8: this.BackgroundImage = Properties.Resources.Marussia; break;
                                case 9: this.BackgroundImage = Properties.Resources.Caterham; break;
                                case 10: this.BackgroundImage = Properties.Resources.RedBull; break;
                            }
                            easterEggBackgroundImage = (easterEggBackgroundImage + 1) % 11;
                            alexMcCormickBackgroundState = 0;
                        }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
            }

        }

    }
}

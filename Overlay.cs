using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static TarkovQuestScanner.TarkovAPI;

namespace TarkovQuestScanner
{
    public partial class Overlay : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private static readonly int GWL_EXSTYLE = -20;
        private static readonly int WS_EX_TOOLWINDOW = 0x00000080;
        private static readonly int WS_EX_LAYERED = 0x80000;
        private static readonly int WS_EX_TRANSPARENT = 0x20;
        private static bool isinfoform = true;
        
        // Wait text handling
        private static string waitinfForTooltipText = MainForm.languageModel == null ? Program.languageLoading : Program.waitingForTooltip;
        private static int DotsCounter = 0;

        public Overlay(bool _isinfoform)
        {
            InitializeComponent();
            isinfoform = _isinfoform;
            this.TopMost = true;
            var style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            if (isinfoform)
            {
                this.Opacity = Int32.Parse(Program.settings["Overlay_Transparent"]) * 0.01;
                SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT);
            }
            else
            {
                SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TOOLWINDOW);
            }
            settingFormPos();
            
            // Clean up unused panels
            itemcompare_panel.Visible = false;
            iteminfo_panel.Visible = false;
            if(iteminfo_ball != null) iteminfo_ball.Visible = false;
            if(ItemCompareGrid != null) ItemCompareGrid.Visible = false;
        }

        public void settingFormPos()
        {
            this.Location = new Point(0, 0);
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        public void ShowQuestList(string text)
        {
            Action show = delegate ()
            {
                iteminfo_text.Text = text;
                // Position panel near center-right or just below cursor if preferred, 
                // but since this is a result of a scan, maybe top-right or center is better.
                // For now let's keep it dynamic or fixed.
                // Let's put it at mouse position for consistency with previous behavior, 
                // or center screen if mouse is null.
                Point p = Control.MousePosition;
                FixLocation(iteminfo_panel); // Ensure it's on screen
                iteminfo_panel.Visible = true;
                iteminfo_panel.Location = new Point(p.X + 20, p.Y + 20);
                FixLocation(iteminfo_panel);
            };
            Invoke(show);
        }

        public void ShowLoadingInfo(Point point, CancellationToken cts_one)
        {
            Action show = delegate ()
            {
                if (!cts_one.IsCancellationRequested)
                {
                    iteminfo_text.Text = MainForm.languageModel == null ? Program.languageLoading : Program.waitingForTooltip;
                    iteminfo_panel.Location = new Point(point.X + 20, point.Y + 20);
                    iteminfo_panel.Visible = true;
                }
            };
            Invoke(show);
        }

        public void ShowWaitingForTooltipInfo(Point point, CancellationToken cts_one)
        {
            Action show = delegate ()
            {
                if (!cts_one.IsCancellationRequested)
                {
                    if (DotsCounter < 3)
                    {
                        DotsCounter++;
                    }
                    else
                    {
                        DotsCounter = 1;
                    }
                    waitinfForTooltipText = MainForm.languageModel == null ? Program.languageLoading : Program.waitingForTooltip;
                    for (var i = 0; i < DotsCounter; i++)
                    {
                        waitinfForTooltipText += ".";
                    }
                    iteminfo_panel.Location = new Point(point.X + 20, point.Y + 20);
                    iteminfo_text.Text = waitinfForTooltipText;
                    iteminfo_panel.Visible = true;
                }
            };
            Invoke(show);
        }

        public void ShowLoadingCompare(Point point, CancellationToken cts_one)
        {
             // Deprecated functionality, empty impl to satisfy MainForm calls if any
        }

        public void HideInfo()
        {
            Action show = delegate ()
            {
                iteminfo_panel.Visible = false;
            };
            Invoke(show);
        }

        public void HideCompare()
        {
            // Deprecated
        }

        public void ChangeTransparent(int value)
        {
            Action show = delegate ()
            {
                this.Opacity = value * 0.01;
            };
            Invoke(show);
        }

        private void FixLocation(Control p)
        {
            int totalwidth = p.Location.X + p.Width;
            int totalheight = p.Location.Y + p.Height;
            int x = p.Location.X;
            int y = p.Location.Y;
            if (totalwidth > this.Width)
            {
                x -= totalwidth - this.Width;
            }
            if (totalheight > this.Height)
            {
                y -= totalheight - this.Height;
            }
            if (x != p.Location.X || y != p.Location.Y)
            {
                p.Location = new Point(x, y);
            }
            p.Refresh();
        }

        private void itemwindow_panel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, (sender as Control).ClientRectangle, Color.White, ButtonBorderStyle.Solid);
        }

        private void itemwindow_panel_SizeChanged(object sender, EventArgs e)
        {
            FixLocation(sender as Control);
        }

        private void itemwindow_panel_LocationChanged(object sender, EventArgs e)
        {
            FixLocation(sender as Control);
        }

        private void Overlay_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void itemwindow_text_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            (sender as Control).ClientSize = new Size(e.NewRectangle.Width + 1, e.NewRectangle.Height + 1);
        }

        public static StringBuilder RemoveTrailingLineBreaks(StringBuilder input)
        {
            while (input.Length > 0 && input[input.Length - 1] == '\n')
            {
                input.Remove(input.Length - 1, 1);
            }

            return input;
        }
    }
}
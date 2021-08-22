using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Words
{
    public class groupboxHighlighterColor_UI : GroupBox
    {
        ck_RichTextBox rtxCK = null;
        List<panelColorSelection> lstPnlColor = new List<panelColorSelection>();

        Ck_Objects.classLabelButton btnOk = new Ck_Objects.classLabelButton();

        public groupboxHighlighterColor_UI(ref ck_RichTextBox rtxCK)
        {
            this.rtxCK = rtxCK;
            Width = 350;

            Text = "Highlighter Color Sequence";

            Controls.Add(btnOk);
            btnOk.Text = "Ok";
            btnOk.AutoSize = true;
            btnOk.Click += BtnOk_Click;

            VisibleChanged += GroupboxHighlighterColor_UI_VisibleChanged;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void GroupboxHighlighterColor_UI_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible) return;

            placeObjects();

            Location = new Point((rtxCK.Width - Width) / 2,
                                 (rtxCK.Height - Height) / 2);
        }

        void placeObjects()
        {
            if (rtxCK == null) return;
            if (lstPnlColor.Count != rtxCK.clrHighlighter.Length )
            {
                while(lstPnlColor.Count >0)
                {
                    panelColorSelection pnl = lstPnlColor[0];
                    lstPnlColor.Remove(pnl);
                    Controls.Remove(pnl);
                }
            }

            if (lstPnlColor.Count ==0)
            {
                for (int intColorCounter = 0; intColorCounter < rtxCK.clrHighlighter.Length; intColorCounter++)
                {
                    ck_RichTextBox.classHighlighterColorItem clrItem = rtxCK.clrHighlighter[intColorCounter];
                    panelColorSelection pnlNew = new panelColorSelection(ref clrItem);
                    Controls.Add(pnlNew);
                    lstPnlColor.Add(pnlNew);
                }
            }

            int intPanelHeight = 25;
            Point ptTL = new Point(5, 20);
            for (int intPanelCounter = 0; intPanelCounter < lstPnlColor.Count; intPanelCounter++)
            {
                panelColorSelection pnlTemp = lstPnlColor[intPanelCounter];
                pnlTemp.Size = new Size(Width - 10, intPanelHeight);
                pnlTemp.Location = ptTL;
                ptTL.Y = pnlTemp.Bottom;
            }

            btnOk.Location = new Point(Width - 5 - btnOk.Width, ptTL.Y);
            Height = btnOk.Bottom +5;
        }


        public class panelColorSelection : Panel
        {
            TextBox txt = new TextBox();
            CheckBox chx = new CheckBox();

            ck_RichTextBox.classHighlighterColorItem _cColorItem = null;
            public ck_RichTextBox.classHighlighterColorItem cColorItem { get { return _cColorItem; } }

            public panelColorSelection(ref ck_RichTextBox.classHighlighterColorItem cColorItem)
            {
                if (cColorItem == null) return;


                _cColorItem = cColorItem;

                Controls.Add(chx);
                chx.Size = new Size(18, 18);
                chx.Checked = cColorItem.valid;
                chx.CheckedChanged += Chx_CheckedChanged;

                Controls.Add(txt);
                txt.Text = cColorItem.Text;
                txt.BackColor = cColorItem.clrBack;
                txt.ForeColor = cColorItem.clrFore;
                txt.TextChanged += Txt_TextChanged;
                txt.ContextMenu = new ContextMenu();
                txt.ContextMenu.MenuItems.Add(new MenuItem("Edit Back Color", mnuEditBackColor_Click));
                txt.ContextMenu.MenuItems.Add(new MenuItem("Edit Fore Color", mnuEditForeColor_Click));

                SizeChanged += PanelColorSelection_SizeChanged;
            }
            void mnuEditBackColor_Click(object sender, EventArgs e)
            {
                ColorDialog cd = new ColorDialog();
                cd.Color = cColorItem.clrBack;
                if (cd.ShowDialog() == DialogResult.OK)
                    cColorItem.clrBack 
                        = txt.BackColor
                        = cd.Color;
            }
            void mnuEditForeColor_Click(object sender, EventArgs e)
            {
                ColorDialog cd = new ColorDialog();
                cd.Color = cColorItem.clrFore;
                if (cd.ShowDialog() == DialogResult.OK)
                    cColorItem.clrFore 
                        = txt.ForeColor 
                        = cd.Color;
            }

            private void Chx_CheckedChanged(object sender, EventArgs e)
            {
                cColorItem.valid = chx.Checked;
            }

            private void Txt_TextChanged(object sender, EventArgs e)
            {
                cColorItem.Text = txt.Text;
            }

            private void PanelColorSelection_SizeChanged(object sender, EventArgs e)
            {
                placeObjects();
            }

            void placeObjects()
            {
                chx.Location = new Point(3, 3);
                txt.Location = new Point(chx.Right, chx.Top);
                txt.Width = Width - txt.Left - chx.Left;
            }
        }
    }
}

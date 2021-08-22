using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using CSRegisterHotkey;

namespace Words
{
    public class ck_RichTextBox : Panel
    {
        public enum enuLanguages { English, _numLanguages };

        #region ObjectsAndVariableDeclarations
        const float fltSubScriptSizeFactor = .7f;

        public RichTextBox rtx = new RichTextBox();
        public panelToolBar ToolBar = null;
        panelRuler Ruler = null;

        MenuItem mnuFontBold = new MenuItem();
        MenuItem mnuFontUnderline = new MenuItem();
        MenuItem mnuFontStrikeOut = new MenuItem();
        MenuItem mnuFontItalic = new MenuItem();
        MenuItem mnuShowRuler = new MenuItem();
        MenuItem mnuShowToolBar = new MenuItem();
        MenuItem mnuHighlight = new MenuItem();

        MenuItem mnuAlignment_Left = new MenuItem();
        MenuItem mnuAlignment_Center = new MenuItem();
        MenuItem mnuAlignment_Right = new MenuItem();

        public ContextMenu cMnu = new ContextMenu();
        #endregion
        groupboxHighlighterColor_UI grbHighlighterColor_UI = null;

        Label lblSelectionBackColor = new Label();
        
        RichTextBox rtxHelper = new RichTextBox();
        public const string strAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖÙÚÛÜÝßàáâãäåæçèéêëìíîïðñòóôõöùúûüýÿ";
        public static classKeyboard cKeyboard = new classKeyboard();
        
        const string conGetFirstWordKeepCharIgnoreCode = "keep no char";
        const int conWidth = 700;

        Timer tmrLblHighlighter_Hide = new Timer();
        public ck_RichTextBox()
        {
            ck_RichTextBox cMyRef = this;
            grbHighlighterColor_UI = new groupboxHighlighterColor_UI(ref cMyRef);
            Controls.Add(grbHighlighterColor_UI);
            grbHighlighterColor_UI.Hide();
            grbHighlighterColor_UI.VisibleChanged += GrbHighlighterColor_UI_VisibleChanged;

            ToolBar = new panelToolBar(ref cMyRef);
            Controls.Add(ToolBar);
            Controls.Add(lblSelectionBackColor);
            lblSelectionBackColor.Font = new Font("arial", 12);
            lblSelectionBackColor.Text = "Highlighter";
            lblSelectionBackColor.Size = new Size(400, 20);
            lblSelectionBackColor.TextAlign = ContentAlignment.MiddleRight;
            lblSelectionBackColor.MouseWheel += LblSelectionBackColor_MouseWheel;
            lblSelectionBackColor.MouseClick += LblSelectionBackColor_MouseClick;
            lblSelectionBackColor.MouseEnter += LblSelectionBackColor_MouseEnter;

            tmrLblHighlighter_Hide.Interval = 5000;
            tmrLblHighlighter_Hide.Tick += TmrLblHighlighter_Hide_Tick;

            Ruler = new panelRuler(ref cMyRef);
            Controls.Add(Ruler);

            Controls.Add(rtx);
            rtx.AcceptsTab = true;
            rtx.RightMargin = 1000;
            rtx.ScrollBars = RichTextBoxScrollBars.Both;

            rtx.SelectionChanged += Rtx_SelectionChanged;
            rtx.KeyDown += rtx_KeyDown;
            rtx.KeyUp += Rtx_KeyUp;
            rtx.MouseDown += Rtx_MouseDown;
            rtx.MouseUp += Rtx_MouseUp;
            rtx.MouseClick += Rtx_MouseClick;
            rtx.MouseDoubleClick += Rtx_MouseDoubleClick;
            rtx.MouseEnter += Rtx_MouseEnter;
            rtx.MouseLeave += Rtx_MouseLeave;
            rtx.MouseMove += Rtx_MouseMove;
            rtx.TextChanged += Rtx_TextChanged;
            rtx.GotFocus += Rtx_GotFocus;
            rtx.LostFocus += Rtx_LostFocus;
            
            cMnu.Popup += CMnu_Popup;
            rtx.ContextMenu = cMnu;

            cMnu_Build();

            SizeChanged += event_SizeChanged;
            Disposed += Ck_RichTextBox_Disposed;
        }

        private void LblSelectionBackColor_MouseEnter(object sender, EventArgs e)
        {
            lblSelectionBackColor.Focus();
        }

        void tmrLblHighlighter_Hide_Reset()
        {
            tmrLblHighlighter_Hide.Enabled = false;
            tmrLblHighlighter_Hide.Enabled = true;
        }

        bool bolHighlighterLabel_Hide_Automatically = false;
        public bool HighlighterLabel_Hide_Automatically
        {
            get { return bolHighlighterLabel_Hide_Automatically; }
            set { bolHighlighterLabel_Hide_Automatically = value; }
        }

        private void TmrLblHighlighter_Hide_Tick(object sender, EventArgs e)
        {
            tmrLblHighlighter_Hide.Enabled = false;
            if (HighlighterLabel_Hide_Automatically)
                lblSelectionBackColor.Hide();
        }        

        private void Ck_RichTextBox_Disposed(object sender, EventArgs e)
        {
            if (event_Disposed != null)
                event_Disposed((object)this, new EventArgs());
        }


        #region Properties


        EventHandler _event_Disposed = null;
        public EventHandler event_Disposed
        {
            get { return _event_Disposed; }
            set { _event_Disposed = value; }
        }


        EventHandler _eventFile_Load = null;
        public EventHandler File_Load
        {
            get { return _eventFile_Load; }
            set { _eventFile_Load = value; }
        }
        EventHandler _eventFile_Save = null;
        public EventHandler File_Save
        {
            get { return _eventFile_Save; }
            set { _eventFile_Save = value; }
        }
        EventHandler _eventFile_SaveAs = null;
        public EventHandler File_SaveAs
        {
            get { return _eventFile_SaveAs; }
            set { _eventFile_SaveAs = value; }
        }
        EventHandler _eventFile_New = null;
        public EventHandler File_New
        {
            get { return _eventFile_New; }
            set { _eventFile_New = value; }
        }

        public string Language
        {
            get { return ToolBar.Language; }
            set { ToolBar.Language = value; }
        }


        bool bolShowToolBar = true;
        public bool ShowToolBar
        {
            get { return bolShowToolBar; }
            set
            {
                bolShowToolBar = value;
                placeObjects();
            }
        }

        bool bolToolTip_Enabled = true;
        public bool ToolTip_Enabled
        {
            get { return bolToolTip_Enabled; }
            set
            {
                bolToolTip_Enabled
                    = ToolBar.ToolTip_Enabled
                    = value;
            }
        }

        bool bolShowRuler = true;
        public bool ShowRuler
        {
            get { return bolShowRuler; }
            set
            {
                bolShowRuler = value;
                placeObjects();
            }
        }

        #region RTX_Property_Reflections

        string strPathAndFilename = "";
        public string PathAndFilename
        {
            get { return strPathAndFilename; }
            set { strPathAndFilename = value; }
        }

        public override string Text
        {
            get { return rtx.Text; }
            set { rtx.Text = value; }
        }

        public int SelectionStart
        {
            get { return rtx.SelectionStart; }
            set { rtx.SelectionStart = value; }
        }

        public int SelectionLength
        {
            get { return rtx.SelectionLength; }
            set { rtx.SelectionLength = value; }
        }

        public string SelectedText
        {
            get { return rtx.SelectedText; }
            set { rtx.SelectedText = value; }
        }

        public Font SelectionFont
        {
            get { return rtx.SelectionFont; }
            set { rtx.SelectionFont = value; }
        }

        public int SelectionCharOffset
        {
            get { return rtx.SelectionCharOffset; }
            set { rtx.SelectionCharOffset = value; }
        }

        public Color SelectionBackColor
        {
            get { return rtx.SelectionBackColor; }
            set { rtx.SelectionBackColor = value; }
        }

        public Color SelectionForeColor
        {
            get { return rtx.SelectionColor; }
            set { rtx.SelectionColor = value; }
        }

        public int RightMargin
        {
            get { return rtx.RightMargin; }
            set { rtx.RightMargin = value; }
        }

        public int SelectionRightIndent
        {
            get { return rtx.SelectionRightIndent; }
            set { rtx.SelectionRightIndent = value; }
        }

        public int SelectionIndent
        {
            get { return rtx.SelectionIndent; }
            set { rtx.SelectionIndent = value; }
        }

        public int SelectionHangingIndent
        {
            get { return rtx.SelectionHangingIndent; }
            set { rtx.SelectionHangingIndent = value; }
        }



        EventHandler _MouseDown = null;
        public EventHandler MouseDown
        {
            get { return _MouseDown; }
            set { _MouseDown = value; }
        }

        EventHandler _SelectionChanged = null;
        public EventHandler SelectionChanged
        {
            get { return _SelectionChanged; }
            set { _SelectionChanged = value; }
        }


        MouseEventHandler _MouseUp = null;
        public MouseEventHandler MouseUp
        {
            get { return _MouseUp; }
            set { _MouseUp = value; }
        }


        MouseEventHandler _MouseClick = null;
        public MouseEventHandler MouseClick
        {
            get { return _MouseClick; }
            set { _MouseClick = value; }
        }


        MouseEventHandler _MouseDoubleClick = null;
        public MouseEventHandler MouseDoubleClick
        {
            get { return _MouseDoubleClick; }
            set { _MouseDoubleClick = value; }
        }


        MouseEventHandler _MouseMove = null;
        public MouseEventHandler MouseMove
        {
            get { return _MouseMove; }
            set { _MouseMove = value; }
        }



        EventHandler _MouseEnter = null;
        public EventHandler MouseEnter
        {
            get { return _MouseEnter; }
            set { _MouseEnter = value; }
        }


        EventHandler _MouseLeave = null;
        public EventHandler MouseLeave
        {
            get { return _MouseLeave; }
            set { _MouseLeave = value; }
        }


        KeyEventHandler _KeyDown = null;
        public KeyEventHandler KeyDown
        {
            get { return _KeyDown; }
            set { _KeyDown = value; }
        }


        KeyEventHandler _KeyUp = null;
        public KeyEventHandler KeyUp
        {
            get { return _KeyUp; }
            set { _KeyUp = value; }
        }


        List<MenuItem> _lstMnuParent = new List<MenuItem>();
        public List<MenuItem> lstMnuParent
        {
            get { return _lstMnuParent; }
            set { _lstMnuParent = value; }
        }

        #endregion


        #endregion

        #region Methods
        void cMnu_Build()
        {
            cMnu.MenuItems.Clear();

            //                                                      Edit
            MenuItem mnuEdit = new MenuItem("Edit");
            {
                MenuItem mnuEdit_SelectAll = new MenuItem("Select All", mnuEdit_SelectAll_Click, Shortcut.CtrlA);
                mnuEdit.MenuItems.Add(mnuEdit_SelectAll);

                MenuItem mnuEdit_Copy = new MenuItem("Copy", mnuEdit_Copy_Click, Shortcut.CtrlC);
                mnuEdit.MenuItems.Add(mnuEdit_Copy);

                MenuItem mnuEdit_Cut = new MenuItem("Cut", mnuEdit_Cut_Click, Shortcut.CtrlX);
                mnuEdit.MenuItems.Add(mnuEdit_Cut);

                MenuItem mnuEdit_Paste = new MenuItem("Paste", mnuEdit_Paste_Click, Shortcut.CtrlV);
                mnuEdit.MenuItems.Add(mnuEdit_Paste);

                mnuEdit.MenuItems.Add(new MenuItem("insert image", mnuInsertImage_Click));
            }
            cMnu.MenuItems.Add(mnuEdit);

            //                                                      Font
            MenuItem mnuFont = new MenuItem("Font");
            {
                mnuFontBold = new MenuItem("Bold", mnuFont_Bold_click, Shortcut.CtrlB);
                mnuFont.MenuItems.Add(mnuFontBold);
                mnuFontBold.Checked = rtx.SelectionFont != null && (rtx.SelectionFont.Style & FontStyle.Bold) != 0;

                mnuFontUnderline = new MenuItem("Underline", mnuFont_Underline_click, Shortcut.CtrlU);
                mnuFont.MenuItems.Add(mnuFontUnderline);
                mnuFontUnderline.Checked = rtx.SelectionFont != null && (rtx.SelectionFont.Style & FontStyle.Underline) != 0;

                mnuFontStrikeOut = new MenuItem("StrikeOut", mnuFont_StrikeOut_click, Shortcut.CtrlK);
                mnuFont.MenuItems.Add(mnuFontStrikeOut);
                mnuFontStrikeOut.Checked = rtx.SelectionFont != null && (rtx.SelectionFont.Style & FontStyle.Strikeout) != 0;

                mnuFontItalic = new MenuItem("Italic", mnuFont_Italic_click, Shortcut.CtrlI);
                mnuFont.MenuItems.Add(mnuFontItalic);
                mnuFontItalic.Checked = rtx.SelectionFont != null && (rtx.SelectionFont.Style & FontStyle.Italic) != 0;

                mnuFont.MenuItems.Add("Regular", mnuFont_Regular_click);
                mnuFont.MenuItems.Add(new MenuItem("Font Settings", mnuFont_Click, Shortcut.CtrlShiftF));
            }
            cMnu.MenuItems.Add(mnuFont);

            //                                                      Colors
            MenuItem mnuColor = new MenuItem("Color");
            {
                mnuColor.MenuItems.Add(new MenuItem("ForeColor", mnuForeColor_Click));
                mnuColor.MenuItems.Add(new MenuItem("BackColor", mnuBackColor_Click));
            }
            cMnu.MenuItems.Add(mnuColor);

            //                                                      Vertical Position
            MenuItem mnuVerticalPosition = new MenuItem("Vertical Position");
            {
                mnuVerticalPosition.MenuItems.Add(new MenuItem("SuperScript", mnuSuperScript_Click));
                mnuVerticalPosition.MenuItems.Add(new MenuItem("SubScript", mnuSubScript_Click));
                mnuVerticalPosition.MenuItems.Add(new MenuItem("normal", mnuNormalScript_Click));
            }
            cMnu.MenuItems.Add(mnuVerticalPosition);

            //                                                      Alignment
            MenuItem mnuAlignment = new MenuItem("Alignment");
            {
                mnuAlignment_Left = new MenuItem("Left", mnuAlignment_Left_Click, Shortcut.CtrlL);
                mnuAlignment.MenuItems.Add(mnuAlignment_Left);
                mnuAlignment_Left.Checked = (rtx.SelectionAlignment == HorizontalAlignment.Left);

                mnuAlignment_Center = new MenuItem("Center", mnuAlignment_Center_Click, Shortcut.CtrlE);
                mnuAlignment.MenuItems.Add(mnuAlignment_Center);
                mnuAlignment_Center.Checked = (rtx.SelectionAlignment == HorizontalAlignment.Center);

                mnuAlignment_Right = new MenuItem("Right", mnuAlignment_Right_Click, Shortcut.CtrlR);
                mnuAlignment.MenuItems.Add(mnuAlignment_Right);
                mnuAlignment_Right.Checked = (rtx.SelectionAlignment == HorizontalAlignment.Right);
            }
            cMnu.MenuItems.Add(mnuAlignment);

            //                                                      Find
            MenuItem mnuFind = new MenuItem("Find", mnuFind_Click, Shortcut.CtrlF);
            cMnu.MenuItems.Add(mnuFind);

            ////                                                      SpellCheck
            //MenuItem mnuSpellCheck = new MenuItem("Spell Check", mnuSpellCheck_Click, Shortcut.F7);
            //cMnu.MenuItems.Add(mnuSpellCheck);

            //                                                      Replace
            MenuItem mnuReplace = new MenuItem("Replace", mnuReplace_Click, Shortcut.CtrlH);
            cMnu.MenuItems.Add(mnuReplace);


            //                                                      highlight
            mnuHighlight = new MenuItem("Highlight");
            {
                mnuHighlight.MenuItems.Add(new MenuItem("Scroll Down", mnuHighlighter_ScrollDown_Click, Shortcut.CtrlShiftZ));
                mnuHighlight.MenuItems.Add(new MenuItem("Scroll Up", mnuHighlighter_ScrollUp_Click, Shortcut.CtrlShiftA));
                mnuHighlight.MenuItems.Add(new MenuItem("set Background Color", mnuHighlight_SetColorSequence_Click));
                MenuItem mnuHighlight_ToggleAutoHideLabel = new MenuItem("Toggle Auto Hide Label", mnuHighlight_AutoHideToolBarLabel_Click);
                mnuHighlight_ToggleAutoHideLabel.Checked = HighlighterLabel_Hide_Automatically;
                mnuHighlight.MenuItems.Add(mnuHighlight_ToggleAutoHideLabel);
            }
            cMnu.MenuItems.Add(mnuHighlight);

            cMnu.MenuItems.Add(new MenuItem("Word Count", mnuOptions_NumberOfWords_Click));



            for (int intMnuParentCounter = 0; intMnuParentCounter < lstMnuParent.Count; intMnuParentCounter++)
                cMnu.MenuItems.Add(lstMnuParent[intMnuParentCounter]);


            //                                                      ShowRuler
            mnuShowRuler = new MenuItem("Show Ruler", mnuShowRuler_Click);
            mnuShowRuler.Checked = ShowRuler;
            cMnu.MenuItems.Add(mnuShowRuler);

            //                                                      Show ToolBar
            mnuShowToolBar = new MenuItem("Show ToolBar", mnuShowToolBar_Click);
            mnuShowToolBar.Checked = ShowToolBar;
            cMnu.MenuItems.Add(mnuShowToolBar);
        }

        void placeObjects()
        {
            Point ptTL = new Point(0, 0);
            if (ShowToolBar)
            {
                ToolBar.Width = Width;
                ToolBar.placeObjects();
                ToolBar.Location = ptTL;
                ptTL.Y = ToolBar.Bottom;
                ToolBar.Visible = true;
            }
            else
                ToolBar.Visible = false;
            if (ShowRuler)
            {
                Ruler.placeObjects();
                Ruler.Location = ptTL;
                ptTL.Y = Ruler.Bottom;
                Ruler.Visible = true;
            }
            else
                Ruler.Visible = false;

            rtx.Location = ptTL;
            rtx.Height = Height - rtx.Top;
            rtx.Width = Width;
        }

        #region RTX_Method_Reflections
        public void Select(int Start, int Length)
        {
            rtx.Select(Start, Length);
        }

        public void Select() { rtx.Select(); }

        public Point GetPositionFromCharIndex(int index)
        {
            return rtx.GetPositionFromCharIndex(index);
        }
        public void ScrollToCaret()
        {
            rtx.ScrollToCaret();
        }

        public void LoadFile(string path)
        {
            rtx.LoadFile(path);
        }

        public void SaveFile(string path)
        {
            rtx.SaveFile(path);
        }
        #endregion

        #endregion

        #region Events
        private void CMnu_Popup(object sender, EventArgs e)
        {
            cMnu_Build();
        }

        private void event_SizeChanged(object sender, EventArgs e)
        {
            placeObjects();
        }

        void mnuShowRuler_Click(object sender, EventArgs e)
        {
            ShowRuler = !ShowRuler;
        }

        void mnuShowToolBar_Click(object sender, EventArgs e)
        {
            ShowToolBar = !ShowToolBar;
        }

        private void Rtx_GotFocus(object sender, EventArgs e)
        {
            if (formFindReplace.instance != null && !formFindReplace.instance.IsDisposed)
                formFindReplace.instance.ckRTX = this;
        }

        private void Rtx_LostFocus(object sender, EventArgs e)
        {
            
        }

        private void Rtx_TextChanged(object sender, EventArgs e)
        {
            if (formFindReplace.instance != null && !formFindReplace.instance.IsDisposed)
                formFindReplace.instance.Search_Reset();
        }

        private void Rtx_SelectionChanged(object sender, EventArgs e)
        {
            ToolBar.SetToSelectedText();
            Ruler.SetToSelectedText();
            if (SelectionChanged != null)
                SelectionChanged(sender, e);

            lblColor_Draw();
        }

        public void lblColor_Draw()
        {
            Color clrSelectionBack = SelectionBackColor;
            Color clrSelectionFore = SelectionForeColor;

            int intColorItemIndex = Highlighter_ColorSelected(ref rtx);

            if (intColorItemIndex >=0)
            {
                lblSelectionBackColor.BackColor = clrHighlighter[intColorItemIndex].clrBack;
                lblSelectionBackColor.ForeColor = clrHighlighter[intColorItemIndex].clrFore;
                lblSelectionBackColor.Text = clrHighlighter[intColorItemIndex].Text;
            }
            else
            {
                lblSelectionBackColor.BackColor = clrSelectionBack;
                lblSelectionBackColor.ForeColor = clrSelectionFore;
                lblSelectionBackColor.Text = "-unrecognized-";
            }
            lblSelectionBackColor.AutoSize = true;
            lblSelectionBackColor.Location = new Point(rtx.Width - lblSelectionBackColor.Width -4,
                                                       0);
            lblSelectionBackColor.BringToFront();
            lblSelectionBackColor.Show();

            tmrLblHighlighter_Hide_Reset();
        }

        private void Rtx_MouseMove(object sender, MouseEventArgs e)
        {
            formWords.instance.grbNotes.BringToFront();

            if (MouseMove != null) MouseMove(sender, e);
        }

        private void Rtx_MouseLeave(object sender, EventArgs e)
        {
            if (MouseLeave != null) MouseLeave(sender, e);
        }

        private void Rtx_MouseEnter(object sender, EventArgs e)
        {
            if (MouseEnter != null) MouseEnter(sender, e);
        }

        private void Rtx_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MouseDoubleClick != null) MouseDoubleClick(sender, e);
        }

        private void Rtx_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseClick != null) MouseClick(sender, e);
        }

        private void Rtx_MouseUp(object sender, MouseEventArgs e)
        {
            if (MouseUp != null) MouseUp(sender, e);
        }

        private void Rtx_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseDown != null) MouseDown(sender, e);
        }

        private void Rtx_KeyUp(object sender, KeyEventArgs e)
        {
           System.Windows.Forms.RichTextBox rtxSender = (System.Windows.Forms.RichTextBox)sender;
            if (cKeyboard.HandleKeyPress(sender, e))
            {

            }
        }

        private void rtx_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.B:
                        mnuFont_Bold_click((object)this, new EventArgs());
                        break;

                    case Keys.I:
                        mnuFont_Italic_click((object)this, new EventArgs());
                        break;

                    case Keys.U:
                        mnuFont_Underline_click((object)this, new EventArgs());
                        break;

                    case Keys.K:
                        mnuFont_StrikeOut_click((object)this, new EventArgs());
                        break;

                    case Keys.H:
                        mnuFind_Click(sender, e);
                        break;

                    case Keys.F:
                        mnuReplace_Click(sender, e);
                        break;

                    case Keys.C:
                        mnuEdit_Copy_Click((object)this, new EventArgs());
                        break;

                    case Keys.X:
                        mnuEdit_Cut_Click((object)this, new EventArgs());
                        break;

                    case Keys.V:
                        mnuEdit_Paste_Click((object)this, new EventArgs());
                        break;
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.F)
                mnuFont_Click((object)this, new EventArgs());
            else if (formFindReplace.instance != null
                    && !formFindReplace.instance.IsDisposed
                    && formFindReplace.instance.Visible)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        formFindReplace.instance.Hide();
                        break;

                    case Keys.Enter:
                        e.SuppressKeyPress = true;
                        formFindReplace.instance.BtnFindNext_Click((object)this, new EventArgs());
                        break;
                }

            }
        }
        void mnuAlignment_Left_Click(object sender, EventArgs e)
        {
            rtx.SelectionAlignment = HorizontalAlignment.Left;
        }
        void mnuAlignment_Center_Click(object sender, EventArgs e)
        {
            rtx.SelectionAlignment = HorizontalAlignment.Center;
        }
        void mnuAlignment_Right_Click(object sender, EventArgs e)
        {
            rtx.SelectionAlignment = HorizontalAlignment.Right;
        }

        void mnuFind_Click(object sender, EventArgs e)
        {
            ck_RichTextBox cMyRef = this;
            formFindReplace.ShowDialog(formFindReplace.enuMode.Find, ref cMyRef);
        }

        void mnuReplace_Click(object sender, EventArgs e)
        {
            ck_RichTextBox cMyRef = this;
            formFindReplace.ShowDialog(formFindReplace.enuMode.FindReplace, ref cMyRef);
        }

        void mnuNormalScript_Click(object sender, EventArgs e)
        {
            Font fntSelected = rtx.SelectionFont;
            float fltSize = fntSelected.Size / (rtx.SelectionCharOffset == 0 ? 1 : fltSubScriptSizeFactor);
            fntSelected = new Font(fntSelected.Name, fltSize, fntSelected.Style);
            rtx.SelectionCharOffset = 0;
            rtx.SelectionFont = fntSelected;
        }

        public void mnuSuperScript_Click(object sender, EventArgs e)
        {
            if (rtx.SelectionStart >= 0)
            {
                Font fntPrevious = rtx.SelectionFont;
                int intOffSet = 0;
                float fltSize = 1f;

                Font fntSelected = rtx.SelectionFont;

                if (rtx.SelectionCharOffset > 0)
                {  // was superScript => make regular
                    intOffSet = 0;
                    fltSize = fntSelected.Size / fltSubScriptSizeFactor;

                }
                else if (rtx.SelectionCharOffset < 0)
                { // was subscript => make Superscript
                    intOffSet = (int)(fntPrevious.Height * .4);
                    fltSize = fntSelected.Size;
                }
                else
                { // rtx.SelectionCharOffset  == 0
                    // was regular => make Superscript
                    intOffSet = (int)(fntPrevious.Height * .4);
                    fltSize = fntSelected.Size * fltSubScriptSizeFactor;
                }

                fntSelected = new Font(fntSelected.Name, fltSize, fntSelected.Style);
                rtx.SelectionCharOffset = intOffSet;
                rtx.SelectionFont = fntSelected;
            }
            else
                rtx.SelectionCharOffset = 10;
            ToolBar.SetToSelectedText();
        }

        public void mnuFontIncrease_Click(object sender, EventArgs e)
        {
           
            if (rtx.SelectionStart >= 0 && rtx.SelectionFont != null)
            {
                Font fntPrevious = rtx.SelectionFont;
                rtx.SelectionFont = FontSize_Change(fntPrevious, 1);
            }
        }
        public void mnuFontDecrease_Click(object sender, EventArgs e)
        {
            if (rtx.SelectionStart >= 0 && rtx.SelectionFont != null )
            {
                Font fntPrevious = rtx.SelectionFont;
                rtx.SelectionFont = FontSize_Change(fntPrevious, -1);
            }
        }

        Font FontSize_Change(Font fntStart, int intDelta)
        {
            Font fntRetVal = new Font(fntStart.FontFamily.Name, fntStart.Size + intDelta, fntStart.Style);
            return fntRetVal;
        }

        public void mnuFile_Load_Click(object sender, EventArgs e)
        {
            if (File_Load != null)
            {
                File_Load(sender, e);
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "rich text files | *.rtf";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                PathAndFilename = ofd.FileName;
                rtx.LoadFile(PathAndFilename);
            }
        }
        public void mnuFile_Save_Click(object sender, EventArgs e)
        {
            if (File_Save != null)
            {
                File_Save(sender, e);
                return;
            }

            if (PathAndFilename.Length == 0)
                mnuFile_SaveAs_Click(sender, e);
            rtx.SaveFile(PathAndFilename); ;
        }

        public void mnuFile_SaveAs_Click(object sender, EventArgs e)
        {
            if (File_SaveAs != null)
            {
                File_SaveAs(sender, e);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "rich text files | *.rtf";
            sfd.DefaultExt = "rtf";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                PathAndFilename = sfd.FileName;

                rtx.SaveFile(sfd.FileName);
            }
        }


        public void mnuFile_New_Click(object sender, EventArgs e)
        {
            if (File_New != null)
            {
                File_New(sender, e);
                return;
            }
             
            rtx.Text = "";
            rtx.SelectionRightIndent = 50;
            rtx.RightMargin = 1100;
            PathAndFilename = System.IO.Directory.GetCurrentDirectory() + "\\default FileName.rtf";
        }

        public void mnuSubScript_Click(object sender, EventArgs e)
        {
            if (rtx.SelectionStart >= 0)
            {
                Font fntPrevious = rtx.SelectionFont;
                int intOffSet = 0;
                float fltSize = 1f;

                Font fntSelected = rtx.SelectionFont;

                if (rtx.SelectionCharOffset > 0)
                {  // was superScript => make subscript
                    intOffSet = -(int)(fntPrevious.Height * .4);
                    fltSize = fntSelected.Size;
                }
                else if (rtx.SelectionCharOffset < 0)
                { // was subscript => make regular
                    intOffSet = 0;
                    fltSize = fntSelected.Size / fltSubScriptSizeFactor;
                }
                else
                { 
                    // was regular => make subscript
                    intOffSet = -(int)(fntPrevious.Height * .4);
                    fltSize = fntSelected.Size * fltSubScriptSizeFactor;
                }

                fntSelected = new Font(fntSelected.Name, fltSize, fntSelected.Style);
                rtx.SelectionCharOffset = intOffSet;
                rtx.SelectionFont = fntSelected;
            }
            else
                rtx.SelectionCharOffset = 10;
            ToolBar.SetToSelectedText();
        }

        void mnuForeColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
                rtx.SelectionColor = cd.Color;
        }

        void mnuBackColor_Click(object sender, EventArgs e)
        {
            if (rtx.SelectedText.Length >= 0)
            {
                ColorDialog cd = new ColorDialog();
                if (cd.ShowDialog() == DialogResult.OK)
                    rtx.SelectionBackColor = cd.Color;
            }
        }

        void mnuEdit_SelectAll_Click(object sender, EventArgs e)
        {
            rtx.SelectAll();
        }

        void mnuEdit_Copy_Click(object sender, EventArgs e)
        {
            if (rtx.SelectedText.Length > 0)
            {
                string strTextFormatting = rtx.SelectedRtf;
                Clipboard.SetText(strTextFormatting, TextDataFormat.Rtf);                
            }
        }

        void mnuEdit_Cut_Click(object sender, EventArgs e)
        {
            if (rtx.SelectedText.Length == 0) return;
            {
                string strTextFormatting = rtx.SelectedRtf;
                Clipboard.SetText(strTextFormatting, TextDataFormat.Rtf);
                rtx.SelectedRtf = "";
            }
        }

        void mnuEdit_Paste_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsData(DataFormats.Rtf))
                    rtx.SelectedRtf = Clipboard.GetText(TextDataFormat.Rtf);
                else
                    rtx.SelectedText = Clipboard.GetText();

            }
            catch (Exception)
            {
                
            }

        }
        #region Highlighter
        bool Color_Compare(Color clr1, Color clr2) { return clr1.A == clr2.A && clr1.R == clr2.R && clr1.G == clr2.G && clr1.B == clr2.B; }

        public classHighlighterColorItem[] clrHighlighter =
                                                            {
                                                                new classHighlighterColorItem(Color.White, Color.Black, "Zero"),
                                                                new classHighlighterColorItem(Color.LightYellow, Color.Black, "One"),
                                                                new classHighlighterColorItem(Color.Beige, Color.Black, "Two"),
                                                                new classHighlighterColorItem(Color.Yellow, Color.Black, "Three"),
                                                                new classHighlighterColorItem(Color.Orange, Color.Black, "Four"),
                                                                new classHighlighterColorItem(Color.Pink, Color.Black, "Five"),
                                                                new classHighlighterColorItem(Color.DeepPink, Color.Black, "Six"),
                                                                new classHighlighterColorItem(Color.Red, Color.Black, "Seven"),
                                                                new classHighlighterColorItem(Color.DarkRed, Color.Black, "Eight")
                                                            };

        
        private void GrbHighlighterColor_UI_VisibleChanged(object sender, EventArgs e)
        {
            grbHighlighterColor_UI.Location = new Point((Width - grbHighlighterColor_UI.Width) / 2,
                                                        (Height - grbHighlighterColor_UI.Height) / 2);
        }

        private void LblSelectionBackColor_MouseClick(object sender, MouseEventArgs e)
        {
            grbHighlighterColor_UI.Show();
            grbHighlighterColor_UI.BringToFront();
        }

        void mnuHighlight_AutoHideToolBarLabel_Click(object sender, EventArgs e)
        {
            HighlighterLabel_Hide_Automatically = !HighlighterLabel_Hide_Automatically;
        }

        void mnuHighlight_SetColorSequence_Click(object sender, EventArgs e)
        {
            grbHighlighterColor_UI.Show();
        }

        int Highlighter_ColorSelected(ref RichTextBox rtxColor)
        {
            Color clrSelectedBack = rtxColor.SelectionBackColor;

            for (int intColorCounter = 0; intColorCounter < clrHighlighter.Length; intColorCounter++)
            {
                classHighlighterColorItem cClrItem = clrHighlighter[intColorCounter];
                if (Color_Compare(clrSelectedBack, cClrItem.clrBack))
                    if (cClrItem.valid)
                        return intColorCounter;
            }
            return -1;
        }

        classHighlighterColorItem Highlighter_ReturnHighest(int intIndexStart)
        {
            if (intIndexStart >= clrHighlighter.Length)
                intIndexStart = clrHighlighter.Length - 1;
            for (int intCounter = intIndexStart; intCounter >= 0; intCounter--)
            {
                classHighlighterColorItem clrTest = clrHighlighter[intCounter];
                if (clrTest.valid)
                    return clrTest;
            }
            return null;
        }

        classHighlighterColorItem Highlighter_ReturnLowest(int intIndexStart)
        {
            if (intIndexStart < 0)
                intIndexStart = 0;
            for (int intCount = intIndexStart; intCount < clrHighlighter.Length; intCount++)
            {
                classHighlighterColorItem clrTest = clrHighlighter[intCount];
                if (clrTest.valid)
                    return clrTest;
            }
            return null;
        }

        classHighlighterColorItem Highlighter_Color_ScrollDown(ref RichTextBox rtxColor)
        {
            int intIndex = Highlighter_ColorSelected(ref rtxColor);

            if (intIndex <= 0)
            {
                // set to Max
                return Highlighter_ReturnHighest(clrHighlighter.Length);
            }
            else
            {
                int intIndex_ScrollDown = intIndex - 1;
                return Highlighter_ReturnHighest(intIndex_ScrollDown);
            }
        }

        classHighlighterColorItem Highlighter_Color_ScrollUp(ref RichTextBox rtxColor)
        {
            int intIndex = Highlighter_ColorSelected(ref rtxColor);

            if (intIndex < 0)
            {
                // set to Min
                return Highlighter_ReturnLowest(0);
            }
            else
            {
                int intIndex_ScrollUp = (intIndex + 1) % clrHighlighter.Length;
                return Highlighter_ReturnLowest(intIndex_ScrollUp);
            }
        }


        public void mnuHighlighter_ScrollUp_Click(object sender, EventArgs e)
        {
            if (SelectionFont == null) return;

            if (SelectionLength == 0)
            {
                string strInsert = "[ ] ";
                int intSelectionStart = rtx.SelectionStart;
                Font fnt = new Font(rtx.SelectionFont.FontFamily, rtx.SelectionFont.Size, FontStyle.Bold);

                rtx.SelectedText = strInsert;
                rtx.SelectionStart = intSelectionStart;
                rtx.SelectionLength = strInsert.Length;
                rtx.SelectionFont = fnt;
                classHighlighterColorItem clrMax = clrHighlighter[clrHighlighter.Length - 1];
                rtx.SelectionBackColor = clrMax.clrBack;
                rtx.SelectionColor = clrMax.clrFore;

                rtx.SelectionStart = intSelectionStart + strInsert.Length - 1;
                rtx.SelectionLength = 1;
                rtx.SelectionBackColor = Color.White;

                rtx.SelectionStart = intSelectionStart + 1;
                rtx.SelectionLength = 1;
                lblColor_Draw();
                return;
            }

            classHighlighterColorItem cScrollUp = Highlighter_Color_ScrollUp(ref rtx);

            if (cScrollUp == null)
            {
                rtx.SelectionBackColor = Color.White;
                lblColor_Draw();
                return;
            }
            else
            {
                SelectionBackColor = cScrollUp.clrBack;
                SelectionForeColor = cScrollUp.clrFore;
                lblColor_Draw();
                return;
            }
        }

        public void mnuHighlighter_ScrollDown_Click(object sender, EventArgs e)
        {

            if (SelectionFont == null) return;

            if (SelectionLength == 0)
            {
                string strInsert = "[ ] ";
                int intSelectionStart = rtx.SelectionStart;
                Font fnt = new Font(rtx.SelectionFont.FontFamily, rtx.SelectionFont.Size, FontStyle.Bold);

                rtx.SelectedText = strInsert;
                rtx.SelectionStart = intSelectionStart;
                rtx.SelectionLength = strInsert.Length;
                classHighlighterColorItem cClrMax = clrHighlighter[clrHighlighter.Length - 1];
                rtx.SelectionBackColor = cClrMax.clrBack;
                rtx.SelectionColor = cClrMax.clrFore;

                rtx.SelectionStart = intSelectionStart + strInsert.Length - 1;
                rtx.SelectionLength = 1;
                rtx.SelectionBackColor = Color.White;

                rtx.SelectionStart = intSelectionStart + 1;
                rtx.SelectionLength = 1;
                lblColor_Draw();
                return;
            }

            classHighlighterColorItem cScrollDown = Highlighter_Color_ScrollDown(ref rtx);

            if (cScrollDown == null)
            {
                rtx.SelectionBackColor = Color.White;
                rtx.SelectionColor = Color.Black;
                lblColor_Draw();
                return;
            }
            else
            {
                SelectionBackColor = cScrollDown.clrBack;
                SelectionForeColor  = cScrollDown.clrFore;
                lblColor_Draw();
                return;
            }
        }


        private void LblSelectionBackColor_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                mnuHighlighter_ScrollDown_Click(sender, e);
            else
                mnuHighlighter_ScrollUp_Click(sender, e);
        }

        #endregion

        public void mnuInsertImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image|*.bmp;*.png;*.jpg;*.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap myBitmap = new Bitmap(ofd.FileName);
                // Copy the bitmap to the clipboard.
                Clipboard.SetDataObject(myBitmap);
                // Get the format for the object type.
                DataFormats.Format myFormat = DataFormats.GetFormat(DataFormats.Bitmap);
                // After verifying that the data can be pasted, paste
                if (rtx.CanPaste(myFormat))
                {
                    rtx.Paste(myFormat);
                }
                else
                {
                    MessageBox.Show("The data format that you attempted site" +
                      " is not supportedby this control.");
                }
            }
        }

        public void mnuOptions_NumberOfWords_Click(object sender, EventArgs e)
        {
            List<string> lstWords = getFirstWords(rtx.SelectionLength > 0
                                                                      ? rtx.SelectedText
                                                                      : rtx.Text);
            IEnumerable<string> query = lstWords.Distinct();
            List<string> lstWords_Distince = (List<string>)query.ToList<string>();

            MessageBox.Show("Words : " + lstWords.Count.ToString()
                                + "\r\nDistinct Words : " + lstWords_Distince.Count.ToString(),
                            "Word Count");
        }
        private static string _strNonAlphaCharacters = "";
        private static string strNonAlphaCharacters
        {
            get
            {
                if (_strNonAlphaCharacters.Length == 0)
                {
                    for (int intCounter = 32; intCounter < 256; intCounter++)
                    {
                        char chrNew = (char)intCounter;
                        if (!char.IsLetter(chrNew))
                            _strNonAlphaCharacters += chrNew.ToString();
                    }
                }
                return _strNonAlphaCharacters;
            }
        }
        public static List<string> getFirstWords(string strText)
        {
            List<string> lstRetVal = getFirstWords(strText, -1, conGetFirstWordKeepCharIgnoreCode);
            return lstRetVal;
        }
        private static List<string> getFirstWords(string strText, int intNumberWords, string strKeepChar)

        {
            if (strText == null || strText.Length == 0) return null;

            //strText = " " + strText + " ";
            int intNextIndex = 0;
            int intIndex = 0;
            List<string> lstRetVal = new List<string>();

            string str_Local_NonAlphaCharacters = "";
            if (string.Compare(conGetFirstWordKeepCharIgnoreCode, strKeepChar) == 0)
                str_Local_NonAlphaCharacters = strNonAlphaCharacters;
            else
            {
                char chrRemoveFromRejectedCharList = strKeepChar[0];
                int intRemoveIndex = strNonAlphaCharacters.IndexOf(chrRemoveFromRejectedCharList);
                if (intRemoveIndex >= 0 && intRemoveIndex < strNonAlphaCharacters.Length)
                    str_Local_NonAlphaCharacters = strNonAlphaCharacters.Remove(intRemoveIndex, 1);
            }
            char[] chr_Local_str_Local_NonAlphaCharacters = str_Local_NonAlphaCharacters.ToArray<char>();

            strText = clean_nonAlpha_Ends(strText) + ".";

            intNextIndex = strText.IndexOfAny(chr_Local_str_Local_NonAlphaCharacters, intIndex + 1);
            while (intNextIndex > intIndex
                    && (lstRetVal.Count < intNumberWords || intNumberWords < 0)
                    )
            {
                string strWord = strText.Substring(intIndex, intNextIndex - intIndex).Trim();
                lstRetVal.Add(strWord);
                while (intNextIndex < strText.Length && str_Local_NonAlphaCharacters.Contains(strText[intNextIndex]))
                    intNextIndex++;
                intIndex = intNextIndex;

                intNextIndex = strText.IndexOfAny(chr_Local_str_Local_NonAlphaCharacters, intIndex);
            }
            if (lstRetVal.Count == 0
                && strText.Length > 0)
                lstRetVal.Add(strText);

            return lstRetVal;
        }
        public static string clean_nonAlpha_Ends(string strSource)
        {
            return cleanBack_nonAlpha(cleanFront_nonAlpha(strSource));
        }

        public static string cleanFront_nonAlpha(string strSource)
        {
            while (strSource.Length > 0
                    &&
                 !strAlphabet.Contains(strSource[0]))
            {
                strSource = strSource.Substring(1);
            }
            return strSource;
        }

        public static string cleanBack_nonAlpha(string strSource)
        {
            while (strSource.Length > 0
                    &&
                 !strAlphabet.Contains(strSource[strSource.Length - 1]))
            {
                strSource = strSource.Substring(0, strSource.Length - 1);
            }
            return strSource;
        }

        void mnuFont_Click(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            if (rtx.SelectionFont != null)
            {
                Font fnt = rtx.SelectionFont;
                fd.Font = fnt;
            }
            if (fd.ShowDialog() == DialogResult.OK)
                rtx.SelectionFont = fd.Font;
        }

        void btnAlignment_Left(object sender, EventArgs e)
        {
            rtx.SelectionAlignment = HorizontalAlignment.Left;
        }

        void btnAlignment_Center(object sender, EventArgs e)
        {
            rtx.SelectionAlignment = HorizontalAlignment.Center;
        }

        void btnAlignment_Right(object sender, EventArgs e)
        {
            rtx.SelectionAlignment = HorizontalAlignment.Right;
        }

        public void mnuFont_Bold_click(object sender, EventArgs e)
        {
            Font fntSelection = rtx.SelectionFont;
            FontStyle fntStyle_OLD = fntSelection.Style;
            bool bolStrikeOut = (fntStyle_OLD & FontStyle.Strikeout) != 0;
            bool bolBold = (fntStyle_OLD & FontStyle.Bold) != 0;
            bool bolItalic = (fntStyle_OLD & FontStyle.Italic) != 0;
            bool bolUnderline = (fntStyle_OLD & FontStyle.Underline) != 0;

            bolBold = !bolBold;

            FontStyle fntStyle_New = (bolStrikeOut ? FontStyle.Strikeout : FontStyle.Regular)
                                    | (bolBold ? FontStyle.Bold : FontStyle.Regular)
                                    | (bolItalic ? FontStyle.Italic : FontStyle.Regular)
                                    | (bolUnderline ? FontStyle.Underline : FontStyle.Regular);
            rtx.SelectionFont = new Font(fntSelection.Name, fntSelection.Size, fntStyle_New);
        }

        public void mnuFont_Italic_click(object sender, EventArgs e)
        {
            Font fntSelection = rtx.SelectionFont;
            FontStyle fntStyle_OLD = fntSelection.Style;
            bool bolStrikeOut = (fntStyle_OLD & FontStyle.Strikeout) != 0;
            bool bolBold = (fntStyle_OLD & FontStyle.Bold) != 0;
            bool bolItalic = (fntStyle_OLD & FontStyle.Italic) != 0;
            bool bolUnderline = (fntStyle_OLD & FontStyle.Underline) != 0;

            bolItalic = !bolItalic;

            FontStyle fntStyle_New = (bolStrikeOut ? FontStyle.Strikeout : FontStyle.Regular)
                                    | (bolBold ? FontStyle.Bold : FontStyle.Regular)
                                    | (bolItalic ? FontStyle.Italic : FontStyle.Regular)
                                    | (bolUnderline ? FontStyle.Underline : FontStyle.Regular);
            rtx.SelectionFont = new Font(fntSelection.Name, fntSelection.Size, fntStyle_New);
        }

        public void mnuFont_Underline_click(object sender, EventArgs e)
        {
            Font fntSelection = rtx.SelectionFont;
            FontStyle fntStyle_OLD = fntSelection.Style;
            bool bolStrikeOut = (fntStyle_OLD & FontStyle.Strikeout) != 0;
            bool bolBold = (fntStyle_OLD & FontStyle.Bold) != 0;
            bool bolItalic = (fntStyle_OLD & FontStyle.Italic) != 0;
            bool bolUnderline = (fntStyle_OLD & FontStyle.Underline) != 0;

            bolUnderline = !bolUnderline;

            FontStyle fntStyle_New = (bolStrikeOut ? FontStyle.Strikeout : FontStyle.Regular)
                                    | (bolBold ? FontStyle.Bold : FontStyle.Regular)
                                    | (bolItalic ? FontStyle.Italic : FontStyle.Regular)
                                    | (bolUnderline ? FontStyle.Underline : FontStyle.Regular);
            rtx.SelectionFont = new Font(fntSelection.Name, fntSelection.Size, fntStyle_New);
        }

        public void mnuFont_StrikeOut_click(object sender, EventArgs e)
        {
            Font fntSelection = rtx.SelectionFont;
            FontStyle fntStyle_OLD = fntSelection.Style;
            bool bolStrikeOut = (fntStyle_OLD & FontStyle.Strikeout) != 0;
            bool bolBold = (fntStyle_OLD & FontStyle.Bold) != 0;
            bool bolItalic = (fntStyle_OLD & FontStyle.Italic) != 0;
            bool bolUnderline = (fntStyle_OLD & FontStyle.Underline) != 0;

            bolStrikeOut = !bolStrikeOut;

            FontStyle fntStyle_New = (bolStrikeOut ? FontStyle.Strikeout : FontStyle.Regular)
                                    | (bolBold ? FontStyle.Bold : FontStyle.Regular)
                                    | (bolItalic ? FontStyle.Italic : FontStyle.Regular)
                                    | (bolUnderline ? FontStyle.Underline : FontStyle.Regular);
            rtx.SelectionFont = new Font(fntSelection.Name, fntSelection.Size, fntStyle_New);
        }

        void mnuFont_Regular_click(object sender, EventArgs e)
        {
            Font fntSelection = rtx.SelectionFont;
            rtx.SelectionFont = new Font(fntSelection.Name, fntSelection.Size, FontStyle.Regular);
        }

        public static string getSentenceAtSelection(ref ck_RichTextBox rtxbox) { return getSentenceAtSelection(ref rtxbox, rtxbox.SelectionStart); }
        public static string getSentenceAtSelection(ref ck_RichTextBox rtxbox, int intSelection)
        {
            string strRetVal = "";

            int intStartSentence = intSelection - 1;
            string strPunctuation = ".!?\r\n\t";
            while (intStartSentence >= 0 && strPunctuation.IndexOf(rtxbox.Text[intStartSentence]) < 0) intStartSentence--;

            int intEndSentence = intSelection;
            while (intEndSentence < rtxbox.Text.Length && strPunctuation.IndexOf(rtxbox.Text[intEndSentence]) < 0) intEndSentence++;

            if (intEndSentence > intStartSentence + 1)
            {
                strRetVal = rtxbox.Text.Substring(intStartSentence + 1, intEndSentence - intStartSentence - 1);
            }

            return strRetVal;
        }

        public static bool isAlpha(char chr) { return strAlphabet.Contains(chr); }
        public static string getWordAtSelection(ref ck_RichTextBox rtxbox) { return getWordAtSelection(ref rtxbox, rtxbox.SelectionStart); }
        public static string getWordAtSelection(ref ck_RichTextBox rtxbox, int intSelection)
        {
            string strRetVal = "";

            if (rtxbox.SelectionLength > 0)
                return rtxbox.SelectedText;

            int intStartWord = intSelection - 1;
            while (intStartWord >= 0 && isAlpha(rtxbox.Text[intStartWord])) intStartWord--;


            int intEndWord = intSelection;
            while (intEndWord < rtxbox.Text.Length && isAlpha(rtxbox.Text[intEndWord])) intEndWord++;

            if (intEndWord > intStartWord + 1)
            {
                strRetVal = rtxbox.Text.Substring(intStartWord + 1, intEndWord - intStartWord - 1);
            }

            return strRetVal;
        }

        #endregion

        public class panelToolBar : Panel
        {
            ck_RichTextBox rtx = null;

            public enum enuButtons
            {
                File_New, File_Load, File_Save, File_SaveAs,
                Bold, Italic, Underline, Strikeout, SuperScript, SubScript,
                Font_Increase, Font_Decrease,
                Align_Left, Align_Center, Align_Right,
                SpellCheck, Highlighter,
                Picture_Insert,
                ToolTip_Toggle,
                _numButtons
            };

            public SPObjects.SPContainer SPContainer = new SPObjects.SPContainer("Tool Bar");
            public List<SPObjects.Button> lstButtons = new List<SPObjects.Button>();

            public string Language
            {
                get { return SPContainer.Language; }
                set { SPContainer.Language = value; }
            }

            public panelToolBar(ref ck_RichTextBox ckRTX)
            {
                this.rtx = ckRTX;
                Height = 22;

                Controls.Add(SPContainer);
                SPContainer.Dock = DockStyle.Fill;

                SPContainer.Language = enuLanguages.English.ToString();

                bool bolBuilding = SPContainer.BuildingInProgress;

                SPContainer.Building_Start();
                {
                    for (int intButtonCounter = 0; intButtonCounter < (int)enuButtons._numButtons; intButtonCounter++)
                    {
                        enuButtons eButton = (enuButtons)intButtonCounter;
                        SPObjects.Button btnNew = new SPObjects.Button(ref SPContainer);
                        lstButtons.Add(btnNew);
                        btnNew.Size = szButtons;
                        btnNew.Tag = (object)eButton;
                        btnNew.cEle.Name = eButton.ToString();
                        btnNew.MouseClick = btn_Click;

                        btnNew.cEle.Tip_Set(eButton.ToString(), enuLanguages.English.ToString());
                        switch (eButton)
                        {

                            case enuButtons.File_Load:
                                btnNew.cEle.Tip_Set("File Load", enuLanguages.English.ToString());
                                break;

                            case enuButtons.File_New:
                                btnNew.cEle.Tip_Set("File New", enuLanguages.English.ToString());
                                break;

                            case enuButtons.File_Save:
                                btnNew.cEle.Tip_Set("File Save", enuLanguages.English.ToString());
                                break;

                            case enuButtons.File_SaveAs:
                                btnNew.cEle.Tip_Set("Save As", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Bold:
                                btnNew.cEle.Tip_Set("Bold", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Italic:
                                btnNew.cEle.Tip_Set("Italic", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Underline:
                                btnNew.cEle.Tip_Set("Underline", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Strikeout:
                                btnNew.cEle.Tip_Set("Strikeout", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Font_Decrease:
                                btnNew.cEle.Tip_Set("Font Shrink", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Font_Increase:
                                btnNew.cEle.Tip_Set("Font Grow", enuLanguages.English.ToString());
                                break;

                            case enuButtons.SubScript:
                                btnNew.cEle.Tip_Set("Subscript", enuLanguages.English.ToString());
                                break;

                            case enuButtons.SuperScript:
                                btnNew.cEle.Tip_Set("Superscript", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Align_Left:
                                btnNew.cEle.Tip_Set("Align Left", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Align_Right:
                                btnNew.cEle.Tip_Set("Align Right", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Align_Center:
                                btnNew.cEle.Tip_Set("Align Center", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Highlighter:
                                btnNew.cEle.Tip_Set("Highlighter", enuLanguages.English.ToString());
                                break;

                            case enuButtons.Picture_Insert:
                                btnNew.cEle.Tip_Set("Picture Insert", enuLanguages.English.ToString());
                                break;

                            case enuButtons.ToolTip_Toggle:
                                btnNew.cEle.Tip_Set("ToolTip Toggle", enuLanguages.English.ToString());
                                break;

                            case enuButtons.SpellCheck:
                                btnNew.cEle.Tip_Set("Seiceálaí Litrithe", enuLanguages.English.ToString());
                                break;

                        }
                    }

                    Buttons_Draw();
                }
                SPContainer.Building_Complete();
                SizeChanged += PanelToolBar_SizeChanged;
                BackColorChanged += PanelToolBar_BackColorChanged;
            }

            private void PanelToolBar_BackColorChanged(object sender, EventArgs e)
            {

            }


            #region Properties

            public bool ToolTip_Enabled
            {
                get { return SPContainer.ToolTip_Enabled; }
                set
                {
                    SPContainer.ToolTip_Enabled = value;
                    btnToolTip_Toggle.Toggled = value;
                }
            }


            Size _szButtons = new Size(18, 18);
            Size szButtons
            {
                get { return _szButtons; }
                set { _szButtons = value; }
            }

            SPObjects.Button btnFile_New
            {
                get { return lstButtons[(int)enuButtons.File_New]; }
                set { lstButtons[(int)enuButtons.File_New] = value; }
            }
            SPObjects.Button btnFile_Load
            {
                get { return lstButtons[(int)enuButtons.File_Load]; }
                set { lstButtons[(int)enuButtons.File_Load] = value; }
            }
            SPObjects.Button btnFile_Save
            {
                get { return lstButtons[(int)enuButtons.File_Save]; }
                set { lstButtons[(int)enuButtons.File_Save] = value; }
            }
            SPObjects.Button btnFile_SaveAs
            {
                get { return lstButtons[(int)enuButtons.File_SaveAs]; }
                set { lstButtons[(int)enuButtons.File_SaveAs] = value; }
            }

            SPObjects.Button btnBold
            {
                get { return lstButtons[(int)enuButtons.Bold]; }
                set { lstButtons[(int)enuButtons.Bold] = value; }
            }
            SPObjects.Button btnItalic
            {
                get { return lstButtons[(int)enuButtons.Italic]; }
                set { lstButtons[(int)enuButtons.Italic] = value; }
            }
            SPObjects.Button btnUnderline
            {
                get { return lstButtons[(int)enuButtons.Underline]; }
                set { lstButtons[(int)enuButtons.Underline] = value; }
            }
            SPObjects.Button btnStrikeout
            {
                get { return lstButtons[(int)enuButtons.Strikeout]; }
                set { lstButtons[(int)enuButtons.Strikeout] = value; }
            }
            SPObjects.Button btnSuperScript
            {
                get { return lstButtons[(int)enuButtons.SuperScript]; }
                set { lstButtons[(int)enuButtons.SuperScript] = value; }
            }
            SPObjects.Button btnSubScript
            {
                get { return lstButtons[(int)enuButtons.SubScript]; }
                set { lstButtons[(int)enuButtons.SubScript] = value; }
            }

            SPObjects.Button btnAlign_Left
            {
                get { return lstButtons[(int)enuButtons.Align_Left]; }
                set { lstButtons[(int)enuButtons.Align_Left] = value; }
            }

            SPObjects.Button btnAlign_Center
            {
                get { return lstButtons[(int)enuButtons.Align_Center]; }
                set { lstButtons[(int)enuButtons.Align_Center] = value; }
            }

            SPObjects.Button btnAlign_Right
            {
                get { return lstButtons[(int)enuButtons.Align_Right]; }
                set { lstButtons[(int)enuButtons.Align_Right] = value; }
            }

            SPObjects.Button btnFontIncrease
            {
                get { return lstButtons[(int)enuButtons.Font_Increase]; }
                set { lstButtons[(int)enuButtons.Font_Increase] = value; }
            }
            SPObjects.Button btnFontDecrease
            {
                get { return lstButtons[(int)enuButtons.Font_Decrease]; }
                set { lstButtons[(int)enuButtons.Font_Decrease] = value; }
            }

            SPObjects.Button btnSpellCheck
            {
                get { return lstButtons[(int)enuButtons.SpellCheck]; }
                set { lstButtons[(int)enuButtons.SpellCheck] = value; }
            }

            SPObjects.Button btnHighlighter
            {
                get { return lstButtons[(int)enuButtons.Highlighter]; }
                set { lstButtons[(int)enuButtons.Highlighter] = value; }
            }

            SPObjects.Button btnPicture_Insert
            {
                get { return lstButtons[(int)enuButtons.Picture_Insert]; }
                set { lstButtons[(int)enuButtons.Picture_Insert] = value; }
            }

            public SPObjects.Button btnToolTip_Toggle
            {
                get
                {
                    return lstButtons[(int)enuButtons.ToolTip_Toggle];
                }
                set
                {
                    lstButtons[(int)enuButtons.ToolTip_Toggle] = value;
                }
            }



            System.Drawing.Color _clrButtons_BackColor = System.Drawing.Color.White;
            public System.Drawing.Color clrButtons_BackColor
            {
                get { return _clrButtons_BackColor; }
                set
                {
                    _clrButtons_BackColor = value;
                    Buttons_Draw();
                }
            }

            System.Drawing.Color _clrButtons_ForeColor = System.Drawing.Color.Black;
            public System.Drawing.Color clrButtons_ForeColor
            {
                get { return _clrButtons_ForeColor; }
                set
                {
                    _clrButtons_ForeColor = value;
                    Buttons_Draw();
                }
            }

            #endregion

            #region Methods
            public void SetToSelectedText()
            {
                Font fnt = rtx.SelectionFont;
                if (fnt == null) return;
                bool bolBuilding = SPContainer.BuildingInProgress;
                SPContainer.Building_Start();
                {

                    btnBold.Toggled = (fnt.Style & FontStyle.Bold) != 0;
                    btnItalic.Toggled = (fnt.Style & FontStyle.Italic) != 0;
                    btnUnderline.Toggled = (fnt.Style & FontStyle.Underline) != 0;
                    btnStrikeout.Toggled = (fnt.Style & FontStyle.Strikeout) != 0;
                    btnSuperScript.Toggled = (rtx.SelectionCharOffset > 0);
                    btnSubScript.Toggled = (rtx.SelectionCharOffset < 0);
                }
                if (!bolBuilding)
                    SPContainer.Building_Complete();
            }


            void Buttons_Draw()
            {
                bool bolBuilding = SPContainer.BuildingInProgress;
                Font fnt = new Font("Segoe UI", 72);
                SPContainer.Building_Start();
                {
                    SPContainer.BackColor_Dull = BackColor;
                    for (int intButtonCounter = 0; intButtonCounter < (int)enuButtons._numButtons; intButtonCounter++)
                    {
                        enuButtons eButton = (enuButtons)intButtonCounter;
                        lstButtons[(int)eButton].Size = szButtons;
                        switch (eButton)
                        {
                            case enuButtons.File_New:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_File_New);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_File_New_Highlight);
                                }
                                break;

                            case enuButtons.File_Load:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_File_Load);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_File_Load_Highlight);
                                }
                                break;

                            case enuButtons.File_Save:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_File_Save);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_File_Save_Highlight);
                                }
                                break;

                            case enuButtons.File_SaveAs:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_File_SaveAs);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_File_SaveAs_Highlight);
                                }
                                break;


                            case enuButtons.Bold:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Font_Bold);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Font_Bold_Highlight);
                                    btn.CanBeToggled = true;
                                }
                                break;

                            case enuButtons.Italic:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Font_Italic);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Font_Italic_Highlight);
                                    btn.CanBeToggled = true;
                                }
                                break;

                            case enuButtons.Underline:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Font_Underline);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Font_Underline_Highlight);
                                    btn.CanBeToggled = true;
                                }
                                break;

                            case enuButtons.Strikeout:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Font_Strikeout);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Font_Strikeout_Highlight);
                                    btn.CanBeToggled = true;
                                }
                                break;

                            case enuButtons.SuperScript:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_SuperScript);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_SuperScript_Highlight);
                                    btn.CanBeToggled = true;
                                }
                                break;

                            case enuButtons.SubScript:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_SubScript);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_SubScript_Highlight);
                                    btn.CanBeToggled = true;
                                }
                                break;


                            case enuButtons.Align_Left:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Align_Left);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Align_Left_Highlight);
                                }
                                break;

                            case enuButtons.Align_Center:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Align_Center);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Align_Center_Highlight);
                                }
                                break;

                            case enuButtons.Align_Right:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Align_Right);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Align_Right_Highlight);
                                }
                                break;


                            case enuButtons.Font_Decrease:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_FontDecrease);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_FontDecrease_Highlight);
                                }
                                break;

                            case enuButtons.Font_Increase:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_FontIncrease);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_FontIncrease_Highlight);
                                }
                                break;

                            case enuButtons.SpellCheck:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_SpellChecker);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_SpellChecker_Highlight);
                                }
                                break;

                            case enuButtons.Highlighter:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_HighLighter_Dull);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_HighLighter_Highlight);
                                }
                                break;

                            case enuButtons.Picture_Insert:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Picture_Insert);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_Picture_Insert_Highlight);
                                }
                                break;

                            case enuButtons.ToolTip_Toggle:
                                {
                                    SPObjects.Button btn = lstButtons[(int)eButton];
                                    btn.bmpDull = new Bitmap(Properties.Resources.ck_RichTextBox_Button_ToolTip_Toggle);
                                    btn.bmpHighlight = new Bitmap(Properties.Resources.ck_RichTextBox_Button_ToolTip_Toggle_Highlight);
                                    btn.CanBeToggled = true;
                                }
                                break;
                        }
                    }
                }

                if (!bolBuilding)
                    SPContainer.Building_Complete();
            }

            public void placeObjects()
            {
                Buttons_Draw();
                bool bolBuilding = SPContainer.BuildingInProgress;
                List<classBtnWidths> lstBtns = new List<classBtnWidths>();

                lstBtns.Add(new classBtnWidths());
                lstBtns[0].lstSPBtns.Add(btnToolTip_Toggle);
                lstBtns[0].lstSPBtns.Add(btnHighlighter);
                //lstBtns[0].lstSPBtns.Add(btnSpellCheck);

                lstBtns.Add(new classBtnWidths());
                lstBtns[1].lstSPBtns.Add(btnFile_Load);
                lstBtns[1].lstSPBtns.Add(btnFile_New);
                lstBtns[1].lstSPBtns.Add(btnFile_Save);
                lstBtns[1].lstSPBtns.Add(btnFile_SaveAs);

                lstBtns.Add(new classBtnWidths());
                lstBtns[2].lstSPBtns.Add(btnAlign_Left);
                lstBtns[2].lstSPBtns.Add(btnAlign_Center);
                lstBtns[2].lstSPBtns.Add(btnAlign_Right);

                lstBtns.Add(new classBtnWidths());
                lstBtns[3].lstSPBtns.Add(btnFontDecrease);
                lstBtns[3].lstSPBtns.Add(btnFontIncrease);
                lstBtns[3].lstSPBtns.Add(btnBold);
                lstBtns[3].lstSPBtns.Add(btnItalic);
                lstBtns[3].lstSPBtns.Add(btnUnderline);
                lstBtns[3].lstSPBtns.Add(btnStrikeout);
                lstBtns[3].lstSPBtns.Add(btnSubScript);
                lstBtns[3].lstSPBtns.Add(btnSuperScript);

                lstBtns.Add(new classBtnWidths());
                lstBtns[4].lstSPBtns.Add(btnPicture_Insert);

                for (int intLstCounter = 0; intLstCounter < lstBtns.Count; intLstCounter++)
                {
                    Rectangle rec = new Rectangle();
                    for (int intBtnCounter = 0; intBtnCounter < lstBtns[intLstCounter].lstSPBtns.Count; intBtnCounter++)
                    {
                        SPObjects.Button btn = lstBtns[intLstCounter].lstSPBtns[intBtnCounter];
                        if (btn.Visible)
                        {
                            rec.Width += btn.MyImage.Width;
                            if (rec.Height < btn.MyImage.Height)
                                rec.Height = btn.MyImage.Height;
                        }
                    }
                    lstBtns[intLstCounter].rec = rec;
                }

                SPContainer.Building_Start();
                {
                    // /*
                    Point pt = new Point(0, 0);
                    for (int intLstCounter = 0; intLstCounter < lstBtns.Count; intLstCounter++)
                    {
                        classBtnWidths cBtnLst = lstBtns[intLstCounter];

                        if (pt.X + cBtnLst.rec.Width > SPContainer.pic.Width)
                        {
                            if (intLstCounter > 0)
                            {
                                pt.Y = lstBtns[intLstCounter - 1].rec.Bottom;
                                pt.X = 0;
                            }
                        }

                        cBtnLst.rec.Location = pt;
                        Point ptBtn = pt;
                        for (int intBtnCounter = 0; intBtnCounter < cBtnLst.lstSPBtns.Count; intBtnCounter++)
                        {
                            SPObjects.Button btn = cBtnLst.lstSPBtns[intBtnCounter];
                            if (btn.Visible)
                            {
                                btn.Location = ptBtn;
                                ptBtn.X += btn.Width;
                            }
                        }

                        pt.X += cBtnLst.rec.Width;
                    }

                    if (SPContainer.pic.Height != lstBtns[lstBtns.Count - 1].rec.Bottom)
                    {
                        SPContainer.pic.Height
                            = Height
                            = lstBtns[lstBtns.Count - 1].rec.Bottom;
                    }

                    SPContainer.recVisible
                        = SPContainer.recSPArea
                        = new Rectangle(0, 0, SPContainer.pic.Width, SPContainer.pic.Height);
                    SPContainer.RecVisible_Changed = true;
                    /*/
                    SPContainer.recVisible
                        = SPContainer.recSPArea
                        = new Rectangle(0, 0, SPContainer.pic.Width, SPContainer.pic.Height);
                    SPContainer.RecVisible_Changed = true;

                    btnToolTip_Toggle.Location = new Point(intButtonGap, intButtonGap);

                    btnSpellCheck.Location = new Point(btnToolTip_Toggle.Right + 16*intButtonGap, btnBold.Top);

                    btnFile_New.Location = new Point(btnSpellCheck.Right + 8*intButtonGap, btnBold.Top);
                    btnFile_Load.Location = new Point(btnFile_New.Right, btnFile_New.Top);
                    btnFile_Save.Location = new Point(btnFile_Load.Right, btnFile_New.Top);
                    btnFile_SaveAs.Location = new Point(btnFile_Save.Right, btnFile_New.Top);

                    btnBold.Location = new Point(btnFile_SaveAs.Right + 5 * intButtonGap, btnFile_New.Top);
                    btnItalic.Location = new Point(btnBold.Right, btnBold.Top);
                    btnUnderline.Location = new Point(btnItalic.Right, btnBold.Top);
                    btnStrikeout.Location = new Point(btnUnderline.Right, btnBold.Top);
                    btnFontDecrease.Location = new Point(btnStrikeout.Right, btnBold.Top);
                    btnFontIncrease.Location = new Point(btnFontDecrease.Right, btnBold.Top);

                    btnSuperScript.Location = new Point(btnFontIncrease.Right + 2 * intButtonGap, btnBold.Top);
                    btnSubScript.Location = new Point(btnSuperScript.Right, btnBold.Top);

                    btnAlign_Left.Location = new Point(btnSubScript.Right + 4 * intButtonGap, btnBold.Top);
                    btnAlign_Center.Location = new Point(btnAlign_Left.Right, btnBold.Top);
                    btnAlign_Right.Location = new Point(btnAlign_Center.Right, btnBold.Top);

                    btnHighlighter.Location = new Point(Width - intButtonGap - btnHighlighter.Width, btnBold.Top);
                    // */
                }

                if (!bolBuilding)
                    SPContainer.Building_Complete();
            }

            #endregion

            #region Events

            public class classBtnWidths
            {
                public List<SPObjects.Button> lstSPBtns = new List<SPObjects.Button>();
                public Rectangle rec = new Rectangle();
            }

            private void PanelToolBar_SizeChanged(object sender, EventArgs e)
            {
                Buttons_Draw();
                placeObjects();
            }

         

            void btn_Click(object sender, EventArgs e)
            {
                SPObjects.Button btnSender = (SPObjects.Button)sender;
                enuButtons eButton = (enuButtons)btnSender.Tag;
                if (btnSender.CanBeToggled)
                    btnSender.Toggle();

                switch (eButton)
                {
                    case enuButtons.File_New:
                        rtx.mnuFile_New_Click((object)this, new EventArgs());
                        break;

                    case enuButtons.File_Load:
                        rtx.mnuFile_Load_Click((object)this, new EventArgs());
                        break;

                    case enuButtons.File_Save:
                        rtx.mnuFile_Save_Click((object)this, new EventArgs());
                        break;

                    case enuButtons.File_SaveAs:
                        rtx.mnuFile_SaveAs_Click((object)this, new EventArgs());
                        break;

                    case enuButtons.Bold:
                        rtx.mnuFont_Bold_click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Italic:
                        rtx.mnuFont_Italic_click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Underline:
                        rtx.mnuFont_Underline_click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Strikeout:
                        rtx.mnuFont_StrikeOut_click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.SuperScript:
                        rtx.mnuSuperScript_Click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.SubScript:
                        rtx.mnuSubScript_Click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Font_Decrease:
                        rtx.mnuFontDecrease_Click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Font_Increase:
                        rtx.mnuFontIncrease_Click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Align_Left:
                        rtx.btnAlignment_Left((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Align_Center:
                        rtx.btnAlignment_Center((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Align_Right:
                        rtx.btnAlignment_Right((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.SpellCheck:
                        //rtx.mnuSpellCheck_Click((object)btnSender, new EventArgs());
                        break;

                    case enuButtons.Highlighter:
                        rtx.mnuHighlighter_ScrollDown_Click((object)btnSender, new EventArgs());
                        break;
                    case enuButtons.Picture_Insert:
                        rtx.mnuInsertImage_Click((object)btnSender, new EventArgs());
                        break;

                }
                #endregion
            }
        }


        public class panelRuler : Panel
        {
            enum enuControls { LeftIndent, RightIndent, FirstLineIndent, HangingIndent, _numControls };
            const int conHeight = 23;

            SPObjects.SPContainer SPContainer = new SPObjects.SPContainer("panelRuler");
            SPObjects.Label[] lblControls = new SPObjects.Label[(int)enuControls._numControls];
            System.Windows.Forms.Timer tmrGrab = new System.Windows.Forms.Timer();

            public panelRuler(ref ck_RichTextBox ckRTX)
            {
                _ckRTX = ckRTX;
                Controls.Add(SPContainer);
                SPContainer.Dock = DockStyle.Fill;
                SPContainer.BackgroundImage = bmpRuler;

                SPContainer.Language = enuLanguages.English.ToString();

                for (int intControlCounter = 0; intControlCounter < (int)enuControls._numControls; intControlCounter++)
                {
                    enuControls eControl = (enuControls)intControlCounter;
                    SPObjects.Label lblNew = new SPObjects.Label(ref SPContainer);
                    lblNew.Text = "";
                    lblNew.MouseDown = lblMouse_Down;
                    lblNew.MouseUp = lblMouse_Up;
                    lblNew.cEle.Name = eControl.ToString();
                    lblNew.BlendIntoSPContainer = true;
                    lblNew.Tag = (object)eControl;

                    switch (eControl)
                    {
                        case enuControls.FirstLineIndent:
                            lblNew.BackgroundImage = bmpFirstLineIndent;
                            lblNew.cEle.Tip_Set("First Line Indent", enuLanguages.English.ToString());
                            break;

                        case enuControls.HangingIndent:
                            lblNew.BackgroundImage = bmpHangingIndent;
                            lblNew.cEle.Tip_Set("Hanging Indent", enuLanguages.English.ToString());
                            break;

                        case enuControls.LeftIndent:
                            lblNew.BackgroundImage = bmpLeftIndent;
                            lblNew.cEle.Tip_Set("Left-Indent", enuLanguages.English.ToString());
                            break;

                        case enuControls.RightIndent:
                            lblNew.BackgroundImage = bmpRightIndent;
                            lblNew.cEle.Tip_Set("Right Indent", enuLanguages.English.ToString());
                            break;
                    }
                    lblNew.Size = lblNew.BackgroundImage.Size;
                    lblNew.cEle.Tip_Set(eControl.ToString(), enuLanguages.English.ToString());

                    lblControls[(int)intControlCounter] = lblNew;
                }
                SPContainer.Building_Complete();

                tmrGrab.Interval = 100;
                tmrGrab.Tick += TmrGrab_Tick;
            }


            #region Events

            private void TmrGrab_Tick(object sender, EventArgs e)
            {
                if (MouseButtons != MouseButtons.Left)
                    tmrGrab.Enabled = false;
                SPObjects.Label lblGrab = (SPObjects.Label)tmrGrab.Tag;

                Point ptMouseRel = MouseRelTo(this);

                enuControls eControlGrabbed = (enuControls)lblGrab.Tag;
                switch (eControlGrabbed)
                {
                    case enuControls.FirstLineIndent:
                        {
                            // moves just the FirstLineIndent

                            int intFirstLineIndent_Start = RTX.SelectionIndent;
                            int intFirstLineIndent_End = ptMouseRel.X;
                            if (intFirstLineIndent_End != intFirstLineIndent_Start)
                            {
                                // measure difference b/w FirstLine and Hanging before/after moving FirstLine
                                int intLeftIndent_Start = intFirstLineIndent_Start + RTX.SelectionHangingIndent;
                                int intLeftIndent_End = intLeftIndent_Start - intFirstLineIndent_End;
                                RTX.SelectionIndent = intFirstLineIndent_End;
                                RTX.SelectionHangingIndent = intLeftIndent_End;
                                placeObjects();
                            }
                        }
                        break;

                    case enuControls.LeftIndent:
                        {
                            // moves both FirstLineIndent && Hanging Indent
                            //measure difference between FirstLine and Hanging indents
                            RTX.SelectionIndent = ptMouseRel.X - RTX.SelectionHangingIndent;
                            placeObjects();
                        }
                        break;

                    case enuControls.HangingIndent:
                        {
                            RTX.SelectionHangingIndent = ptMouseRel.X - RTX.SelectionIndent;
                            placeObjects();
                        }
                        break;

                    case enuControls.RightIndent:
                        {
                            int intMouseXRelToWidth = RTX.RightMargin - ptMouseRel.X;
                            if (intMouseXRelToWidth < 10)
                                intMouseXRelToWidth = 10;
                            RTX.SelectionRightIndent = intMouseXRelToWidth;
                            placeObjects();
                        }
                        break;
                }


            }

            void lblMouse_Down(object sender, MouseEventArgs e)
            {
                tmrGrab.Tag = (object)SPContainer.cEleUnderMouse.obj;
                tmrGrab.Enabled = true;
            }

            void lblMouse_Up(object sender, MouseEventArgs e)
            {
                tmrGrab.Enabled = false;
            }

            #endregion

            #region Properties

            ck_RichTextBox _ckRTX = null;
            ck_RichTextBox RTX
            {
                get { return _ckRTX; }
            }

            #region bitmaps

            static Bitmap _bmpRuler = null;
            static Bitmap bmpRuler
            {
                get
                {
                    if (true || _bmpRuler == null)
                    {
                        Font fntNumeral = new Font("Segoe UI", 10);
                        _bmpRuler = new Bitmap(Screen.PrimaryScreen.Bounds.Width, conHeight);
                        int intVMid = (int)(conHeight / 2);
                        using (Graphics g = Graphics.FromImage(_bmpRuler))
                        {
                            g.FillRectangle(Brushes.LightGray, new RectangleF(0, 0, _bmpRuler.Width, _bmpRuler.Height));
                            int intRulerHeight = conHeight;
                            g.FillRectangle(Brushes.White, new RectangleF(0, intVMid - intRulerHeight / 2, _bmpRuler.Width, intRulerHeight));
                            g.DrawRectangle(Pens.Black, new Rectangle(0, 0, _bmpRuler.Width - 1, _bmpRuler.Height - 1));

                            int intStep = 10;
                            for (int intHashCounter = 0; intHashCounter < _bmpRuler.Width; intHashCounter += intStep)
                            {
                                int intHeight = 0;
                                bool bolDrawNumeral = false;
                                if (intHashCounter % (intStep * 10) == 0)
                                    intHeight = (int)(conHeight * .7);
                                else if (intHashCounter % (intStep * 5) == 0)
                                {
                                    intHeight = (int)(conHeight * .4);
                                    bolDrawNumeral = true;
                                }
                                else
                                    intHeight = (int)(conHeight * .2);
                                g.DrawLine(Pens.Black, new Point(intHashCounter, intVMid - intHeight / 2), new Point(intHashCounter, intVMid + intHeight / 2));
                                if (bolDrawNumeral)
                                {
                                    string strNumeral = (intHashCounter / (intStep * 10)).ToString();
                                    Size szNumeral = TextRenderer.MeasureText(strNumeral, fntNumeral);
                                    g.DrawString(strNumeral, fntNumeral, Brushes.DarkGray, new Point(intHashCounter - szNumeral.Width / 2, intVMid - szNumeral.Height / 2));
                                }
                            }
                        }
                    }
                    return _bmpRuler;
                }
            }



            static Bitmap _bmpLeftIndent = null;
            static Bitmap bmpLeftIndent
            {
                get
                {
                    if (_bmpLeftIndent == null)
                    {
                        _bmpLeftIndent = new Bitmap(9, 8);
                        using (Graphics g = Graphics.FromImage(_bmpLeftIndent))
                        {
                            //g.FillRectangle(Brushes.Red, new RectangleF(0, 0, _bmpLeftIndent.Width, _bmpLeftIndent.Height));
                            //int intPointDepth = 4;
                            Point[] pts = {
                                            new Point(0,0),
                                            new Point(_bmpLeftIndent.Width-1, 0),
                                            new Point(_bmpLeftIndent.Width -1, _bmpLeftIndent.Height-1),
                                            new Point(0, _bmpLeftIndent.Height-1),
                                            new Point(0,0)
                                        };
                            g.FillPolygon(Brushes.LightGray, pts);
                            g.DrawPolygon(Pens.DarkGray, pts);
                        }
                    }
                    return _bmpLeftIndent;
                }
            }

            static Bitmap _bmpRightIndent = null;
            static Bitmap bmpRightIndent
            {
                get
                {
                    if (_bmpRightIndent == null)
                    {
                        _bmpRightIndent = new Bitmap(9, 8);
                        Color clrTransparent = Color.Red;
                        using (Graphics g = Graphics.FromImage(_bmpRightIndent))
                        {
                            g.FillRectangle(new SolidBrush(clrTransparent), new RectangleF(0, 0, _bmpRightIndent.Width, _bmpRightIndent.Height));
                            int intPointDepth = 4;
                            Point[] pts = {
                                            new Point(_bmpRightIndent.Width /2, 0),
                                            new Point(_bmpRightIndent.Width-1, intPointDepth),
                                            new Point(_bmpRightIndent.Width -1, _bmpRightIndent.Height-1),
                                            new Point(0, _bmpRightIndent.Height-1),
                                            new Point(0, intPointDepth),
                                            new Point(_bmpRightIndent.Width /2, 0)
                                        };
                            g.FillPolygon(Brushes.LightGray, pts);
                            g.DrawPolygon(Pens.DarkGray, pts);
                        }
                        _bmpRightIndent.MakeTransparent(clrTransparent);
                    }

                    return _bmpRightIndent;
                }
            }

            static Bitmap _bmpFirstLineIndent = null;
            static Bitmap bmpFirstLineIndent
            {
                get
                {
                    if (_bmpFirstLineIndent == null)
                    {
                        _bmpFirstLineIndent = new Bitmap(9, 8);
                        Color clrTransparent = Color.Red;
                        using (Graphics g = Graphics.FromImage(_bmpFirstLineIndent))
                        {
                            g.FillRectangle(new SolidBrush(clrTransparent), new RectangleF(0, 0, _bmpFirstLineIndent.Width, _bmpFirstLineIndent.Height));
                            int intPointDepth = 4;
                            Point[] pts = {
                                            new Point(0,0),
                                            new Point(_bmpFirstLineIndent.Width-1, 0),
                                            new Point(_bmpFirstLineIndent.Width -1, _bmpFirstLineIndent.Height - intPointDepth),
                                            new Point(_bmpFirstLineIndent.Width /2, _bmpFirstLineIndent.Height-1),
                                            new Point(0, _bmpFirstLineIndent.Height - intPointDepth),
                                            new Point(0,0)
                                        };
                            g.FillPolygon(Brushes.LightGray, pts);
                            g.DrawPolygon(Pens.DarkGray, pts);
                        }
                        _bmpFirstLineIndent.MakeTransparent(clrTransparent);
                    }
                    return _bmpFirstLineIndent;
                }
            }

            static Bitmap _bmpHangingIndent = null;
            static Bitmap bmpHangingIndent
            {
                get
                {
                    if (_bmpHangingIndent == null)
                    {
                        _bmpHangingIndent = new Bitmap(9, 8);
                        Color clrTransparent = Color.Red;
                        using (Graphics g = Graphics.FromImage(_bmpHangingIndent))
                        {
                            g.FillRectangle(new SolidBrush(clrTransparent), new RectangleF(0, 0, _bmpHangingIndent.Width, _bmpHangingIndent.Height));
                            int intPointDepth = 4;
                            Point[] pts = {
                                            new Point(_bmpHangingIndent.Width /2,0),
                                            new Point(_bmpHangingIndent.Width -1, intPointDepth),
                                            new Point(_bmpHangingIndent.Width-1, _bmpHangingIndent.Height-1),
                                            new Point(0, _bmpHangingIndent.Height-1),
                                            new Point(0, intPointDepth),
                                            new Point(_bmpHangingIndent.Width /2,0)
                                        };
                            g.FillPolygon(Brushes.LightGray, pts);
                            g.DrawPolygon(Pens.DarkGray, pts);
                        }
                        _bmpHangingIndent.MakeTransparent(clrTransparent);
                    }

                    return _bmpHangingIndent;
                }
            }

            #endregion

            SPObjects.Label lblLeftIndent
            {
                get { return lblControls[(int)enuControls.LeftIndent]; }
            }

            SPObjects.Label lblRightIndent
            {
                get { return lblControls[(int)enuControls.RightIndent]; }
            }

            SPObjects.Label lblFirstLineIndent
            {
                get { return lblControls[(int)enuControls.FirstLineIndent]; }
            }

            SPObjects.Label lblHangingIndent
            {
                get { return lblControls[(int)enuControls.HangingIndent]; }
            }

            #endregion


            #region Methods
            public static Point MouseRelTo(Control ctrl)
            {
                Point ptRetVal = System.Windows.Forms.Control.MousePosition;

                while (ctrl != null)
                {
                    ptRetVal.X -= ctrl.Location.X;
                    ptRetVal.Y -= ctrl.Location.Y;
                    ctrl = ctrl.Parent;
                }

                return ptRetVal;
            }

            public void placeObjects()
            {
                bool bolBuilding = SPContainer.BuildingInProgress;
                SPContainer.Building_Start();
                {
                    if (Parent != null)
                        Width = Parent.Width;
                    Height = conHeight;

                    lblFirstLineIndent.Left = RTX.SelectionIndent - RTX.Margin.Left - lblFirstLineIndent.Width / 2;
                    if (lblFirstLineIndent.Left < 0)
                        lblFirstLineIndent.Left = 0;

                    lblFirstLineIndent.Top = 0;

                    int intLeftIndent = RTX.SelectionIndent + RTX.SelectionHangingIndent - RTX.Margin.Left - lblLeftIndent.Width / 2;
                    if (intLeftIndent < 0)
                        intLeftIndent = 0;
                    lblLeftIndent.Left
                        = lblHangingIndent.Left
                        = intLeftIndent;
                    lblRightIndent.Left = RTX.RightMargin - RTX.SelectionRightIndent - lblRightIndent.Width / 2;

                    lblRightIndent.Top = Height - lblRightIndent.Height;
                    lblLeftIndent.Top = Height - lblLeftIndent.Height;
                    lblHangingIndent.Top = lblLeftIndent.Top - lblHangingIndent.Height + 1;

                    SPContainer.BackgroundImage = bmpRuler;
                    SPContainer.recVisible
                        = SPContainer.recSPArea
                        = new Rectangle(0, 0, Width, Height);
                    SPContainer.RecVisible_Changed = true;
                }
                if (!bolBuilding)
                    SPContainer.Building_Complete();
            }

            public void SetToSelectedText()
            {
                placeObjects();
            }

            #endregion
        }

        public class formFindReplace : Form
        {
            public static formFindReplace instance = null;

            string strAlphabet
            {
                get { return ck_RichTextBox.strAlphabet; }
            }

            public enum enuMode { Find, FindReplace, _numMode };

            List<int> lstSearchResults = new List<int>();
            string strSearchWord = "";

            Label lblSearchText = new Label();
            TextBox txtSearchText = new TextBox();

            Label lblReplacementText = new Label();
            TextBox txtReplacementText = new TextBox();

            GroupBox grbSearchOptions = new GroupBox();
            CheckBox cbxMatchWholeWord = new CheckBox();
            CheckBox cbxMatchCase = new CheckBox();
            RadioButton rbtSearchUp = new RadioButton();
            RadioButton rbtSearchDown = new RadioButton();

            Button btnFindNext = new Button();
            Button btnReplace = new Button();
            Button btnReplaceAll = new Button();
            Button btnCancel = new Button();

            ck_RichTextBox _ckRTX = null;
            public ck_RichTextBox ckRTX
            {
                get { return _ckRTX; }
                set
                {
                    if (_ckRTX != value)
                    {
                        _ckRTX = value;
                        Search_Reset();
                    }
                }
            }

            public static void ShowDialog(enuMode eMode, ref ck_RichTextBox ck_RichText)
            {
                if (instance == null || instance.IsDisposed)
                    new formFindReplace(eMode, ref ck_RichText);
                instance.ckRTX = ck_RichText;
                instance.eMode = eMode;
                instance.Show();
            }


            formFindReplace(enuMode eMode, ref ck_RichTextBox ck_RichText)
            {
                ckRTX = ck_RichText;
                instance = this;

                this.eMode = eMode;
                MinimizeBox = false;
                MaximizeBox = false;

                Icon = Properties.Resources.Search_Text;
                ShowInTaskbar = false;

                TopMost = true;// !formWords.Debugging;
                BringToFront();

                Controls.Add(lblSearchText);
                lblSearchText.AutoSize = true;
                lblSearchText.Text = "Search Text:";

                Controls.Add(txtSearchText);
                txtSearchText.KeyDown += _KeyDown_QuitTest;
                txtSearchText.KeyUp += TxtSearchText_KeyUp;
                txtSearchText.GotFocus += TxtSearchText_GotFocus;

                Controls.Add(lblReplacementText);
                lblReplacementText.Text = "Replacement Text";
                lblReplacementText.AutoSize = true;

                Controls.Add(txtReplacementText);
                txtReplacementText.Enabled = false;
                txtReplacementText.KeyDown += _KeyDown_QuitTest;

                Controls.Add(grbSearchOptions);
                grbSearchOptions.Text = "Search Options";
                grbSearchOptions.AutoSize = true;

                grbSearchOptions.Controls.Add(cbxMatchWholeWord);
                cbxMatchWholeWord.Text = "Match Whole Word";
                cbxMatchWholeWord.AutoSize = true;
                cbxMatchWholeWord.Checked = false;
                cbxMatchWholeWord.CheckedChanged += CbxMatchWholeWord_CheckedChanged;

                grbSearchOptions.Controls.Add(cbxMatchCase);
                cbxMatchCase.Text = "Match Case";
                cbxMatchCase.AutoSize = true;
                cbxMatchCase.Checked = false;
                cbxMatchCase.CheckedChanged += CbxMatchCase_CheckedChanged;

                grbSearchOptions.Controls.Add(rbtSearchUp);
                rbtSearchUp.Text = "Search Up";
                rbtSearchUp.AutoSize = true;
                rbtSearchUp.Checked = false;

                grbSearchOptions.Controls.Add(rbtSearchDown);
                rbtSearchDown.Text = "Search Down";
                rbtSearchDown.AutoSize = true;
                rbtSearchDown.Checked = true;
                rbtSearchUp.CheckedChanged += RbtSearchUp_CheckedChanged;

                Controls.Add(btnFindNext);
                btnFindNext.Text = "Find Next";
                btnFindNext.Click += BtnFindNext_Click;

                Controls.Add(btnReplace);
                btnReplace.Text = "Replace";
                btnReplace.Enabled = false;
                btnReplace.Click += BtnReplace_Click;

                Controls.Add(btnReplaceAll);
                btnReplaceAll.Text = "Replace All";
                btnReplaceAll.Enabled = false;
                btnReplaceAll.Click += BtnReplaceAll_Click;

                Controls.Add(btnCancel);
                btnCancel.Text = "Cancel";
                btnCancel.Click += BtnCancel_Click;

                btnFindNext.Size
                    = btnReplace.Size
                    = btnReplaceAll.Size
                    = btnCancel.Size
                    = new Size(73, 21);

                Activated += FormFindReplace_Activated;
                KeyDown += _KeyDown_QuitTest;
            }

            #region Methods
            void placeObjects()
            {
                grbSearchOptions.Size = new Size(225, 78);

                cbxMatchWholeWord.Location = new Point(5, 25);
                cbxMatchCase.Location = new Point(cbxMatchWholeWord.Left, cbxMatchWholeWord.Bottom + 10);

                rbtSearchUp.Location = new Point(cbxMatchWholeWord.Right, cbxMatchWholeWord.Top);
                rbtSearchDown.Location = new Point(rbtSearchUp.Left, cbxMatchCase.Top);

                lblSearchText.Location = new Point(10, 5);
                txtSearchText.Location = new Point(lblSearchText.Left, lblSearchText.Bottom);

                txtSearchText.Width
                    = txtReplacementText.Width = grbSearchOptions.Width - 10;

                grbSearchOptions.Left = lblSearchText.Left;
                btnFindNext.Location = new Point(grbSearchOptions.Right + 7, txtSearchText.Top);

                switch (eMode)
                {
                    case enuMode.Find:
                        {
                            Text = "Search Text";
                            grbSearchOptions.Top = txtSearchText.Bottom;
                            btnCancel.Location = new Point(btnFindNext.Left, btnFindNext.Bottom + 20);

                            btnReplace.Visible
                                = btnReplaceAll.Visible
                                = lblReplacementText.Visible
                                = txtReplacementText.Visible
                                = false;

                        }
                        break;

                    case enuMode.FindReplace:
                        {
                            Text = "Search & Replace Text";
                            lblReplacementText.Location = new Point(lblSearchText.Left, txtSearchText.Bottom);
                            txtReplacementText.Location = new Point(lblReplacementText.Left, lblReplacementText.Bottom);
                            grbSearchOptions.Top = txtReplacementText.Bottom;

                            int intVGap = 5;
                            btnReplace.Location = new Point(btnFindNext.Left, btnFindNext.Bottom + intVGap);
                            btnReplaceAll.Location = new Point(btnFindNext.Left, btnReplace.Bottom + intVGap);
                            btnCancel.Location = new Point(btnFindNext.Left, btnReplaceAll.Bottom + intVGap);

                            btnReplace.Visible
                                = btnReplaceAll.Visible
                                = lblReplacementText.Visible
                                = txtReplacementText.Visible
                                = true;
                        }
                        break;
                }
                Size = new Size(348, grbSearchOptions.Bottom + 45);
            }

            public void Search_Reset()
            {
                strSearchWord = "";
                intSearchListIndex = -1;
                lstSearchResults.Clear();
            }

            void Search_SelectText()
            {
                if (intSearchListIndex < 0 || intSearchListIndex >= lstSearchResults.Count) return;
                rtx.Select(lstSearchResults[intSearchListIndex], txtSearchText.Text.Length);
                rtx.ScrollToCaret();
                rtx.rtx.Focus();
            }

            void Search_Indices_FindBest()
            {
                if (lstSearchResults.Count == 0)
                    return;

                int intListIndexBest = lstSearchResults.Count / 2;
                int intStep = intListIndexBest;
                int intDir = 1;
                int intCursorIndex = rtx.SelectionStart;
                do
                {
                    intStep /= 2;
                    if (intStep < 1)
                        intStep = 1;
                    if (intCursorIndex == lstSearchResults[intListIndexBest])
                    {
                        intSearchListIndex = intListIndexBest;
                        return;
                    }
                    else if (intCursorIndex > lstSearchResults[intListIndexBest])
                    {
                        if (lstSearchResults.Count > intListIndexBest + 1)
                        {
                            if (intCursorIndex < lstSearchResults[intListIndexBest + 1])
                            {
                                intSearchListIndex = intListIndexBest;
                                return;
                            }
                        }
                        else
                        {
                            intSearchListIndex = intListIndexBest;
                            return;
                        }
                        intDir = 1;
                    }
                    else
                    {
                        if (intListIndexBest > 0)
                        {
                            if (intCursorIndex > lstSearchResults[intListIndexBest - 1])
                            {
                                intSearchListIndex = intListIndexBest;
                                return;
                            }
                        }
                        else
                        {
                            intSearchListIndex = intListIndexBest;
                            return;
                        }
                        intDir = -1;
                    }
                    intListIndexBest += intStep * intDir;
                } while (true);
            }

            void Search_Indices_BuildList()
            {
                string strText = cbxMatchCase.Checked
                                            ? rtx.Text
                                            : rtx.Text.ToLower();
                if (strSearchWord.Length == 0 && txtSearchText.Text.Length > 0)
                {
                    strSearchWord = cbxMatchCase.Checked
                                            ? txtSearchText.Text
                                            : txtSearchText.Text.ToLower();
                    lstSearchResults.Clear();

                    int intIndex = -1;
                    do
                    {
                        intIndex = strText.IndexOf(strSearchWord, intIndex + 1);
                        if (intIndex >= 0)
                        {
                            if (cbxMatchWholeWord.Checked)
                            {
                                if ((intIndex > 0 && !strAlphabet.Contains(strText[intIndex - 1]))
                                                || intIndex == 0)
                                {
                                    if (((intIndex < strText.Length - 1 && !strAlphabet.Contains(strText[intIndex + strSearchWord.Length])
                                                || intIndex == strText.Length)))
                                        lstSearchResults.Add(intIndex);
                                }
                            }
                            else
                                lstSearchResults.Add(intIndex);
                        }
                    } while (intIndex >= 0);
                }
            }
            bool replace(string strSearch, string strReplace)
            {
                if (string.Compare(rtx.SelectedText.ToLower(), strSearch.ToLower()) == 0)
                {
                    rtx.SelectedText = strReplace;
                    Search_Reset();
                    return true;
                }
                return false;
            }
            #endregion

            #region Properties
            enuMode _eMode = enuMode.Find;
            public enuMode eMode
            {
                get { return _eMode; }
                set
                {
                    _eMode = value;
                    placeObjects();
                }
            }

            int _intSearchListIndex = -1;
            int intSearchListIndex
            {
                get { return _intSearchListIndex; }
                set
                {
                    if (value >= lstSearchResults.Count)
                        value = -1;
                    _intSearchListIndex = value;
                }
            }

            ck_RichTextBox rtx { get { return ckRTX; } }

            #endregion

            #region Events 
            bool bolActivated = false;
            private void FormFindReplace_Activated(object sender, EventArgs e)
            {
                if (bolActivated) return;

                placeObjects();
                bolActivated = true;
            }

            private void _KeyDown_QuitTest(object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        {
                            Dispose();
                        }
                        break;

                    case Keys.F:
                        {
                            if (e.Modifiers == Keys.Control)
                                eMode = enuMode.Find;
                        }
                        break;

                    case Keys.H:
                        {
                            if (e.Modifiers == Keys.Control)
                                eMode = enuMode.FindReplace;
                        }
                        break;
                }
            }

            private void CbxMatchCase_CheckedChanged(object sender, EventArgs e)
            {
                //formWords.Search_MatchCase = cbxMatchCase.Checked;
                Search_Reset();
            }

            private void CbxMatchWholeWord_CheckedChanged(object sender, EventArgs e)
            {
                //formWords.Search_MatchWord = cbxMatchWholeWord.Checked; 
                Search_Reset();
            }

            private void RbtSearchUp_CheckedChanged(object sender, EventArgs e)
            {
                //formWords.Search_Up = rbtSearchUp.Checked;
            }

            private void BtnCancel_Click(object sender, EventArgs e)
            {
                Dispose();
            }

            private void BtnReplaceAll_Click(object sender, EventArgs e)
            {
                do
                {
                    BtnFindNext_Click(sender, e);
                } while (replace(txtSearchText.Text, txtReplacementText.Text));
            }

            private void BtnReplace_Click(object sender, EventArgs e)
            {
                replace(txtSearchText.Text, txtReplacementText.Text);
            }

            public void BtnFindNext_Click(object sender, EventArgs e)
            {
                Search_Indices_BuildList();

                if (intSearchListIndex < 0)
                {
                    Search_Indices_FindBest();
                }

                if (intSearchListIndex < 0 || intSearchListIndex >= lstSearchResults.Count)
                {
                    goto QuitSearch;
                }

                if (rbtSearchDown.Checked)
                {
                    if (rtx.SelectionStart >= lstSearchResults[intSearchListIndex])
                        intSearchListIndex++;
                    if (intSearchListIndex < lstSearchResults.Count && intSearchListIndex >= 0)
                    {
                        Search_SelectText();
                    }
                    else
                        goto QuitSearch;
                }
                else
                {
                    if (rtx.SelectionStart <= lstSearchResults[intSearchListIndex])
                        intSearchListIndex--;
                    if (intSearchListIndex > 0)
                    {
                        Search_SelectText();
                    }
                    else
                        goto QuitSearch;
                }
                return;
            QuitSearch:
                MessageBox.Show("Search has reached end of text", "Search " + (rbtSearchDown.Checked ? "Down" : "Up") + " Complete");

            }
            private void TxtSearchText_GotFocus(object sender, EventArgs e)
            {
                txtSearchText.SelectAll();
            }

            private void TxtSearchText_KeyUp(object sender, KeyEventArgs e)
            {
                btnReplace.Enabled
                    = btnReplaceAll.Enabled
                    = txtReplacementText.Enabled
                    = txtSearchText.Text.Length > 0;
                Search_Reset();
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        {
                            BtnFindNext_Click((object)btnFindNext, new EventArgs());
                        }
                        break;
                }
            }

            #endregion 
        }

        public class classHighlighterColorItem
        {
            public bool valid = false;
            public Color clrBack = Color.White;
            public Color clrFore = Color.Black;
            public string Text = "";
            public classHighlighterColorItem(Color clrBack, Color clrFore, string text)
            {
                this.clrBack = clrBack;
                this.clrFore = clrFore;
                this.Text = text;
                this.valid = true;
            }
        }
    }
}
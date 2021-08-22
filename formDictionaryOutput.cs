using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using StringLibrary;

namespace Words
{
    public class formDictionaryOutput : Form
    {
        public static formDictionaryOutput instance;
        static public RichTextBox rtxOutput = null;

        static public RichTextBox _rtxUnderMouse = null;
        static public RichTextBox rtxUnderMouse
        {
            get { return _rtxUnderMouse; }
            set { _rtxUnderMouse = value; }
        }


        SplitContainer sptMain = new SplitContainer();
        SplitContainer sptSub = new SplitContainer();
        panelSP.panelSP pnlSP = new panelSP.panelSP();

        public static Font fntDictionaryHeading = new Font("Times Roman", 14, FontStyle.Bold & FontStyle.Italic);
        public static Font fntWordHeading = new Font("Times Roman", 12, FontStyle.Underline);
        public static Font fntWordDefinition = new Font("Times Roman", 10, FontStyle.Regular);

        static Point ptRtxMouseLocation = new Point();
        formFindReplace _frmFindReplace;
        ListBox lbxResults = new ListBox();

        Ck_Objects.classLabelButton btnCopy = new Ck_Objects.classLabelButton();
        Ck_Objects.classLabelButton btnTopMost = new Ck_Objects.classLabelButton();

        static System.Windows.Forms.Timer tmrMouseHover = new System.Windows.Forms.Timer();
        static Semaphore semHoverWord = new Semaphore(1, 1);

        public static List<string> lstRTFDictionaries = new List<string>();
        string strDirectoryAndFileName = "";
        public formDictionaryOutput()
        {
            instance = this;
            rtxOutput = new RichTextBox();
            _frmFindReplace = formWords.instance.frmFindReplace;
            TopMost = true;
            sptSub.Orientation = Orientation.Horizontal;
            sptMain.Orientation = Orientation.Horizontal;
            VisibleChanged += new EventHandler(formDictionaryOutput_VisibleChanged);

            sptSub.SplitterMoved += spt_SplitterMoved;
            sptMain.SplitterMoved += spt_SplitterMoved;

            Bitmap bmp = new Bitmap(Properties.Resources.Dictionary);
            Icon = Icon.FromHandle(bmp.GetHicon());

            rtxOutput.Controls.Add(btnCopy);
            btnCopy.AutoSize = true;
            btnCopy.Text = "^";
            btnCopy.Click += new EventHandler(btnCopy_Click);

            rtxOutput.Controls.Add(btnTopMost);
            btnTopMost.AutoSize = true;
            btnTopMost.CanBeToggled = true;
            btnTopMost.Text = "!";
            btnTopMost.Click += new EventHandler(btnTopMost_Click);

            Controls.Add(sptMain);
            sptMain.Dock = DockStyle.Fill;
            {
                sptMain.Panel1.Controls.Add(lbxResults);
                lbxResults.SelectedIndexChanged += LbxResults_SelectedIndexChanged;
                lbxResults.MouseLeave += LbxResults_MouseLeave;
                lbxResults.MouseEnter += LbxResults_MouseEnter;
                lbxResults.MouseMove += LbxResults_MouseMove;
                lbxResults.MouseClick += LbxResults_MouseClick;
                lbxResults.Dock = DockStyle.Fill;
            }

            sptMain.Panel2.Controls.Add(sptSub);
            sptSub.Dock = DockStyle.Fill;

            sptSub.Panel1.Controls.Add(rtxOutput);
            rtxOutput.Multiline = true;
            rtxOutput.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtxOutput.Font = new Font("ms sans-serif", 12);
            rtxOutput.Visible = true;
            rtxOutput.VScroll += RtxOutput_VScroll;
            rtxOutput.MouseMove += RtxOutput_MouseMove;
            rtxOutput.MouseEnter += RtxOutput_MouseEnter;
            rtxOutput.MouseLeave += RtxOutput_MouseLeave;
            rtxOutput.MouseUp += RtxOutput_MouseUp;
            rtxOutput.MouseWheel += RtxOutput_MouseWheel;
            rtxOutput.KeyDown += RtxOutput_KeyDown;
            rtxOutput.KeyUp += RtxOutput_KeyUp;
            rtxOutput.KeyPress += RtxOutput_KeyPress;
            rtxOutput.Disposed += RtxOutput_Disposed;

            rtxOutput.GotFocus += new EventHandler(formDictionaryOutput_GotFocus);
            rtxOutput.LostFocus += new EventHandler(formDictionaryOutput_LostFocus);
            rtxOutput.Dock = DockStyle.Fill;

            sptSub.Panel2.Controls.Add(pnlSP);
            pnlSP.Dock = DockStyle.Fill;

            Disposed += new EventHandler(formDictionaryOutput_Disposed);
            SizeChanged += new EventHandler(formDictionaryOutput_SizeChanged);
            LocationChanged += FormDictionaryOutput_LocationChanged;
            Activated += FormDictionaryOutput_Activated;
            GotFocus += new EventHandler(formDictionaryOutput_GotFocus);
            LostFocus += new EventHandler(formDictionaryOutput_LostFocus);
            MouseLeave += FormDictionaryOutput_MouseLeave;
            TextChanged += FormDictionaryOutput_TextChanged;


            tmrMouseHover_Interval_Set();
            tmrMouseHover.Enabled = false;
            tmrMouseHover.Tick += TmrMouseHover_Tick;
        }


        private void FormDictionaryOutput_TextChanged(object sender, EventArgs e)
        {
            ;
        }

        static RichTextBox _rtx_Calling = null;
        static public RichTextBox rtxCalling
        {
            get { return _rtx_Calling; }
            set { _rtx_Calling = value; }
        }

        private void spt_SplitterMoved(object sender, SplitterEventArgs e)
        {
            placeObjects();
        }


        public static void tmrMouseHover_Reset()
        {
            tmrMouseHover_Enabled = false;
            tmrMouseHover_Enabled = (formDictionarySelection.instance.PopUp_Dictionary != null && formDictionarySelection.ePopUpDelayTime != enuPopUpDelayTime.RightMouse_Click);
        }


        public void tmrMouseHover_Interval_Set()
        {
            switch (formDictionarySelection.ePopUpDelayTime)
            {
                case enuPopUpDelayTime.One_Second:
                    tmrMouseHover_Enabled = true;
                    tmrMouseHover.Interval = 1000;
                    break;

                case enuPopUpDelayTime.Three_Seconds:
                    tmrMouseHover_Enabled = true;
                    tmrMouseHover.Interval = 3000;
                    break;

                case enuPopUpDelayTime.Five_Seconds:
                    tmrMouseHover_Enabled = true;
                    tmrMouseHover.Interval = 5000;
                    break;

                case enuPopUpDelayTime.Ten_Seconds:
                    tmrMouseHover_Enabled = true;
                    tmrMouseHover.Interval = 10000;
                    break;

                case enuPopUpDelayTime.RightMouse_Click:
                    tmrMouseHover_Enabled = false;
                    tmrMouseHover.Interval = int.MaxValue;
                    break;

                default:
                    break;
            }
        }



        private void FormDictionaryOutput_MouseLeave(object sender, EventArgs e)
        {
            bolMouseOverLbxResults = false;
        }


        void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(rtxOutput.Text);
        }

        void btnTopMost_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
            btnTopMost.Toggled = TopMost;
        }
        private void FormDictionaryOutput_Activated(object sender, EventArgs e)
        {
            if (bolInit) return;

            formInfo_Load();

            bolInit = true;
        }


        static void WordUnderMouse_PopUpDefinition() { WordUnderMouse_PopUpDefinition((object)rtxUnderMouse ); }
        static void WordUnderMouse_PopUpDefinition(object sender)
        {
            semHoverWord.WaitOne();

            RichTextBox rtxSender = (RichTextBox)sender;

            if ( formDictionarySelection.instance.PopUp_Dictionary != null)
            {

                string strWordUnderMouse = classStringLibrary.getWordUnderMouse(ref rtxSender, ptRtxMouseLocation);
                if (strWordUnderMouse != null && strWordUnderMouse.Length > 0)
                {
                    if (string.Compare(strWordUnderMouse, strSearchWord) != 0)
                    {
                        strSearchWord = strWordUnderMouse;
                        List<string> lstWords = new List<string>();
                        lstWords.Add(strSearchWord);
                        classDictionary cDictionaryPopUp = formDictionarySelection.instance.PopUp_Dictionary;
                        List<classSearchParameters> lstSearchParameters = new List<classSearchParameters>();
                        lstSearchParameters.Add(new classSearchParameters(ref cDictionaryPopUp, ref lstWords, enuSearchType.Heading));
                        formWords.instance.Search(ref lstSearchParameters, enuSearchRequestor.definition_Hover);
                    }
                }
                else
                {
                    if (formPopUp.instance != null
                                &&
                        !formPopUp.instance.IsDisposed)
                        formPopUp.instance.Hide(); 
                }
            }
            semHoverWord.Release();
        }

        static void WordUnderMouse_Insert(object sender, Point ptMouse)
        {
            RichTextBox rtxSender = (RichTextBox)sender;

            int intSelection = rtxSender.GetCharIndexFromPosition(ptMouse);

            int intStartWord = intSelection - 1;
            while (intStartWord >= 0 && classStringLibrary.isAlpha(rtxSender.Text[intStartWord])) intStartWord--;
            if (intStartWord < 0)
                intStartWord = 0;
            int intEndWord = intSelection;
            while (intEndWord < rtxSender.Text.Length && classStringLibrary.isAlpha(rtxSender.Text[intEndWord])) intEndWord++;
            if (intEndWord > rtxSender.Text.Length)
                intEndWord = rtxSender.Text.Length;
            if (intEndWord > intStartWord + 1)
            {
                rtxSender.Select(intStartWord, intEndWord - intStartWord);
                if (formWords.RTX_Focused != null)
                    formWords.RTX_Focused.SelectedRtf = rtxSender.SelectedRtf;
            }
        }

        static void WordUnderMouse_Search(object sender, Point ptMouse)
        {
            RichTextBox rtxSender = (RichTextBox)sender;

            strSearchWord
                = strHeading
                = classStringLibrary.getWordUnderMouse(ref rtxSender, ptMouse);

            if (strSearchWord != null
                && strSearchWord.Length > 0
                && formDictionarySelection.instance.PopUp_Dictionary != null)
            {
                List<string> lstWords = new List<string>();
                lstWords.Add(strSearchWord);
                classDictionary cDictionary = null;
                if (rtxSender != rtxOutput && rtxSender.Tag != null)
                {
                    classDefinitionCopy cDefinitionCopy = (classDefinitionCopy)rtxSender.Tag;
                    cDictionary = cDefinitionCopy.cDictionary;
                }
                else
                {
                    cDictionary = formDictionarySelection.instance.PopUp_Dictionary;
                }

                List<classSearchParameters> lstSearchParameters = new List<classSearchParameters>();
                lstSearchParameters.Add(new classSearchParameters(ref cDictionary, ref lstWords, enuSearchType.Heading));
                formWords.instance.strClickedWord = strSearchWord;
                rtxDisplayCopy_Clicked = rtxSender;
                formWords.instance.Search(ref lstSearchParameters, rtxSender == rtxOutput
                                                                             ? enuSearchRequestor.definition_Click
                                                                             : enuSearchRequestor.txtDisplay_Click);
            }
        }


        private void RtxOutput_Disposed(object sender, EventArgs e)
        {

        }


        public static void Rtx_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WordUnderMouse_Insert(sender, new Point(e.X, e.Y));
        }

        static bool bolIgnoreMouseUp = false;

        public static void RtxOutput_MouseUp(object sender, MouseEventArgs e)
        {
            if (bolIgnoreMouseUp)
            {
                bolIgnoreMouseUp = false;
                return;
            }
            rtxCalling = (RichTextBox)sender;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        WordUnderMouse_Search(sender, new Point(e.X, e.Y));
                    }
                    break;

                case MouseButtons.Right:
                    {
                        WordUnderMouse_PopUpDefinition(sender);
                    }
                    break;

                case MouseButtons.Middle:
                    break;
            }
        }

        public static void RtxOutput_MouseLeave(object sender, EventArgs e)
        {
            tmrMouseHover_Enabled = false;
            formWords.RTX_Focused.Focus();
        }

        public static void RtxOutput_MouseWheel(object sender, MouseEventArgs e)
        {
            rtxOutput.Focus();
        }

        public static void RtxOutput_MouseEnter(object sender, EventArgs e)
        {
            tmrMouseHover_Enabled = (formDictionarySelection.instance.PopUp_Dictionary != null && formDictionarySelection.ePopUpDelayTime != enuPopUpDelayTime.RightMouse_Click);
            RichTextBox rtxSender = (RichTextBox)sender;
            rtxUnderMouse = rtxSender;
            rtxSender.Focus();
        }

        public static void RtxOutput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (formWords.RTX_Focused != null)
            {
                RichTextBox rtx = formWords.RTX_Focused;
                if (!rtx.Focused)
                    rtx.Focus();
                SendKeys.Send(e.KeyChar.ToString());
            }
        }
        public static void RtxOutput_KeyUp(object sender, KeyEventArgs e)
        {
            object objSender = (object)formWords.RTX_Focused;
            e.SuppressKeyPress = true;
        }

        public static void RtxOutput_KeyDown(object sender, KeyEventArgs e)
        {
            if (formWords.RTX_Focused != null)
            {
                RichTextBox rtx = formWords.RTX_Focused;
                if (!rtx.Focused)
                rtx.Focus();
                formWords.instance.rtxCK_KeyDown(rtx, e);
                
            }
        }

        List<classSearchResult> _lstHoverSearchResults = null;
        public List<classSearchResult> lstHoverSearchResults
        {
            get { return _lstHoverSearchResults; }
            set
            {
                semHoverWord.WaitOne();
                _lstHoverSearchResults = value;

                if (lstHoverSearchResults != null && lstHoverSearchResults.Count > 0)
                {
                    IEnumerable<classSearchResult> query = lstHoverSearchResults.OrderBy(SearchResult => SearchResult.strFileName);
                    _lstHoverSearchResults = (List<classSearchResult>)query.ToList<classSearchResult>();

                    // eliminate all entries that do not start with same letter as searchec word
                    if (strSearchWord.Length > 0)
                    {
                        char chrTest = strSearchWord.ToLower()[0];
                        int intIndex = 0;
                        bool bolFoundIdealMatch = false;
                        for (intIndex = 0; intIndex < lstHoverSearchResults.Count; intIndex++)
                        {
                            if (lstHoverSearchResults[intIndex].strFileName != null
                                && lstHoverSearchResults[intIndex].strFileName.Length > 0
                                && lstHoverSearchResults[intIndex].strFileName.ToLower()[0] == chrTest)
                            {
                                bolFoundIdealMatch = true;
                                break;
                            }
                        }
                        if (!bolFoundIdealMatch)
                            intIndex = 0;
                        // find entry whose heading exactly matches searched word
                        string strWordTest = StringLibrary.classStringLibrary.cleanFront_nonAlpha(strSearchWord.ToLower());
                        bool bolResultFound = false;
                        for (int intCounter = intIndex; intCounter < lstHoverSearchResults.Count; intCounter++)
                        {
                            if (lstHoverSearchResults[intCounter].strFileName != null
                                && lstHoverSearchResults[intCounter].strFileName.Length > 0)
                            {
                                classFileContent cFileContent = new classFileContent(lstHoverSearchResults[intCounter].cDictionary.strSourceDirectory,
                                                                                     lstHoverSearchResults[intCounter].strFileName);
                                if (cFileContent != null
                                    && (string.Compare(strWordTest, cFileContent.Heading.Trim().ToLower()) == 0
                                            ||
                                        (!bolResultFound && cFileContent.Heading.ToLower().Contains(strWordTest))))
                                {
                                    string strFileName = classFileContent.getFileNameAndDirectory(lstHoverSearchResults[intCounter].cDictionary.strSourceDirectory, lstHoverSearchResults[intCounter].strFileName);
                                    bolResultFound = true;

                                    switch (formWords.instance.eSearchRequestor)
                                    {
                                        case enuSearchRequestor.definition_Hover:
                                            {
                                                PopUp(strFileName, true);
                                            }
                                            break;
                                    }

                                }
                            }
                            else
                                break;
                        }
                    }
                }





                semHoverWord.Release();
            }
        }

        static string _strSearchWord = "";
        static string strSearchWord
        {
            get { return _strSearchWord; }
            set
            {
                List<string> lstWords = classStringLibrary.getWords(value);
                //if (lstWords.Count > 1)
                //    MessageBox.Show("fuck !");

                _strSearchWord = value;
            }
        }

        private void TmrMouseHover_Tick(object sender, EventArgs e)
        {
            tmrMouseHover_Enabled = false;
            long lngDTStop = DateTime.Now.Ticks;
            WordUnderMouse_PopUpDefinition();
        }


        public static int tmrMouseHover_Interval
        {
            get
            {
                return instance != null
                            ? tmrMouseHover.Interval
                            : int.MaxValue;
            }
        }

        public static bool tmrMouseHover_Enabled
        {
            get { return tmrMouseHover.Enabled; }
            set
            {
                tmrMouseHover.Enabled = value;
            }
        }



        static long lngDTStart = 0;
        public static void RtxOutput_VScroll(object sender, EventArgs e)
        {

            tmrMouseHover_Reset();
            lngDTStart = DateTime.Now.Ticks;
        }

        public static void RtxOutput_MouseMove(object sender, MouseEventArgs e)
        {
            tmrMouseHover_Reset();
            //SPObject_TxtDisplay.txtDisplay_UnderMouse = null;
            lngDTStart = DateTime.Now.Ticks;

            ptRtxMouseLocation.X = e.X;
            ptRtxMouseLocation.Y = e.Y;
        }

        public void lbxResults_ScrollDown()
        {
            if (lbxResults.SelectedIndex < lbxResults.Items.Count - 1)
            {
                lbxResults.SelectedIndex += 1;
                if (lbxResults.SelectedIndex >= 0 && lbxResults.SelectedIndex < lbxResults.Items.Count)
                    LoadRTX(lbxResults.SelectedIndex);
            }
        }


        public void lbxResults_ScrollUp()
        {
            if (lbxResults.SelectedIndex > 0)
            {
                lbxResults.SelectedIndex -= 1;
                if (lbxResults.SelectedIndex >= 0 && lbxResults.SelectedIndex < lbxResults.Items.Count)
                    LoadRTX(lbxResults.SelectedIndex);
            }
        }

        private void LbxResults_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox lbxSender = (ListBox)sender;
            int intMouseOverIndex = lbxResults.IndexFromPoint(new Point(e.X, e.Y));
            if (intMouseOverIndex >= 0 && intMouseOverIndex < lbxSender.Items.Count)
            {
                lbxSender.SelectedIndex = intMouseOverIndex;
            }
        }
        bool bolMouseOverLbxResults = false;
        private void LbxResults_MouseEnter(object sender, EventArgs e)
        {
            bolMouseOverLbxResults = true;
        }
        private void LbxResults_MouseLeave(object sender, EventArgs e)
        {
            bolMouseOverLbxResults = false;
            if (lbxResults.Items.Count > intRtxOutput_Index)
                lbxResults.SelectedIndex = intRtxOutput_Index;
            if (formPopUp.instance != null)
                formPopUp.instance.Hide();
        }

        private void LbxResults_MouseClick(object sender, MouseEventArgs e)
        {
            ListBox lbxSender = (ListBox)sender;
            if (lbxSender.SelectedIndex >= 0 && lbxSender.SelectedIndex < lbxSender.Items.Count)
            {
                LoadRTX(lbxSender.SelectedIndex);
            }
        }

        private void LbxResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lbxSender = (ListBox)sender;
            if (lbxSender.Items.Count != lstSearchResults_Main.Count) return;
            if (bolMouseOverLbxResults)
            {
                if (lbxSender.SelectedIndex >= 0 && lbxSender.SelectedIndex < lbxSender.Items.Count)
                {
                    if (formPopUp.instance == null || formPopUp.instance.IsDisposed) new formPopUp();
                    string strFileName = classFileContent.getFileNameAndDirectory(lstSearchResults_Main[lbxSender.SelectedIndex].cDictionary.strSourceDirectory, lstSearchResults_Main[lbxSender.SelectedIndex].strFileName);
                    PopUp(strFileName);
                }
            }
        }


        public void PopUp(string strFileName) { PopUp(strFileName, false); }
        public void PopUp(string strFileName, bool bolUnderMouse)
        {
            if (formPopUp.instance == null)
                new formPopUp();
            formPopUp.instance.cDictionaryOutput_rtx.LoadFile(strFileName);
            if (!bolUnderMouse)
            {
                formPopUp.instance.Location = new Point(MousePosition.X + 15, MousePosition.Y + 15);
                if (formPopUp.instance.Right > Screen.PrimaryScreen.WorkingArea.Width)
                    formPopUp.instance.Left = MousePosition.X - formPopUp.instance.Width - 15;
                if (formPopUp.instance.Bottom > Screen.PrimaryScreen.WorkingArea.Height)
                    formPopUp.instance.Top = MousePosition.Y - formPopUp.instance.Height - 15;
            }
            else
            {
                formPopUp.instance.Location = new Point(MousePosition.X - 15, MousePosition.Y - 15);
                if (formPopUp.instance.Right > Screen.PrimaryScreen.WorkingArea.Width)
                    formPopUp.instance.Left = MousePosition.X - formPopUp.instance.Width + 35;
                if (formPopUp.instance.Bottom > Screen.PrimaryScreen.WorkingArea.Height)
                    formPopUp.instance.Top = MousePosition.Y - formPopUp.instance.Height + 15;
                formPopUp.ptLocation = formPopUp.instance.Location;
            }
            formPopUp.instance.Show();
            formPopUp.instance.BringToFront();
        }
        enuFileExtensions eFileExtension = enuFileExtensions._num;
        int intRtxOutput_Index = -1;


        void LoadRTX(int intLbxResultsIndex)
        {
            List<classRTXInfo> lstNewRTX = new List<classRTXInfo>();
            RichTextBox rtx = rtxOutput;
            rtx.Text = "";
            intRtxOutput_Index = intLbxResultsIndex;
            if (intLbxResultsIndex >= 0 && intLbxResultsIndex < lstSearchResults_Main.Count)
            {
                classSearchResult cSearchResult = lstSearchResults_Main[intLbxResultsIndex];
                strDirectoryAndFileName = classFileContent.getFileNameAndDirectory(cSearchResult.cDictionary.strSourceDirectory, cSearchResult.strFileName);
                eFileExtension = enuFileExtensions._num;

                for (int intExtensionCounter = 0; intExtensionCounter <= (int)enuFileExtensions._num; intExtensionCounter++)
                {
                    eFileExtension = (enuFileExtensions)intExtensionCounter;
                    string strTestFileName = strDirectoryAndFileName + "." + eFileExtension.ToString();
                    if (System.IO.File.Exists(strTestFileName))
                    {
                        strDirectoryAndFileName = strTestFileName;
                        break;
                    }
                }


                switch (eFileExtension)
                {
                    case enuFileExtensions.txt:
                        {
                            classFileContent cFileContent = new classFileContent(cSearchResult.cDictionary.strSourceDirectory, cSearchResult.strFileName);
                            try
                            {
                                strHeading = cFileContent.Heading;
                                lstNewRTX.Add(getRTX(ref rtx, cFileContent.Heading.Trim(), enuRTXStyle.WordHeading));
                            }
                            catch (Exception)
                            {

                            }

                            try
                            {
                                if (cFileContent.alt_Heading != null
                                    &&
                                    cFileContent.alt_Heading.Length > 0)
                                    lstNewRTX.Add(getRTX(ref rtx, cFileContent.alt_Heading.Trim(), enuRTXStyle.WordDefinition));
                            }
                            catch (Exception)
                            {

                            }

                            try
                            {
                                lstNewRTX.Add(getRTX(ref rtx, cFileContent.Definition.Trim(), enuRTXStyle.WordDefinition));
                            }
                            catch (Exception)
                            {
                            }
                            lstRTX = lstNewRTX;
                        }
                        break;

                    case enuFileExtensions.rtf:
                        {
                            if (rtxOutput == null || rtxOutput.IsDisposed)
                            {
                                rtxOutput.LoadFile(strDirectoryAndFileName);
                            }
                            else
                            {
                                rtxOutput.LoadFile(strDirectoryAndFileName);
                                int intFirstCR = rtx.Text.IndexOf("\n");
                                if (intFirstCR > 0)
                                    strHeading = rtxOutput.Text.Substring(0, intFirstCR).Trim();

                            }
                        }
                        break;

                    case enuFileExtensions._num:
                    default:
                        { }
                        break;

                }

                Show();
                rtxOutput.Select(0, 0);
            }
        }
        enum enuRTXStyle { DictionaryHeading, WordHeading, WordDefinition };
        static classRTXInfo getRTX(ref RichTextBox rtx, string strText, enuRTXStyle eRTXStyle)
        {
            int intStart = rtx.Text.Length;
            rtx.Text += "\r\n" + strText;
            int intStop = rtx.Text.Length;

            switch (eRTXStyle)
            {
                case enuRTXStyle.DictionaryHeading:
                    return new classRTXInfo(strText, fntDictionaryHeading, Color.Red, intStart, intStop);

                case enuRTXStyle.WordDefinition:
                    return new classRTXInfo(strText, fntWordDefinition, Color.Black, intStart, intStop);

                case enuRTXStyle.WordHeading:
                    return new classRTXInfo(strText, fntWordHeading, Color.Blue, intStart, intStop);
            }
            return null;
        }

        static string _strSearchText = "";
        static public string SearchText
        {
            get { return _strSearchText; }
            set { _strSearchText = value; }
        }

        List<classSearchResult> _lstSearchResults_ = new List<classSearchResult>();
        List<string> lstHeadings = new List<string>();
        public List<classSearchResult> lstSearchResults_Main
        {
            get { return _lstSearchResults_; }
            set
            {
                switch (formWords.instance.eSearchRequestor)
                {
                    case enuSearchRequestor.clipboard:
                    case enuSearchRequestor.main:
                    case enuSearchRequestor.definition_Click:
                        {
                            _lstSearchResults_ = value;
                            lbxResults.Items.Clear();
                            rtxOutput.Text = "";

                            List<string> lstFilenames = new List<string>();
                            int intCounter = 0;
                            while (intCounter < lstSearchResults_Main.Count)
                            {
                                classSearchResult cResult = lstSearchResults_Main[intCounter];

                                if (lstFilenames.Contains(cResult.strFileName))
                                {
                                    lstSearchResults_Main.Remove(cResult);
                                }
                                else
                                {
                                    lstFilenames.Add(cResult.strFileName);
                                    lstHeadings.Add(cResult.strHeading);
                                    lbxResults.Items.Add(cResult.cDictionary.Heading.Trim() + "  :  " + cResult.strHeading.Trim());
                                    intCounter++;
                                }
                            }

                            if (lbxResults.Items.Count > 0)
                            {
                                lbxResults.SelectedIndex = 0;
                                LoadRTX(0);
                                Show();

                                if (formWords.instance.eSearchRequestor == enuSearchRequestor.main)
                                    formWords.RTX_Focused.Focus();
                                //formWords.instance.rtxCK.rtx.Focus();
                            }
                        }
                        break;


                    case enuSearchRequestor.txtDisplay_Click:
                        {
                            List<classSearchResult> lstSearchResults_Copy = value;
                            if (rtxDisplayCopy_Clicked != null && lstSearchResults_Copy.Count > 0)
                            {
                                classSearchResult cSearchResult = lstSearchResults_Copy[0];

                                string strFileNameAndDirectory = classFileContent.getFileNameAndDirectory(cSearchResult.cDictionary.strSourceDirectory, cSearchResult.strFileName);

                                for (int intExtCounter = 0; intExtCounter < (int)enuFileExtensions._num; intExtCounter++)
                                {
                                    enuFileExtensions eFileExtension = (enuFileExtensions)intExtCounter;

                                    string strFileNameDirExtension = strFileNameAndDirectory + "." + eFileExtension.ToString();
                                    if (System.IO.File.Exists(strFileNameDirExtension))
                                    {
                                        if (rtxDisplayCopy_Clicked.Tag != null)
                                        {
                                            classDefinitionCopy cDefinitionCopy = (classDefinitionCopy)rtxDisplayCopy_Clicked.Tag;
                                            //cDefinitionCopy.Filename = strFileNameDirExtension;
                                            cDefinitionCopy.Jump(strFileNameDirExtension);
                                        }

                                    }
                                }


                            }
                        }
                        break;
                }
            }
        }

        static string _strHeading = "";
        static string strHeading
        {
            get { return _strHeading; }
            set { _strHeading = value; }
        }

        List<classRTXInfo> _lstRTX = new List<classRTXInfo>();
        public List<classRTXInfo> lstRTX
        {
            get { return _lstRTX; }
            set
            {
                _lstRTX = value;
                for (int intRTXCounter = 0; intRTXCounter < lstRTX.Count; intRTXCounter++)
                {
                    classRTXInfo cRTX = lstRTX[intRTXCounter];
                    rtxOutput.SelectionStart = cRTX.start;
                    rtxOutput.SelectionLength = cRTX.stop - cRTX.start;
                    rtxOutput.SelectionColor = cRTX.clr;
                    rtxOutput.SelectionFont = cRTX.fnt;
                }
            }
        }

        public string Title
        {
            get { return Text; }
            set
            {
                Text = value;
            }
        }

        private void BtnHide_Click(object sender, EventArgs e)
        {
            Hide();
        }

        void formDictionaryOutput_LostFocus(object sender, EventArgs e)
        {

        }

        private void FormDictionaryOutput_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
                loc = Location;
        }

        void formDictionaryOutput_SizeChanged(object sender, EventArgs e)
        {
            placeObjects();
        }

        void placeObjects()
        {
            btnCopy.Top = 5;
            btnCopy.Left = Width - btnCopy.Width - 45;
            btnTopMost.Location = new Point(btnCopy.Left - btnTopMost.Width, btnCopy.Top);
            btnTopMost.Toggled = TopMost;

            txtDisplayCopies_Place();

            if (WindowState != FormWindowState.Minimized)
                sz = Size;
        }

        public void btnCopy_Delete_click(object sender, EventArgs e)
        {
            Ck_Objects.classLabelButton btnSender = (Ck_Objects.classLabelButton)sender;
            RichTextBox rtxSender = (RichTextBox)btnSender.Parent.Parent;
            Panel pnlContainer = (Panel)rtxSender.Parent;

            pnlSP.Building = true;
            {
                int intIndex = pnlSP.IndexOf(ref pnlContainer);
                if (intIndex >= 0)
                {
                    for (int intCounter = pnlSP.lstElements.Count - 1; intCounter > intIndex; intCounter--)
                    {
                        panelSP.classSweepAndPrune_Element cEle = pnlSP.lstElements[intCounter];
                        if (intCounter > 0)
                        {
                            panelSP.classSweepAndPrune_Element cEle_Prev = pnlSP.lstElements[intCounter - 1];
                            cEle.Location = cEle_Prev.Location;
                        }
                        else
                            cEle.Location = new Point();
                    }
                }
                pnlSP.Sub(ref pnlContainer);
            }
            pnlSP.Building = false;

            formWords.RTX_Focused.Focus();
        }

        int intTxtDisplayHeight = 100;
        void txtDisplayCopies_Place()
        {
            pnlSP.Building = true;
            {
                for (int intCopyCounter = 0; intCopyCounter < pnlSP.lstElements.Count; intCopyCounter++)
                {
                    panelSP.classSweepAndPrune_Element cEle = pnlSP.lstElements[intCopyCounter];

                    Panel pnlContainer = (Panel)cEle.obj;
                    pnlContainer.Width = pnlSP.Width - 17;
                }
            }

            pnlSP.Building = false;
        }

        public void txtDisplay_WordUnderMouse_PopUpDefinition(object sender, EventArgs e)
        {
            semHoverWord.WaitOne();
            if (formDictionarySelection.instance.PopUp_Dictionary != null)
            {
                RichTextBox rtxSender = (RichTextBox)sender;
                string strWordUnderMouse = classStringLibrary.getWordUnderMouse(ref rtxSender, ptRtxMouseLocation);

                if (strWordUnderMouse != null && strWordUnderMouse.Length > 0)
                {
                    if (string.Compare(strWordUnderMouse, strSearchWord) != 0)
                    {
                        strSearchWord = strWordUnderMouse;
                        List<string> lstWords = new List<string>();
                        lstWords.Add(strSearchWord);
                        classDictionary cDictionaryPopUp = formDictionarySelection.instance.PopUp_Dictionary;
                        List<classSearchParameters> lstSearchParameters = new List<classSearchParameters>();
                        lstSearchParameters.Add(new classSearchParameters(ref cDictionaryPopUp, ref lstWords, enuSearchType.Heading));
                        formWords.instance.Search(ref lstSearchParameters, enuSearchRequestor.definition_Hover);
                    }
                }
                else
                {
                    if (formPopUp.instance != null
                                &&
                        !formPopUp.instance.IsDisposed)
                        formPopUp.instance.Hide();
                }
            }
            semHoverWord.Release();
        }

        public void txtDisplay_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //SPObjects.TextDisplay txtSender = (SPObjects.TextDisplay)sender;
            //rtxDisplayCopy_Clicked = (SPObject_TxtDisplay)txtSender.Tag;
            //List<string> lstWords = new List<string>();
            //strSearchWord =  classStringLibrary.clean_nonAlpha_Ends(txtSender.WordUnderMouse);

            //int intSelectionStart = formWords.RTX_Focused.SelectionStart;
            //formWords.RTX_Focused.SelectedText   = strSearchWord;
            //formWords.RTX_Focused.Select(intSelectionStart, strSearchWord.Length);
            //formWords.RTX_Focused.Focus();
            //bolTxtDisplay_DoubleClick = true;
        }


        bool _bolTxtDisplay_DoubleClick = false;
        bool bolTxtDisplay_DoubleClick
        {
            get { return _bolTxtDisplay_DoubleClick; }
            set { _bolTxtDisplay_DoubleClick = value; }
        }


        public void txtDisplay_MouseClick(object sender, MouseEventArgs e)
        {
            if (!bolTxtDisplay_DoubleClick)
            {
                //SPObjects.TextDisplay txtSender = (SPObjects.TextDisplay)sender;
                //rtxDisplayCopy_Clicked = (SPObject_TxtDisplay)txtSender.Tag;

                //if (formDictionarySelection.ePopUpDelayTime == enuPopUpDelayTime.RightMouse_Click && e.Button == MouseButtons.Right)
                //{
                //    SPObjects.TextDisplay txtDisplay_Sender = rtxDisplayCopy_Clicked.txtDisplay;
                //    formDictionaryOutput.instance.txtDisplay_WordUnderMouse_PopUpDefinition((object)txtDisplay_Sender, new EventArgs());
                //}
                //else
                //{
                //    List<string> lstWords = new List<string>();
                //    strSearchWord = classStringLibrary.clean_nonAlpha_Ends(txtSender.WordUnderMouse);
                //    lstWords.Add(strSearchWord);
                //    classDictionary cDictionaryPopUp = rtxDisplayCopy_Clicked.cDictionary_Default;

                //    List<classSearchParameters> lstSearchParameters = new List<classSearchParameters>();
                //    lstSearchParameters.Add(new classSearchParameters(ref cDictionaryPopUp, ref lstWords, enuSearchType.Heading));
                //    formWords.instance.Search(ref lstSearchParameters, enuSearchRequestor.txtDisplay_Click);
                //}
            }
            bolTxtDisplay_DoubleClick = false;
        }

        static RichTextBox rtx = null;
        
        static RichTextBox rtxDisplayCopy_Clicked
        {
            get { return rtx; }
            set { rtx = value; }
        }


        public void txtDisplay_Clear()
        {
            pnlSP.Clear();
        }

        public void btnCopy_Click(object sender, EventArgs e)
        {
            pnlSP.Building = true;
            {
                RichTextBox rtx = new RichTextBox();
                rtx.Rtf = rtxOutput.Rtf;

                if (lbxResults.SelectedIndex >= 0 && lbxResults.SelectedIndex < lstSearchResults_Main.Count)
                {
                    string strFilename = classFileContent.getFileNameAndDirectory(lstSearchResults_Main[lbxResults.SelectedIndex].cDictionary.strSourceDirectory, lstSearchResults_Main[lbxResults.SelectedIndex].strFileName);
                    int intCounter = 0;
                    string[] strExtensions = { ".txt", ".rtf" };
                    for (intCounter = 0; intCounter < strExtensions.Length ; intCounter++)
                    {
                        if (System.IO.File.Exists(strFilename + strExtensions[ intCounter]))
                                break;
                    }


                    classDefinitionCopy cDefinitionCopy = new classDefinitionCopy(ref rtx, strFilename+strExtensions[intCounter]);
                    cDefinitionCopy.rtx = rtx;
                    cDefinitionCopy.cDictionary = lstSearchResults_Main[lbxResults.SelectedIndex].cDictionary;
                    rtx.Tag = (object)cDefinitionCopy;
                }
                else
                    rtx.Tag = (object)null;

                Panel pnlContainer = new Panel();
       
                rtx.KeyDown += RtxOutput_KeyDown;
                rtx.KeyUp += RtxOutput_KeyUp;
                rtx.KeyPress += Rtx_KeyPress;
                rtx.MouseEnter += RtxOutput_MouseEnter;
                rtx.MouseLeave += RtxOutput_MouseLeave;
                rtx.MouseDoubleClick += Rtx_MouseDoubleClick;
                rtx.MouseMove += RtxOutput_MouseMove;
                rtx.MouseUp += RtxOutput_MouseUp;
                rtx.MouseWheel += RtxOutput_MouseWheel;
                rtx.VScroll += RtxOutput_VScroll;

                pnlContainer.Size = new Size(pnlSP.Width, intTxtDisplayHeight);
                pnlContainer.Controls.Add(rtx);
                rtx.Dock = DockStyle.Fill;

                panelSP.classSweepAndPrune_Element cEleNew = pnlSP.Add(ref pnlContainer);

                if (pnlSP.lstElements.Count > 1)
                {
                    panelSP.classSweepAndPrune_Element cEleLast = pnlSP.lstElements[pnlSP.lstElements.Count - 2];
                    cEleNew.Location = new Point(0, cEleLast.recArea.Bottom);
                }
            }
            pnlSP.Building = false;
            formWords.instance.rtxCK.rtx.Focus();
        }

        private void Rtx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (formWords.RTX_Focused != null)
            {
                if (!formWords.RTX_Focused.Focused)
                    formWords.RTX_Focused.Focus();
                SendKeys.Send(e.KeyChar.ToString());
            }
        }

        void formDictionaryOutput_GotFocus(object sender, EventArgs e)
        {
            if (_frmFindReplace != null)
                _frmFindReplace.setTextBox(ref sender);
        }


        public static Size sz;
        public static Point loc;

        bool bolInit = false;

        void formDictionaryOutput_VisibleChanged(object sender, EventArgs e)
        {
        }

        void formDictionaryOutput_Disposed(object sender, EventArgs e)
        {
            recordForm();
        }

        void recordForm()
        {
            if (!bolInit)
                return;
            formInfo_Save();
        }

        enum enuFormInfo { Left, Top, Width, Height, splitterMain, splitterSub, _num };
        const string strFormInfoSplit = ".";
        void formInfo_Save()
        {
            string strOutput = "";
            for (int intCounter = 0; intCounter < (int)enuFormInfo._num; intCounter++)
            {
                enuFormInfo eFormInfo = (enuFormInfo)intCounter;
                switch (eFormInfo)
                {
                    case enuFormInfo.Height:
                        {
                            strOutput += sz.Height.ToString() + strFormInfoSplit;
                        }
                        break;

                    case enuFormInfo.Width:
                        {
                            strOutput += sz.Width.ToString() + strFormInfoSplit;
                        }
                        break;

                    case enuFormInfo.Left:
                        {
                            strOutput += loc.X.ToString() + strFormInfoSplit;
                        }
                        break;

                    case enuFormInfo.Top:
                        {
                            strOutput += loc.Y.ToString() + strFormInfoSplit;
                        }
                        break;

                    case enuFormInfo.splitterMain:
                        {
                            strOutput += sptMain.SplitterDistance.ToString() + strFormInfoSplit;
                        }
                        break;

                    case enuFormInfo.splitterSub:
                        {
                            strOutput += sptSub.SplitterDistance.ToString() + strFormInfoSplit;
                        }
                        break;
                }
            }
            System.IO.File.WriteAllText(FormInfoFileName, strOutput);
        }

        void formInfo_Load()
        {
            if (System.IO.File.Exists(FormInfoFileName))
            {
                string strFormInfo = System.IO.File.ReadAllText(FormInfoFileName);
                char[] chrSplit = strFormInfoSplit.ToArray<char>();
                string[] strInfo = strFormInfo.Split(chrSplit);
                if (strInfo.Length == (int)enuFormInfo._num + 1)
                {
                    for (int intCounter = 0; intCounter < strInfo.Length; intCounter++)
                    {
                        enuFormInfo eFormInfo = (enuFormInfo)intCounter;
                        switch (eFormInfo)
                        {
                            case enuFormInfo.Height:
                                {
                                    try
                                    {
                                        Height = Convert.ToInt32(strInfo[intCounter]);
                                    }
                                    catch (Exception)
                                    {
                                        goto FailLoad;
                                    }
                                }
                                break;

                            case enuFormInfo.Width:
                                {
                                    try
                                    {
                                        Width = Convert.ToInt32(strInfo[intCounter]);
                                    }
                                    catch (Exception)
                                    {
                                        goto FailLoad;
                                    }
                                }
                                break;

                            case enuFormInfo.Left:
                                {
                                    try
                                    {
                                        Left = Convert.ToInt32(strInfo[intCounter]);
                                    }
                                    catch (Exception)
                                    {
                                        goto FailLoad;
                                    }
                                }
                                break;

                            case enuFormInfo.Top:
                                {
                                    try
                                    {
                                        Top = Convert.ToInt32(strInfo[intCounter]);
                                    }
                                    catch (Exception)
                                    {
                                        goto FailLoad;
                                    }
                                }
                                break;


                            case enuFormInfo.splitterMain:
                                {
                                    try
                                    {
                                        sptMain.SplitterDistance = Convert.ToInt32(strInfo[intCounter]);
                                    }
                                    catch (Exception)
                                    {
                                        goto FailLoad;
                                    }
                                }
                                break;

                            case enuFormInfo.splitterSub:
                                {
                                    try
                                    {
                                        sptSub.SplitterDistance = Convert.ToInt32(strInfo[intCounter]);
                                    }
                                    catch (Exception)
                                    {
                                        goto FailLoad;
                                    }
                                }
                                break;

                        }
                    }

                }
            }
            return;
        FailLoad:
            Width = 400;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            Left = (int)(Screen.PrimaryScreen.WorkingArea.Width * .75);
            Top = 0;
        }

        string FormInfoFileName
        {
            get
            {
                return System.IO.Directory.GetCurrentDirectory() + "\\formDictionarOutput.txt";
            }
        }
    }


    public class classDefinitionCopy
    {
        public RichTextBox rtx = null;
        List<string> lstQueue = new List<string>();
        public classDictionary cDictionary = null;

        enum enuButtons { delete, forward, back, reset, _numButtons };
        Ck_Objects.classLabelButton[] btns = new Ck_Objects.classLabelButton[(int)enuButtons._numButtons];
        Panel pnlButtons = new Panel();
        public classDefinitionCopy(ref RichTextBox rtx, string strFilename)
        {
            this.rtx = rtx;
            lstQueue.Add(strFilename);
            rtx.SizeChanged += rtx_SizeChanged;
            rtx.Controls.Add(pnlButtons);
            for (int intButtonCounter = 0; intButtonCounter < (int)enuButtons._numButtons; intButtonCounter++)
            {
                enuButtons eButton = (enuButtons)intButtonCounter;
                Ck_Objects.classLabelButton btnNew = new Ck_Objects.classLabelButton();
                {
                    pnlButtons.Controls.Add(btnNew);
                    btnNew.Tag = (object)this;
                    btnNew.AutoSize = true;
                    btnNew.LocationChanged += BtnNew_LocationChanged;
                    switch (eButton)
                    {
                        case enuButtons.back:
                            btnNew.Text = "<";
                            btnNew.Click += btn_Back_Clicked;
                            break;

                        case enuButtons.forward:
                            btnNew.Text = ">";
                            btnNew.Click += btn_Forward_Clicked;
                            break;

                        case enuButtons.reset:
                            btnNew.Text = "¤";
                            btnNew.Click += btn_Reset_Clicked;
                            break;

                        case enuButtons.delete:
                            btnNew.Text = "X";
                            btnNew.Click += formDictionaryOutput.instance.btnCopy_Delete_click;
                            break;
                    }
                }
                btns[intButtonCounter] = btnNew;
            }

            placeButtons();
            Buttons_Visible();
        }

        private void BtnNew_LocationChanged(object sender, EventArgs e)
        {
            
        }

        void btn_Back_Clicked(object sender, EventArgs e)
        {
            Back();
        }

        void btn_Forward_Clicked(object sender, EventArgs e)
        {
            Forward();
        }

        void btn_Reset_Clicked(object sender, EventArgs e)
        {
            Queue_Index = 0;
        }

        public void Jump(string strFilename)
        {
            while (Queue_Index < lstQueue.Count - 1)
                lstQueue.RemoveAt(lstQueue.Count - 1);
            lstQueue.Add(strFilename);
            Queue_Index = lstQueue.Count - 1;
        }


        void Load(string strFilename)
        {
            if (System.IO.File.Exists(strFilename))
            {
                System.IO.FileInfo filInfo = new System.IO.FileInfo(strFilename);
                if (string.Compare(filInfo.Extension.ToLower(), ".txt") == 0)
                {
                    classFileContent cFileContent = new classFileContent(cDictionary.strSourceDirectory, strFilename.Substring(strFilename.Length - 12, 8));
                    if (cFileContent.Heading != null && cFileContent.Definition != null)
                    {
                        rtx.Clear();
                        classStringLibrary.RTX_AppendText(ref rtx, cFileContent.Heading.Trim(), formDictionaryOutput.fntWordHeading, Color.Blue, 0);
                        classStringLibrary.RTX_AppendNL(ref rtx);
                        if (cFileContent.alt_Heading != null)
                        {
                            classStringLibrary.RTX_AppendText(ref rtx, cFileContent.alt_Heading.Trim(), formDictionaryOutput.fntWordHeading, Color.LightBlue, 0);
                            classStringLibrary.RTX_AppendNL(ref rtx);
                        }

                        classStringLibrary.RTX_AppendText(ref rtx, cFileContent.Definition, formDictionaryOutput.fntWordDefinition, Color.Black, 0);
                        rtx.Select(0, 0);
                        rtx.ScrollToCaret();
                    }
                }
                else if (string.Compare(filInfo.Extension.ToLower(), ".rtf") == 0)
                {
                    rtx.LoadFile(strFilename);
                }
            }
        }


        string strFilename = "";
        public string Filename
        {
            get { return strFilename; }
            set
            {
                if (string.Compare(strFilename, value.ToUpper()) != 0)
                {
                    strFilename = value.ToUpper();
                    Load(strFilename);
                }
            }
        }


        int intQueue_Index = 0;
        int Queue_Index
        {
            get { return intQueue_Index; }
            set
            {
                if (value != intQueue_Index
                     && value >= 0
                     && value < lstQueue.Count)
                {
                    intQueue_Index = value;
                    Filename = lstQueue[intQueue_Index];
                    Buttons_Visible();
                }
            }
        }


        void Buttons_Visible()
        {
            btns[(int)enuButtons.back].Visible = Queue_Index > 0;
            btns[(int)enuButtons.forward].Visible = Queue_Index < lstQueue.Count - 1;
        }

        public void Back()
        {
            if (intQueue_Index > 0)
                Queue_Index -= 1;
        }

        public void Forward()
        {
            if (intQueue_Index < lstQueue.Count - 1)
                Queue_Index += 1;
        }


        public void rtx_SizeChanged(object sender, EventArgs e)
        {
            placeObjects();
        }

        void placeButtons()
        {
            int intGap = 3;
            Point ptTL = new Point(intGap, intGap);
            for (int intButtonCounter = ((int)enuButtons._numButtons)-1; intButtonCounter >=0 ; intButtonCounter--)
            {
                Ck_Objects.classLabelButton btn = btns[intButtonCounter];
                btn.Location = ptTL;
                ptTL.X += btn.Width;
            }
            pnlButtons.Width = btns[0].Right + intGap;
            pnlButtons.Height = btns[0].Bottom + intGap;
        }

        public void placeObjects()
        {
            pnlButtons.Location = new Point(rtx.Width - 24 - pnlButtons.Width, 3);
        }
    }

}

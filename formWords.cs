using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using StringLibrary;

namespace Words
{

    public enum enuSearchRequestor { main, definition_Hover, definition_Click, clipboard, txtDisplay, txtDisplay_Click, _num };
    public partial class formWords : Form
    {
        public static formWords instance;
        const int intVerticalOffset = 35;

        public ck_RichTextBox rtxCK = new ck_RichTextBox();
        RichTextBox rtb = new RichTextBox();
        static formDictionaryOutput frmDictionaryOutput;
        const string strBackupFileName = "Backup.rtf";
        public const string conStandardToolTip = "write tool tip in ToolTip_SetUp()";
        bool bolDispose = false;
        PictureBox picHelp = new PictureBox();
        public groupboxNotes grbNotes = null;
        public BackgroundWorker bckSearch = new BackgroundWorker();
        public SplitContainer splMain = new SplitContainer();
        public SplitContainer splSub = new SplitContainer();

        groupboxHeadingList grbHeadingList
        {
            get
            {
                if (groupboxHeadingList.instance == null)
                    new groupboxHeadingList();
                return groupboxHeadingList.instance;
            }
        }

        Timer tmrHeadingList = new Timer();

        Size sz;
        Point loc;
        int intsplMainSplitterDistance = 0;
        int intsplSubSplitterDistance = 0;
        int intSplHeadingListSplitterDistance = 0;

        public static bool bolInit = false;
        public formFindReplace frmFindReplace;
        Timer tmrBackup = new Timer();
        Timer tmrMessage = new Timer();
        Timer tmrPosition = new Timer();
        string strFirstRunFilename = "FirstRunFilename.txt";
        public formWords()
        {
            instance = this;
            InitializeComponent();

            Controls.Add(splMain);
            splMain.SplitterMoved += splMain_SplitterMoved;
            splMain.Dock = DockStyle.Fill;

            splSub.Orientation = Orientation.Horizontal;
            splSub.SplitterMoved += SplSub_SplitterMoved;
            splMain.Panel2.Controls.Add(splSub);
            splSub.Dock = DockStyle.Fill;
            splSub.Panel1.Controls.Add(rtxCK);
            rtb.AllowDrop = true;

            rtxCK.rtx.KeyDown += rtxCK_KeyDown;
            rtxCK.rtx.TextChanged += Rtx_TextChanged;
            rtxCK.rtx.SelectionChanged += Rtx_SelectionChanged;
            rtxCK.ToolBar.lstButtons[(int)ck_RichTextBox.panelToolBar.enuButtons.File_Load].Visible = false;
            rtxCK.ToolBar.lstButtons[(int)ck_RichTextBox.panelToolBar.enuButtons.File_SaveAs].Visible = false;
            rtxCK.ToolBar.lstButtons[(int)ck_RichTextBox.panelToolBar.enuButtons.File_New].Visible = false;
            rtxCK.File_Save = mnuFile_Save_Click;
            rtxCK.rtx.ContextMenu.MenuItems.Add( mnuSearchDictionary);
            rtxCK.rtx.ContextMenu.Tag = (object)rtxCK.rtx ;
            rtxCK.rtx.ContextMenu.Popup += ContextMenu_Popup;
            rtxCK.rtx .Tag = (object)enuSearchRequestor.main;
            splSub.Panel2.Controls.Add(grbHeadingList);
            grbHeadingList.Dock = DockStyle.Fill;
            tmrHeadingList.Interval = 500;
            tmrHeadingList.Tick += TmrHeadingList_Tick;

            bckSearch.WorkerReportsProgress = true;
            bckSearch.WorkerSupportsCancellation = true;
            bckSearch.DoWork += BckSearch_DoWork;
            bckSearch.ProgressChanged += BckSearch_ProgressChanged;
            bckSearch.RunWorkerCompleted += BckSearch_RunWorkerCompleted;

            grbNotes = new groupboxNotes();
            splMain.Panel1.Controls.Add(grbNotes);
            grbNotes.Dock = DockStyle.Fill;
            Forms_Load();

            new formFeedback();
            new classBinTrees();
            new formDictionarySelection();
            new formDictionaryOutput();
            formDictionaryOutput.instance.Hide();

            grbHeadingList.DictionarySelection_Init();

            if (!System.IO.File.Exists( strFirstRunFilename ))
            {
                cProject.FilePath
                    = PathAndFilename
                    = System.IO.Directory.GetCurrentDirectory() + "\\Short Stories.WordsProject";
                cProject.Load();
                Forms_Save();
                System.IO.File.WriteAllText(strFirstRunFilename, "Words First run");
            }
            else if (PathAndFilename.Length > 0)
            {
                cProject.FilePath = PathAndFilename.Replace(".rtf", ".xml");
                cProject.Load();
            }
            grbNotes.rebuildLst();
            ToolTip_SetUp();

            tmrMessage.Interval = 2 * 1000;
            tmrMessage.Tick += TmrMessage_Tick;

            tmrBackup.Interval = 5 * 60 * 1000;
            tmrBackup.Tick += new EventHandler(tmrBackup_Tick);
            tmrBackup.Enabled = true;

            Disposed += new EventHandler(formWords_Disposed);
            Move += new EventHandler(formWords_Move);
            SizeChanged += new EventHandler(formWords_SizeChanged);
            Activated += new EventHandler(formWords_Activated);
            VisibleChanged += new EventHandler(formWords_VisibleChanged);
            FormClosing += FormWords_FormClosing;

            tmrPosition.Interval = 5;
            tmrPosition.Tick += new EventHandler(tmrPosition_Tick);
        }

        MenuItem  mnuSearchDictionary = new MenuItem("Search Dictionary");
        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            System.Windows.Forms.ContextMenu mnuSender = (System.Windows.Forms.ContextMenu)sender;
            RichTextBox rtxSender = (RichTextBox)mnuSender.Tag;
            mnuSearchDictionary.MenuItems.Clear();
            List<string> lstWords = new List<string>();

            lstWords.Add(classStringLibrary.getWordAtSelection(ref rtxSender));
            for (int intPanelCounter = 0; intPanelCounter < formDictionarySelection.instance.lstPnlSelector.Count; intPanelCounter++)
            {
                panelSelector pnlSelector = formDictionarySelection.instance.lstPnlSelector[intPanelCounter];
                for (int intDictionaryCounter = 0; intDictionaryCounter < pnlSelector.lstToggles.Count; intDictionaryCounter++)
                {
                    char chrKey = pnlSelector.FastKey;
                    panelSelector.classButtonArray cBtnArray = pnlSelector.lstToggles[intDictionaryCounter];
                    List<classSearchParameters> lstSP = Search_GetParameters(ref cBtnArray, lstWords);
                    if (lstSP.Count > 0)
                    {
                        classSearchParameters cSearchParameter = lstSP[0];
                        if (cSearchParameter.cDictionary != null)
                        {
                            MenuItem mnuNew = new MenuItem();
                            mnuNew.Text = cSearchParameter.cDictionary.Heading;
                            mnuNew.Click += mnuSearchDictionary_click;
                            mnuNew.Tag = (object)lstSP;
                            mnuSearchDictionary.MenuItems.Add(mnuNew);
                        }
                    }
                }
                rtxCK.rtx.ContextMenu.MenuItems.Add(mnuSearchDictionary);
            }

            MenuItem mnuAutoSave = new MenuItem("Auto Save", mnuAutoSave_Click);
            mnuAutoSave.Checked = AutoSave;
            rtxCK.rtx.ContextMenu.MenuItems.Add(mnuAutoSave);
        }
        void mnuAutoSave_Click(object sender, EventArgs e)
        {
            AutoSave = !AutoSave;
        }
        void mnuSearchDictionary_click(object sender, EventArgs e)
        {
           MenuItem mnuSender = (MenuItem)sender;
            List<classSearchParameters> lstSearchParameters_New = (List<classSearchParameters>)mnuSender.Tag;
            Search(ref lstSearchParameters_New,  enuSearchRequestor.definition_Click );
        }

        private void SplSub_SplitterMoved(object sender, SplitterEventArgs e)
        {
            placeObjects();
        }

        private void Rtx_SelectionChanged(object sender, EventArgs e)
        {
            tmrHeadingList_Reset();
        }

        void tmrHeadingList_Reset()
        {
            tmrHeadingList.Enabled = false;
            tmrHeadingList.Enabled = grbHeadingList.Visible;
        }

        private void TmrHeadingList_Tick(object sender, EventArgs e)
        {
            Timer tmrSender = (Timer)sender;
            tmrSender.Enabled = false;
            if (grbHeadingList.Visible)
            {
                RichTextBox rtxTemp = rtxCK.rtx;
                string strWordUnderCursor = classStringLibrary.getWordAtSelection(ref rtxTemp).Trim();
                if (strWordUnderCursor.Length > 0)
                    grbHeadingList.JumpTo(strWordUnderCursor);
            }
        }

        
        private void FormWords_FormClosing(object sender, FormClosingEventArgs e)
        {
             TextChangedAndNotSaved(MessageBoxButtons.YesNo);
            if (classHeadingList.instance != null && classHeadingList.instance.bck_BuildHeadingList.IsBusy)
                classHeadingList.instance.bck_BuildHeadingList.CancelAsync();
        }

        public static void Message(string strMessageNew)
        {
            lstMessages.Add(strMessageNew);
            if (!instance.tmrMessage.Enabled)
                instance.TmrMessage_Tick((object)instance.tmrMessage, new EventArgs());
        }

        static List<string> lstMessages = new List<string>();
        private void TmrMessage_Tick(object sender, EventArgs e)
        {
            if (lstMessages.Count > 0)
            {
                Text = lstMessages[0];
                lstMessages.RemoveAt(0);

                tmrMessage.Enabled = true;
            }
            else
            {
                Text = Title;

                tmrMessage.Enabled = false;
            }

        }

        private void splMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            placeObjects();
        }

        static RichTextBox _RTX_Focused = null;
        public static RichTextBox RTX_Focused
        {
            get
            {
                if (_RTX_Focused == null) return instance.rtxCK.rtx;
                return _RTX_Focused;
            }
            set { _RTX_Focused = value; }
        }

        public string Path
        {
            get
            {
                string strPath = System.IO.Path.GetFullPath(PathAndFilename);
                string strFilename = System.IO.Path.GetFileName(PathAndFilename);
                string strRetVal = strPath.Substring(0, strPath.Length - strFilename.Length);
                return strRetVal;
            }
        }

                public string PathAndFilename
        {
            get
            {
                return cProject.FilePath;
            }
            set
            {
                cProject.FilePath = value;
                Text = Title;
            }
        }


        public string ProjectFileName
        {
            get
            {

                string strFilename = System.IO.Path.GetFileName(PathAndFilename);
                string strExtension = System.IO.Path.GetExtension(PathAndFilename);

                return strFilename.Substring(0, strFilename.Length - strExtension.Length);// + ":" + cProject.Heading_Current;
            }
        }

        public string Title
        {
            get
            {
                return ProjectFileName + " : " + cProject.Heading_Current;
            }
        }

        public void Title_Draw()
        {
            Text = Title;
        }

        static classProject _cProject = new classProject();
        public static classProject cProject
        {
            get { return _cProject; }
            set { _cProject = value; }
        }


        form_Busy frmBusy = null;
        List<classSearchResult> lstSearchResults = new List<classSearchResult>();
        public List<classSearchParameters> lstSearchParameters;
        public enuSearchRequestor eSearchRequestor = enuSearchRequestor._num;
        public void Search(ref List<classSearchParameters> _lstSearchParameters, enuSearchRequestor _eSearchRequestor)
        {
            if (bckSearch.IsBusy) return;
            lstSearchParameters = _lstSearchParameters;
            if (lstSearchParameters == null || lstSearchParameters.Count == 0)
                return;

            lstSearchParameters = _lstSearchParameters;
            eSearchRequestor = _eSearchRequestor;

            if (frmBusy == null)
            {
                frmBusy = new form_Busy();
                frmBusy.Location = new Point(10, 10);
                frmBusy.ForeColor = Color.White;
                frmBusy.Font = new Font("roman", 12, FontStyle.Bold);
                ContextMenu cmuBusy = new ContextMenu();
                MenuItem mnuAbortSearch = new MenuItem("Abort Search", frmBusy_AbortSearch_Click);
                cmuBusy.MenuItems.Add(mnuAbortSearch);
                frmBusy.ContextMenu = cmuBusy;
            }

            frmBusy.Show();
            frmBusy.Start();
            bckSearch.RunWorkerAsync();
        }

        void frmBusy_AbortSearch_Click(object sender, EventArgs e)
        {
            bckSearch.CancelAsync();
        }

        private void BckSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            setSearchProgress(enuSearchProgress.Display);
            if (formDictionaryOutput.instance == null)
                new formDictionaryOutput();
            switch (eSearchRequestor)
            {
                case enuSearchRequestor.definition_Hover:
                    {
                        formDictionaryOutput.instance.lstHoverSearchResults = lstSearchResults;
                    }
                    break;

                case enuSearchRequestor.txtDisplay_Click:
                    {
                        formDictionaryOutput.instance.lstSearchResults_Main = lstSearchResults;
                    }
                    break;


                case enuSearchRequestor.definition_Click:
                case enuSearchRequestor.main:
                case enuSearchRequestor.txtDisplay:
                    {
                        string strWordFound = strClickedWord;
                        string strTemp = strClickedWord.ToUpper();

                        if (classBinTrees.Prefix.Length > 0 && strTemp.Length > classBinTrees.Prefix.Length)
                        {
                            if (string.Compare(strTemp.Substring(0, classBinTrees.Prefix.Length), classBinTrees.Prefix.ToUpper()) == 0)
                            {
                                strWordFound = strWordFound.Substring(classBinTrees.Prefix.Length);
                            }
                        }

                        if (classBinTrees.Suffix.Length > 0 && strTemp.Length > classBinTrees.Suffix.Length)
                        {
                            if (string.Compare(strTemp.Substring(strTemp.Length - classBinTrees.Suffix.Length), classBinTrees.Suffix.ToUpper()) == 0)
                            {
                                strWordFound = strWordFound.Substring(0, strWordFound.Length - classBinTrees.Suffix.Length);
                            }
                        }


                        Output(lstSearchResults,
                               "Search (" + lstSearchResults.Count.ToString() + ") : "
                               + (classBinTrees.Prefix.Length > 0
                                                              ? (classBinTrees.Prefix + " + ")
                                                              : "")
                               + strWordFound
                               + (classBinTrees.Suffix.Length > 0
                                                              ? (" + " + classBinTrees.Suffix)
                                                              : "")
                               );
                    }
                    break;

                case enuSearchRequestor.clipboard:
                    {
                        // /*
                        formDictionarySelection.instance.lstSearchResults_Clipboard = lstSearchResults;
                        /*/
                        Output(lstSearchResults, "Search (" + lstSearchResults.Count.ToString() + ") : " + strClickedWord);
                        // */
                    }
                    break;


            }
            //if (formDictionaryOutput.rtxCalling != null)
            //    formDictionaryOutput.rtxCalling.Focus();
            //else 
            //rtxCK.rtx.Focus();

            formDictionaryOutput.rtxCalling = null;

            frmBusy.Hide();
            frmBusy.Stop();
        }

        private void BckSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > (int)enuSearchProgress.idle && e.ProgressPercentage <= (int)enuSearchProgress.Display)
                setSearchProgress((enuSearchProgress)e.ProgressPercentage);
        }

        void setSearchProgress(enuSearchProgress eSearchProgress)
        {
            frmBusy.Text = eSearchProgress.ToString();
            Color[] clr = { Color.White, Color.Red, Color.Yellow, Color.Green, Color.Purple };
            frmBusy.ForeColor = clr[(int)eSearchProgress];
        }

        public enum enuSearchProgress { idle, Search, Sort, Display, _num };
        public enuSearchProgress eSearchProgress = enuSearchProgress.idle;

        private void BckSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            lstSearchResults = new List<classSearchResult>();
            string strSearchText = "";
            if (lstSearchParameters.Count > 0)
                for (int intCounter = 0; intCounter < lstSearchParameters[0].lstWords.Count; intCounter++)
                    strSearchText += (intCounter > 0 ? " " : "") + lstSearchParameters[0].lstWords[intCounter].Trim();
            formDictionaryOutput.SearchText = strSearchText;

            bckSearch.ReportProgress((int)enuSearchProgress.Search);

            lstSearchResults.Clear();
            List<string> lstFileNames = new List<string>();

            List<classSearchResult> lstBestResults = new List<classSearchResult>();
            List<classSearchResult> lstResults = new List<classSearchResult>();

            for (int intParameterCounter = 0; intParameterCounter < lstSearchParameters.Count; intParameterCounter++)
            {
                List<List<class_LL_Record>> lstLL = new List<List<class_LL_Record>>();
                classSearchParameters cParameters = lstSearchParameters[intParameterCounter];
                List<string> lstWords = cParameters.lstWords;
                classDictionary cDictionary = cParameters.cDictionary;

                int intBoolean = 0, intAltBoolean = 1;

                List<enuFileDataTypes> lstDataType = new List<enuFileDataTypes>();
                switch (cParameters.eSearchType)
                {
                    case enuSearchType.Content:
                        {
                            lstDataType.Add(enuFileDataTypes.content);
                        }
                        break;

                    case enuSearchType.Heading:
                        {
                            lstDataType.Add(enuFileDataTypes.heading);
                        }
                        break;

                    case enuSearchType.Both_Heading_And_Content:
                        {
                            lstDataType.Add(enuFileDataTypes.heading);
                            lstDataType.Add(enuFileDataTypes.content);
                        }
                        break;
                }

                for (int intDataTypeCounter = 0; intDataTypeCounter < lstDataType.Count; intDataTypeCounter++)
                {
                    if (bckSearch.CancellationPending)
                    {
                        lstSearchResults.Clear();
                        return;
                    }
                    for (int intWordCounter = 0; intWordCounter < lstWords.Count; intWordCounter++)
                    {
                        List<class_LL_Record> lstLL_Intermediate = classBinTrees.BinTree_Search(ref cDictionary, lstWords[intWordCounter], lstDataType[intDataTypeCounter]);
                        if (lstLL_Intermediate != null && lstLL_Intermediate.Count > 0)
                        {
                            lstLL.Add(lstLL_Intermediate);
                        }
                    }

                    if (lstLL.Count == 1)
                    {
                        for (int intResCounter = 0; intResCounter < lstLL[0].Count; intResCounter++)
                        {
                            class_LL_Record cLL = lstLL[0][intResCounter];
                            if (!lstFileNames.Contains(cLL.FileName))
                            {
                                classSearchResult cRes = new classSearchResult(ref cDictionary, ref cLL);
                                lstSearchResults.Add(cRes);
                                lstFileNames.Add(cLL.FileName);
                            }
                        }
                    }

                    //if (lstLL.Count >= 1)
                    if (lstLL.Count > 1)
                    {
                        IEnumerable<List<class_LL_Record>> queryLLResults = lstLL.OrderBy(lst => lst.Count);
                        lstLL = (List<List<class_LL_Record>>)queryLLResults.ToList<List<class_LL_Record>>();

                        AlphaTrees.cAlphaTree[] cAT = new AlphaTrees.cAlphaTree[2];


                        List<class_LL_Record> lstLLRec = lstLL[0];
                        lstLL.Remove(lstLL[0]);

                        cAT[intBoolean] = new AlphaTrees.cAlphaTree();
                        while (lstLLRec.Count > 0)
                        {
                            int intRND = Math3.classRND.Get_Int(0, lstLLRec.Count - 1);
                            class_LL_Record cLL = lstLLRec[intRND];
                            lstLLRec.Remove(cLL);
                            if (!lstFileNames.Contains(cLL.FileName))
                            {

                                object objLL = (object)cLL;
                                cAT[intBoolean].Insert(ref objLL, cLL.FileName);

                                lstFileNames.Add(cLL.FileName);
                            }
                        }

                        for (int intListCounter = 0; intListCounter < lstLL.Count; intListCounter++)
                        {
                            List<class_LL_Record> lst = lstLL[intListCounter];
                            cAT[intAltBoolean] = new AlphaTrees.cAlphaTree();

                            while (lst.Count > 0)
                            {
                                int intRND = Math3.classRND.Get_Int(0, lst.Count - 1);
                                class_LL_Record cLL = lst[intRND];
                                lst.Remove(cLL);
                                if (cAT[intBoolean].Search(cLL.FileName) != null)
                                {
                                    object objLL = (object)cLL;
                                    cAT[intAltBoolean].Insert(ref objLL, cLL.FileName);
                                }
                            }
                            intBoolean = intAltBoolean;
                            intAltBoolean = (intBoolean + 1) % 2;
                        }

                        if (cAT[intAltBoolean] != null)
                        {
                            List<AlphaTrees.cAlphaTree.classTraversalReport_Record> lstObjResults = cAT[intBoolean].TraverseTree_InOrder();
                            for (int intResCounter = 0; intResCounter < lstObjResults.Count; intResCounter++)
                            {
                                object objLL = (object)lstObjResults[intResCounter].data;
                                class_LL_Record cLL = (class_LL_Record)objLL;
                                classSearchResult cRes = new classSearchResult(ref cDictionary, ref cLL);
                                lstSearchResults.Add(cRes);
                            }
                        }
                    }
                }
                if (lstDataType.Count > 0)
                    bckSearch.ReportProgress((int)enuSearchProgress.Sort);

                ///*
                IEnumerable<classSearchResult> query = lstSearchResults.OrderBy(result => result.strHeading);

                lstBestResults = (List<classSearchResult>)query.ToList<classSearchResult>();
                /*/

                for (int intCounter = 0; intCounter < lstSearchResults.Count; intCounter++)
                {
                    if (bckSearch.CancellationPending)
                    {
                        lstSearchResults.Clear();
                        return; 
                    }
                    classSearchResult cResult = lstSearchResults[intCounter];
                    cResult.cFileContent = new classFileContent(cResult.cDictionary.strSourceDirectory, cResult.strFileName);
                    string strHeading = classStringLibrary.cleanFront_nonAlpha(cResult.cFileContent.Heading.Trim());
                    if (string.Compare(strHeading, strSearchText) == 0)
                        lstBestResults.Insert(0, cResult);
                    else if (string.Compare(strHeading.ToLower(), strSearchText.ToLower()) == 0)
                        lstBestResults.Add(cResult);
                    else
                        lstResults.Add(cResult);
                }
                IEnumerable<classSearchResult> query = lstBestResults.OrderBy(result => result.cFileContent.Heading);
                lstBestResults = (List<classSearchResult>)query.ToList<classSearchResult>();
                //*/

            }
            lstSearchResults.Clear();

            lstSearchResults.AddRange(lstBestResults);
            lstSearchResults.AddRange(lstResults);
            bckSearch.ReportProgress((int)enuSearchProgress.Display);

            return;
        }

        public void Output(List<classSearchResult> lstSearchResults, string strTitle)
        {
            if (frmDictionaryOutput == null)
                initOutput();

            frmDictionaryOutput.lstSearchResults_Main = lstSearchResults;
            frmDictionaryOutput.Text = strTitle;
        }


        void initOutput()
        {
            frmDictionaryOutput = new formDictionaryOutput();
            frmDictionaryOutput.Disposed += new EventHandler(formDictionaryOutput_Disposed);
        }

        void formDictionaryOutput_Disposed(object sender, EventArgs e)
        {
            if (bolDispose)
                return;
            initOutput();
        }

        public List<classSearchParameters> Search_GetParameters(ref panelSelector.classButtonArray cBtnArray, List<string> lstWords)
        {
            List<classSearchParameters> lstRetVal = new List<classSearchParameters>();

            bool bolContent = false;
            bool bolHeading = false;

            switch (cBtnArray.eSearchType)
            {
                case enuSearchType.Both_Heading_And_Content:
                    bolContent
                        = bolHeading
                        = true;
                    break;

                case enuSearchType.Content:
                    bolContent = true;
                    break;

                case enuSearchType.Heading:
                    bolHeading = true;
                    break;
            }
            classDictionary cDictionary = cBtnArray.cDictionary;

            int intMaxWords = 4;
            if (lstWords.Count > intMaxWords)
                lstWords.RemoveRange(intMaxWords, lstWords.Count - intMaxWords);

            if (bolHeading && bolContent)
                lstRetVal.Add(new classSearchParameters(ref cDictionary, ref lstWords, enuSearchType.Both_Heading_And_Content));
            else if (bolHeading)
                lstRetVal.Add(new classSearchParameters(ref cDictionary, ref lstWords, enuSearchType.Heading));
            else if (bolContent)
                lstRetVal.Add(new classSearchParameters(ref cDictionary, ref lstWords, enuSearchType.Content));

            return lstRetVal;
        }

        public string strClickedWord = "";

        void LBX_Scroll_Down()
        {
            if (formDictionaryOutput.instance != null)
                formDictionaryOutput.instance.lbxResults_ScrollDown();
        }

        void LBX_Scroll_Up()
        {
            if (formDictionaryOutput.instance != null)
                formDictionaryOutput.instance.lbxResults_ScrollUp();
        }

        public void rtxCK_KeyDown(object sender, KeyEventArgs e)
        {
            RTX_Focused = (RichTextBox)sender;

            bool bolDebug = false;
            if (bolDebug)
            {
                classDictionary cDictionary = grbHeadingList.cDictionary;
                
                int intNumEntries = classHeadingList.NumEntries(ref cDictionary);

                for (int intCounter = 0; intCounter < intNumEntries; intCounter++)
                {
                    classHeadingList.classHeadingList_Record cWR = classHeadingList.BuildHeadingList_Load(ref cDictionary, intCounter);
                    System.Diagnostics.Debug.Print(cWR.strHeading);
                }
            }

            if (RTX_Focused == rtxCK.rtx && e.Modifiers == (Keys.Control | Keys.Shift | Keys.Alt))
            { //        Heading List Key controls
                switch (e.KeyCode)
                {
                    case Keys.A:
                        //WordHeading scroll up
                        grbHeadingList.Scroll_Up();
                        return;

                    case Keys.Z:
                        //WordHeading scroll down
                        grbHeadingList.Scroll_Down();
                        return;

                    case Keys.Q:
                        {       // insert selected WordHeading
                            classHeadingList.classHeadingList_Record cHLRec = grbHeadingList.cSelected;
                            if (cHLRec != null)
                            {
                                RichTextBox rtxTemp = rtxCK.rtx;
                                classStringLibrary.RTX_SelectWordUnderMouse(ref rtxTemp);
                                rtxTemp.SelectedText = cHLRec.strHeading;
                            }
                        }
                        return;
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Alt))
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        LBX_Scroll_Up();
                        return;

                    case Keys.Z:
                        LBX_Scroll_Down();
                        return;

                    case Keys.Q:
                        if (formDictionaryOutput.instance != null && !formDictionaryOutput.instance.IsDisposed)
                        {
                            formDictionaryOutput.instance.btnCopy_Click((object)this, new EventArgs());
                            RTX_Focused.Focus();
                        }
                        return;

                    case Keys.X:
                        if (formDictionaryOutput.instance != null && !formDictionaryOutput.instance.IsDisposed)
                        {
                            formDictionaryOutput.instance.txtDisplay_Clear();
                            RTX_Focused.Focus();
                        }
                        return;

                }
            }

            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        if (e.Alt)
                        {
                            LBX_Scroll_Down();
                            return;
                        }
                    }
                    break;

                case Keys.Up:
                    {
                        if (e.Alt)
                        {
                            LBX_Scroll_Up();
                            return;
                        }
                    }
                    break;

                default:
                    {
                        if (e.Modifiers != Keys.Shift && e.Modifiers != Keys.Control && e.Modifiers != Keys.Alt)
                        {
                            Type typeBox = sender.GetType();
                            RichTextBox rtx = (RichTextBox)sender;
                            strClickedWord = rtx.SelectionLength == 0
                                                                 ? classStringLibrary.getClickedWord(ref rtx)
                                                                 : classStringLibrary.getWordAtSelection(ref rtx);

                            List<string> lstWords = classStringLibrary.getFirstWords(strClickedWord);
                            char chrKey = (char)e.KeyValue;

                            if (lstWords != null && e.Control && e.Shift && lstWords.Count > 0)
                            {
                                RichTextBox rtxRef = (RichTextBox)sender;
                                Search(lstWords, ref rtxRef, chrKey);
                            }
            tmrHeadingList_Reset();
                        }
                    }
                    break;
            }


        }
        public void Search(List<string> lstWords, ref RichTextBox rtxCalling, char chrKey)
        {
            List<classSearchParameters> lstSearchParameters_New = new List<classSearchParameters>();
            for (int intPanelCounter = 0; intPanelCounter < formDictionarySelection.instance.lstPnlSelector.Count; intPanelCounter++)
            {
                panelSelector pnlSelector = formDictionarySelection.instance.lstPnlSelector[intPanelCounter];
                for (int intDictionaryCounter = 0; intDictionaryCounter < pnlSelector.lstToggles.Count; intDictionaryCounter++)
                {
                    if (pnlSelector.FastKey == chrKey)
                    {
                        panelSelector.classButtonArray cBtnArray = pnlSelector.lstToggles[intDictionaryCounter];
                        List<classSearchParameters> lstSP = Search_GetParameters(ref cBtnArray, lstWords);
                        if (lstSP != null && lstSP.Count > 0)
                        {
                            lstSearchParameters_New.AddRange(lstSP);
                        }
                    }
                }
            }

            if (lstSearchParameters_New.Count > 0)
            {
                formDictionaryOutput.rtxCalling = rtxCalling;
                enuSearchRequestor eSearchRequestor = (enuSearchRequestor)rtxCalling.Tag;
                switch (eSearchRequestor)
                {
                    case enuSearchRequestor.main:
                    case enuSearchRequestor.definition_Hover:
                        Search(ref lstSearchParameters_New, eSearchRequestor);
                        break;


                    default:
                        MessageBox.Show("unhandled search");
                        break;
                }
                Search(ref lstSearchParameters, eSearchRequestor);
            }
        }

        /// <summary>
        /// classToolTip allows multiple tips to be assigned to each control
        /// </summary>
        public classToolTip cTTips = new classToolTip();

        /// <summary>
        /// manages all tool tips in SpriteEditor_2017
        /// </summary>
        public ToolTip toolTip = new ToolTip();

        /// <summary>
        /// initializes an instance of classToolTip with all the tips for each control
        /// </summary>
        public void ToolTip_SetUp()
        {
            cTTips.Tip_Clear_All();

            // Set up the delays for the ToolTip.
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 100;
            toolTip.UseAnimation = true;
            toolTip.Popup += ToolTip_Popup;

            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip.ShowAlways = true;

            #region Word
            // 
            string[] strTips = {
                "",
                ""};

            #endregion

            #region Notes
            toolTip.SetToolTip(grbNotes.lbxNotes, formWords.conStandardToolTip);
            toolTip.SetToolTip(grbNotes.rtxNotes, formWords.conStandardToolTip);

            // list - notes
            strTips = new string[] {
                "you can select the note you wish to view or edit here",
                "move any notes up or down in this list by first highlighting them and then pressing the up or down arrows",
                "these notes can be moved using the up/down arrows",
                "you can scroll the mouse to move these notes up or down"};
            for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                cTTips.Tip_set(grbNotes.lbxNotes.Name, strTips[intCounter]);

            // textbox note
            strTips = new string[]{
                "type your notes in this text box",
                "these notes are saved automatically when you save your project",
                "typing the '#' key here will insert a numeric entry for a list in this note",
                "pressing the '#' key inside the numeric bullet will renumber your list to keep it in order",
                "you can erase a numeric-list entry or cut/paste them into a different order then reset them by typing '#' inside a numeric bullet"};
            for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                cTTips.Tip_set(grbNotes.rtxNotes.Name, strTips[intCounter]);
            #endregion

            #region DictionarySelector 
            if (formDictionarySelection.instance != null)
            {
                formDictionarySelection.instance.lbtnAdd.Name = "formDictionarySelection_lbtnAdd";
                formDictionarySelection.instance.lbtnHide.Name = "formDictionarySelection_lbtnHide";
                toolTip.SetToolTip(formDictionarySelection.instance.lbtnAdd, formWords.conStandardToolTip);
                toolTip.SetToolTip(formDictionarySelection.instance.lbtnHide, formWords.conStandardToolTip);

                // button add note
                strTips = new string[]{
                "press this button to create a new dictionary selection",
                "you can program the Words app to search dictionary combinations by clicking here"};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("formDictionarySelection_lbtnAdd", strTips[intCounter]);

                // button Hide note
                strTips = new string[]{
                "hide this form by clicking this button",
                "clicking this button will hide this form - use the Options menu in the main form to bring it back "};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("formDictionarySelection_lbtnHide", strTips[intCounter]);

                // btn delete
                strTips = new string[]{
                "press this button to delete this dictionary selection",
                "clicking here will magically make this box disappear"};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("panelSelector_btnDelete", strTips[intCounter]);

                // label fastkey note
                strTips = new string[]{
                "pressing the control-key combination described here will summon the results of this dictionary selection's search",
                "you can program your Words app to search dictionary entries by setting this control-key combination"};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("panelSelector_lblFastKey", strTips[intCounter]);

                // textbox FastKey note
                strTips = new string[]{
                "enter the letter that will combine with the Control-key to make a quick dictionary search",
                "you can program the Words App to search the dictionaries selected in this list by typing a letter here"};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("panelSelector_txtFastKey", strTips[intCounter]);

                // label cursor note
                strTips = new string[]{
                "i have no idea what this label cursor is supposed to do",
                "this is the label cursor figure it out"};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("panelSelector_lblCursor", strTips[intCounter]);

                // panel scrolling note
                strTips = new string[]{
                "the scrolling panel lists something something that i forget",
                "scrolling panel note - write it in later"};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("panelSelector_pnlScrolling", strTips[intCounter]);

                // checkbox UseClipBoard note
                strTips = new string[]{
                "by selecting this option you tell the Words App to search words you 'copy' using MS Copy/Cut options",
                "you can search your selected dictionaries for words which are copied to the MS clipboard by highlighting text and pressing Ctrl-C or Ctrol-X"};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("panelSelector_chkUseClipBoard", strTips[intCounter]);

                // (Dictionary) cSelector note
                strTips = new string[]{
                    "this list of dictionaries allows your to select any combination of dictionaries to search with a single control-key combination",
                    "you can select any combination of dictionaries to help you find what you're looking for" };
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("panelSelector_cSelector", strTips[intCounter]);

                // "classRDBArray_lbl"
                strTips = new string[]{
                "you can select any of the four options for this particular dictionary : do-not-search, search heading only, search-content only, search both heading/content",
                "you can set your search preference for each dictionary independently using the radio buttons on the left of the dictionary names listed here"};
                for (int intCounter = 0; intCounter < strTips.Length; intCounter++)
                    cTTips.Tip_set("classRDBArray_lbl", strTips[intCounter]);



            }
            #endregion 
        }


        /// <summary>
        /// prevents tool tip from giving the same control a tip every mousemove over that control
        /// </summary>
        object objAssociatedControl = null;
        /// <summary>
        /// stops edits tool tip with the multiple tips available for a given control
        /// </summary>
        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            if (e.AssociatedControl == objAssociatedControl
                || e.AssociatedControl == null
                || e.AssociatedControl.Name.Length == 0) return;
            objAssociatedControl = e.AssociatedControl;
            string strName = e.AssociatedControl.Name;
            string strTip = cTTips.Tip_get(strName);

            if (strTip.Length > 0)
                toolTip.SetToolTip(e.AssociatedControl, strTip);
        }
        void formWords_VisibleChanged(object sender, EventArgs e)
        {
            tmrPosition.Enabled = Visible;
        }

        void tmrPosition_Tick(object sender, EventArgs e)
        {
            tmrPosition.Enabled = false;
            Size = sz;
            Location = loc;
            bolInit = true;
        }

        void formWords_Disposed(object sender, EventArgs e)
        {
            if (classBinTrees.bck_BuildBinSearch != null
                && classBinTrees.bck_BuildBinSearch.IsBusy)
            {
                classBinTrees.bck_BuildBinSearch.CancelAsync();
            }

            if (formDictionarySelection.instance != null
                &&
                !formDictionarySelection.instance.IsDisposed)
                formDictionarySelection.instance.DictionarySelections_Save();

            cProject.Save();
            grbNotes.bolDie = true;
            Forms_Save();
        }

        public void mnuOptions_DictionarySelection_Click(object sender, EventArgs e)
        {
            if (formDictionarySelection.instance == null || formDictionarySelection.instance.IsDisposed)
                new formDictionarySelection();
            formDictionarySelection.instance.Show();
        }

        string getResourceDirectory()
        {
            return System.IO.Directory.GetCurrentDirectory().ToUpper().Replace("BIN\\DEBUG", "").Replace("BIN\\RELEASE", "");
        }

        string GetFormsFileName()
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\forms_20210816.bfs";
        }

        public void Forms_Save()
        {
            string strResourcesDirectoryAndFileName = GetFormsFileName();
            FileStream fs;
            if (System.IO.File.Exists(strResourcesDirectoryAndFileName))
                fs = new FileStream(strResourcesDirectoryAndFileName, FileMode.Open);
            else
                fs = new FileStream(strResourcesDirectoryAndFileName, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            fs.Position = 0;
            if (PathAndFilename == null)
                PathAndFilename = "";
            formatter.Serialize(fs, PathAndFilename);

            formatter.Serialize(fs, sz.Width);
            formatter.Serialize(fs, sz.Height);
            formatter.Serialize(fs, loc.X);
            formatter.Serialize(fs, loc.Y);

            formatter.Serialize(fs, sz.Width);
            formatter.Serialize(fs, sz.Height);
            formatter.Serialize(fs, loc.X);
            formatter.Serialize(fs, loc.Y);

            formatter.Serialize(fs, splMain.SplitterDistance);
            formatter.Serialize(fs, splSub.SplitterDistance);
            formatter.Serialize(fs, grbHeadingList.splMain_Distance);
            formatter.Serialize(fs, groupboxNotes.intSpc_distance);
            
            formatter.Serialize(fs, formFindReplace.loc.X);
            formatter.Serialize(fs, formFindReplace.loc.Y);

            formatter.Serialize(fs, (bool)AutoSave);

            formatter.Serialize(fs, (bool)rtxCK.HighlighterLabel_Hide_Automatically);
            formatter.Serialize(fs, (bool)grbNotes.rtxNotes.HighlighterLabel_Hide_Automatically );
            for (int intHighLighterColorCounter = 0; intHighLighterColorCounter < rtxCK.clrHighlighter.Length; intHighLighterColorCounter++)
            {
                ck_RichTextBox.classHighlighterColorItem cHighlighterColorItem = rtxCK.clrHighlighter[intHighLighterColorCounter];
                formatter.Serialize(fs, cHighlighterColorItem.clrBack.A);
                formatter.Serialize(fs, cHighlighterColorItem.clrBack.R);
                formatter.Serialize(fs, cHighlighterColorItem.clrBack.G);
                formatter.Serialize(fs, cHighlighterColorItem.clrBack.B);

                formatter.Serialize(fs, cHighlighterColorItem.clrFore.A);
                formatter.Serialize(fs, cHighlighterColorItem.clrFore.R);
                formatter.Serialize(fs, cHighlighterColorItem.clrFore.G);
                formatter.Serialize(fs, cHighlighterColorItem.clrFore.B);
                
                formatter.Serialize(fs, cHighlighterColorItem.Text);
                formatter.Serialize(fs, cHighlighterColorItem.valid);
            }

            formatter.Serialize(fs, groupboxHeadingList.DefaultDictionaryIndex);

            fs.Close();
        }

        void Forms_Load()
        {
            FileStream fs;
            string PathAndFilenameAndDirectory = GetFormsFileName();
            if (System.IO.File.Exists(PathAndFilenameAndDirectory))
            {
                fs = new FileStream(PathAndFilenameAndDirectory, FileMode.Open);
                fs.Position = 0;

                BinaryFormatter formatter = new BinaryFormatter();
                PathAndFilename = (string)formatter.Deserialize(fs);

                sz.Width = (int)formatter.Deserialize(fs);
                sz.Height = (int)formatter.Deserialize(fs);
                loc.X = (int)formatter.Deserialize(fs);
                if (loc.X < 0) loc.X = 0;
                loc.Y = (int)formatter.Deserialize(fs);
                if (loc.Y < 0) loc.Y = 0;

                sz.Width = (int)formatter.Deserialize(fs);
                sz.Height = (int)formatter.Deserialize(fs);
                loc.X = (int)formatter.Deserialize(fs);
                if (loc.X < 0) loc.X = 0;
                loc.Y = (int)formatter.Deserialize(fs);
                if (loc.Y < 0) loc.Y = 0;

                intsplMainSplitterDistance = (int)formatter.Deserialize(fs);
                intsplSubSplitterDistance = (int)formatter.Deserialize(fs);
                intSplHeadingListSplitterDistance = (int)formatter.Deserialize(fs);
                groupboxNotes.intSpc_distance = (int)formatter.Deserialize(fs);

                formFindReplace.loc.X = (int)formatter.Deserialize(fs);
                if (formFindReplace.loc.X < 0) formFindReplace.loc.X = 0;
                formFindReplace.loc.Y = (int)formatter.Deserialize(fs);
                if (formFindReplace.loc.Y < 0) formFindReplace.loc.Y = 0;

                AutoSave = (bool)formatter.Deserialize(fs);

                rtxCK.HighlighterLabel_Hide_Automatically = (bool)formatter.Deserialize(fs);
                grbNotes.rtxNotes.HighlighterLabel_Hide_Automatically = (bool)formatter.Deserialize(fs);
                for (int intHighlighter_ColorCounter = 0; intHighlighter_ColorCounter < rtxCK.clrHighlighter.Length; intHighlighter_ColorCounter++)
                {
                    byte bytClrBackHighlight_A = (byte)formatter.Deserialize(fs);
                    byte bytClrBackHighlight_R = (byte)formatter.Deserialize(fs);
                    byte bytClrBackHighlight_G = (byte)formatter.Deserialize(fs);
                    byte bytClrBackHighlight_B = (byte)formatter.Deserialize(fs);
                    
                    byte bytClrForeHighlight_A = (byte)formatter.Deserialize(fs);
                    byte bytClrForeHighlight_R = (byte)formatter.Deserialize(fs);
                    byte bytClrForeHighlight_G = (byte)formatter.Deserialize(fs);
                    byte bytClrForeHighlight_B = (byte)formatter.Deserialize(fs);

                    rtxCK.clrHighlighter[intHighlighter_ColorCounter].clrBack 
                        = grbNotes.rtxNotes.clrHighlighter[intHighlighter_ColorCounter].clrBack 
                        = Color.FromArgb(bytClrBackHighlight_A, bytClrBackHighlight_R, bytClrBackHighlight_G, bytClrBackHighlight_B);

                    rtxCK.clrHighlighter[intHighlighter_ColorCounter].clrFore 
                        = grbNotes.rtxNotes.clrHighlighter[intHighlighter_ColorCounter].clrFore 
                        = Color.FromArgb(bytClrForeHighlight_A, bytClrForeHighlight_R, bytClrForeHighlight_G, bytClrForeHighlight_B);

                    rtxCK.clrHighlighter[intHighlighter_ColorCounter].Text
                        = grbNotes.rtxNotes.clrHighlighter[intHighlighter_ColorCounter].Text
                        = (string)formatter.Deserialize(fs);
                    
                    rtxCK.clrHighlighter[intHighlighter_ColorCounter].valid
                        = grbNotes.rtxNotes.clrHighlighter[intHighlighter_ColorCounter].valid
                        = (bool)formatter.Deserialize(fs);
                }

         
                try
                {
                    int intHeadingList_DefaultDictionary = (int)formatter.Deserialize(fs);
                    groupboxHeadingList.DefaultDictionaryIndex = intHeadingList_DefaultDictionary;
                }
                catch (Exception)
                {
                    groupboxHeadingList.DefaultDictionaryIndex = 0;
                }
                
                fs.Close();
            }
            else
            {
                PathAndFilename = System.IO.Directory.GetCurrentDirectory() + "\\default.rtf";
            }
        }

        static bool bolAutoSave = false;
        static public bool AutoSave
        {
            get { return bolAutoSave; }
            set { bolAutoSave = value; }
        }

        void tmrBackup_Tick(object sender, EventArgs e)
        {
            int intIndex = PathAndFilename.LastIndexOf("\\");
            if (intIndex > 0)
            {
                if (AutoSave)
                {
                    mnuFile_Save_Click(sender, e);
                    Message("auto save : " + groupboxNotes.NotesFilename(cProject.Heading_Current));
                }

                string strBackupFileNameAndDirectory = PathAndFilename.Substring(0, intIndex + 1) + strBackupFileName;
                rtxCK.SaveFile(strBackupFileNameAndDirectory);
            }
        }

        void formWords_Move(object sender, EventArgs e)
        {
            recordForm();
        }

        void formWords_Activated(object sender, EventArgs e)
        {
            if (bolInit)
                return;
            Size = sz;
            Location = loc;

            formWords_SizeChanged((object)this, new EventArgs());
            if (classDictionary.lstDictionaries != null && classDictionary.lstDictionaries.Count > 0)
                if (grbHeadingList != null)
                    grbHeadingList.cDictionary = classDictionary.lstDictionaries[groupboxHeadingList.DefaultDictionaryIndex];

            splMain.SplitterDistance = intsplMainSplitterDistance;
            splSub.SplitterDistance = intsplSubSplitterDistance;
            grbHeadingList.splMain_Distance = intSplHeadingListSplitterDistance;

            rtxCK.lblColor_Draw();
            grbNotes.rtxNotes.lblColor_Draw();

            bolInit = true;
        }

        void formWords_SizeChanged(object sender, EventArgs e)
        {
            placeObjects();
        }

        public void placeObjects()
        {
            rtxCK.Left = 0;
            rtxCK.Top = 25;
            rtxCK.Height = splSub.Panel1.Height-20;
            rtxCK.Width = splSub.Panel1.Width;
            if (grbNotes != null && grbNotes.MainScreen)
            {
                grbNotes.rtxNotes.Dock = DockStyle.None;
                grbNotes.rtxNotes.Location = rtxCK.Location;
                grbNotes.rtxNotes.Size = rtxCK.Size;
            }
            grbHeadingList.Size = new Size(Width, 250);
            grbHeadingList.Location = new Point(0,
                                                rtxCK.Height - grbHeadingList.Height);
            grbHeadingList.BringToFront();
            recordForm();
        }

        void recordForm()
        {
            if (!bolInit)
                return;
            if (WindowState != FormWindowState.Minimized)
            {
                sz = Size;
                loc = Location;
            }
        }

        void mnuFile_Save_Click(object sender, EventArgs e)
        {
            string strFilename = groupboxNotes.NotesFilename(cProject.Heading_Current);

            if (System.IO.File.Exists(strFilename))
                rtxCK.SaveFile(strFilename);
        }

        public void mnuProject_Load_Click(object sender, EventArgs e)
        {
            if (!TextChangedAndNotSaved()) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Project Files | *." + classProject.ProjectFileExtension;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                PathAndFilename = ofd.FileName;
                cProject.Load();
                grbNotes.rebuildLst();

                setWorkingDirectoryFromFileName(PathAndFilename);
            }
        }

        private void setWorkingDirectoryFromFileName(string FileName)
        {
            int intIndexLastSlash = FileName.LastIndexOf('\\');
            if (intIndexLastSlash > 0)
            {
                string strTemp = FileName.Substring(0, intIndexLastSlash).ToUpper();
                if (grbNotes != null)
                {
                    if (strTemp != grbNotes.WorkingDirectory)
                    {
                        grbNotes.WorkingDirectory = strTemp;
                    }
                }
            }
        }
        public void mnuProject_Save_Click(object sender, EventArgs e)
        {
            cProject.Save();
            bolTextChangedAndNotSaved = false;
        }

        public void mnuProject_SaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Project Files | *."  +  classProject.ProjectFileExtension;
            sfd.DefaultExt = classProject.ProjectFileExtension;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                PathAndFilename = sfd.FileName;
                Text = sfd.FileName;
                cProject.Save();
                bolTextChangedAndNotSaved = false;
            }
        }

     
        private void Rtx_TextChanged(object sender, EventArgs e)
        {
            bolTextChangedAndNotSaved = true;
        }

        bool _bolTextChangedAndNotSaved = false;
        public bool bolTextChangedAndNotSaved
        {
            get { return _bolTextChangedAndNotSaved; }
            set 
            {
                _bolTextChangedAndNotSaved = value; 
            }
        }

        bool TextChangedAndNotSaved() { return TextChangedAndNotSaved(MessageBoxButtons.YesNoCancel); }

        bool TextChangedAndNotSaved(MessageBoxButtons msgBtns)
        {
            if (bolTextChangedAndNotSaved)
            {
                DialogResult dr = MessageBox.Show("Your work is not saved.\r\nDo you want to save your work?", "Work not saved", msgBtns);
                switch (dr)
                {
                    case DialogResult.Yes:
                        mnuProject_Save_Click(rtxCK, new EventArgs());
                        return true;

                    case DialogResult.No:
                        return true;

                    case DialogResult.Cancel:
                    default:
                        return false;
                }
            }
            return true;
        }

        public void mnuProject_Exit_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        public void mnuProject_New_Click(object sender, EventArgs e)
        {
            if (! TextChangedAndNotSaved()) return;
            
            rtxCK.Text = "";
            rtxCK.SelectionRightIndent = 50;
            rtxCK.RightMargin = 1100;
            grbNotes.Init();
            PathAndFilename = System.IO.Directory.GetCurrentDirectory() + "\\default FileName.rtf";
            Text = PathAndFilename;
        }
    }


    public class classRTXInfo
    {
        public string text;
        public Font fnt;
        public Color clr;
        public int start;
        public int stop;

        public classRTXInfo(string _text, Font _fnt, Color _clr, int _start, int _stop)
        {
            text = _text;
            fnt = _fnt;
            clr = _clr;
            start = _start;
            stop = _stop;
        }
    }
    public class classSearchResult
    {
        public string strFileName;
        public string strHeading;
        public classDictionary cDictionary;
        public classFileContent cFileContent;
        public classSearchResult(ref classDictionary dictionary, ref class_LL_Record cLL)
        {
            strFileName = cLL.FileName;
            strHeading = cLL.strHeading;
            cDictionary = dictionary;
        }
    }

    public class classSearchParameters
    {
        public classDictionary cDictionary = null;
        public List<string> lstWords = new List<string>();
        public enuSearchType eSearchType = enuSearchType._num;

        public classSearchParameters(ref classDictionary _cDictionary, ref List<string> _lstWords, enuSearchType _eSearchType)
        {
            cDictionary = _cDictionary;
            lstWords = _lstWords;
            eSearchType = _eSearchType;
        }
    }

}
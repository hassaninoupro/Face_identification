using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace Words
{
        enum enuFontDetails { FamilyName, Height, Bold, Underline, Italic, StrikeOut, _num };
    
    public partial class groupboxNotes : GroupBox
    {
        string strWorkingDirectory = "";
        const char conOpenBrace = '►';
        const char conCloseBrace = '◄';
        System.Windows.Forms.Timer tmrAutoSave = new Timer();

        public SplitContainer spcMain = new SplitContainer();
        public ck_RichTextBox rtxNotes = new ck_RichTextBox();
        public System.Windows.Forms.ListBox lbxNotes;
        public System.Windows.Forms.TextBox txtNotes_New;
        public System.Windows.Forms.GroupBox grbNotes_New;

        public Ck_Objects.classLabelButton btnMainScreen_Toggle = new Ck_Objects.classLabelButton();

        bool bolIgnoreListChange = false;
        public bool bolDie = false;
        bool bolQuickView = false;

        static int IDCounter = 0;
        int ID = IDCounter++;
        ContextMenu cmuNotes = new ContextMenu();

        public groupboxNotes()
        {
            InitializeComponent();

            Controls.Add(spcMain);
            spcMain.Dock = DockStyle.Fill;
            spcMain.Orientation = Orientation.Horizontal;
            spcMain.Panel1.Controls.Add(lbxNotes);
            
            spcMain.Panel2.Controls.Add(rtxNotes);
            rtxNotes.Dock = DockStyle.Fill;
            rtxNotes.rtx.Font = new Font("Arial", 10);
            rtxNotes.rtx.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
            rtxNotes.rtx.WordWrap = false;

            spcMain.SplitterMoved += SpcMain_SplitterMoved1;
            formWords.cProject.Font_Load();
            cmuNotes_Build();
            grbNotes_New.Hide();

            if (formWords.cProject.Load())
                rebuildLst();

            lbxNotes.Name = "lbxNotes";
            rtxNotes.Name = "rtxNotes";
            rtxNotes.ToolBar.lstButtons[(int)ck_RichTextBox.panelToolBar.enuButtons.File_Load].Visible = false;
            rtxNotes.ToolBar.lstButtons[(int)ck_RichTextBox.panelToolBar.enuButtons.File_SaveAs].Visible = false;
            rtxNotes.ToolBar.lstButtons[(int)ck_RichTextBox.panelToolBar.enuButtons.File_New].Visible = false;

            txtNotes_New.TextChanged += TxtNotes_New_TextChanged;
            txtNotes_New.KeyDown += txtNotes_New_KeyDown;
            txtNotes_New.LostFocus += txtNotes_New_LostFocus;

            grbNotes_New.VisibleChanged += GrbNotes_New_VisibleChanged;

            rtxNotes.rtx.TextChanged += rtxNotes_TextChanged;
            rtxNotes.rtx.KeyDown += formWords.instance.rtxCK_KeyDown;

            rtxNotes.rtx.GotFocus += RtxNotes_GotFocus;
            rtxNotes.rtx.LostFocus += RtxNotes_LostFocus;
            rtxNotes.SizeChanged += RtxNotes_SizeChanged;

            
            lbxNotes.KeyDown += lbxNotes_KeyDown;
            lbxNotes.MouseWheel += lbxNotes_MouseWheel;
            lbxNotes.Click += LbxNotes_Click;
            lbxNotes.MouseMove += LbxNotes_MouseMove;
            lbxNotes.SelectedIndexChanged += lbxNotes_SelectedIndexChanged;

            Controls.Add(btnMainScreen_Toggle);
            btnMainScreen_Toggle.Text = "";
            btnMainScreen_Toggle.Size = new Size(19, 19);
            btnMainScreen_Toggle.img_Idle = new Bitmap(Properties.Resources.btnNotes_MainScreen, 
                                                       new Size (btnMainScreen_Toggle.Size.Width - 2,
                                                                 btnMainScreen_Toggle.Size.Height - 2));
            btnMainScreen_Toggle.img_Highlight = new Bitmap(Properties.Resources.btnNotes_MainScreen_Highlight,
                                                            new Size(btnMainScreen_Toggle.Size.Width - 2,
                                                                     btnMainScreen_Toggle.Size.Height - 2));
            btnMainScreen_Toggle.Toggled = true;
            btnMainScreen_Toggle.Click += BtnMainScreen_Toggle_Click;
            btnMainScreen_Toggle.Image = btnMainScreen_Toggle.img_Idle;
            btnMainScreen_Toggle.Refresh();

            tmrAutoSave.Interval = 120000; // every two minutes
            tmrAutoSave.Tick += TmrAutoSave_Tick;

            SizeChanged += groupboxNotes_SizeChanged;
        }

        
        public static classProject cProject
        {
            get { return formWords.cProject; }
        }


        public void Init()
        {
            formWords.cProject.Init();
            rtxNotes.Text = "";
            lbxNotes.Items.Clear();
            lbxNotes.Items.Add(NewNote);
            NotesNeedToBeSaved = false;
        }


        bool bolMainScreen = false;
        public bool MainScreen
        {
            get { return bolMainScreen; }
            set
            {
                if (bolMainScreen != value)
                {
                    bolMainScreen = value;
                    if (MainScreen)
                    {
                        rtxNotes.Hide();
                        {
                            Controls.Remove(rtxNotes);
                            formWords.instance.splMain.Panel2.Controls.Add(rtxNotes);
                            rtxNotes.BringToFront();
                            formWords.instance.placeObjects();
                        }
                        rtxNotes.Show();
                    }
                    else
                    {
                        formWords.instance.splMain.Panel2.Controls.Remove(rtxNotes);
                        spcMain.Panel2.Controls.Add(rtxNotes);
                        rtxNotes.Dock = DockStyle.Fill;
                    }
                }
            }
        }
         
        private void RtxNotes_SizeChanged(object sender, EventArgs e)
        {
            // position Bottom-Left corner
        }

        private void BtnMainScreen_Toggle_Click(object sender, EventArgs e)
        {
            MainScreen = !MainScreen;
        }

        private void TxtNotes_New_TextChanged(object sender, EventArgs e)
        {
            string strMSOSReservedCharacters = "<>:\"/\\|?*";
            char[] chrReserved = strMSOSReservedCharacters.ToArray<char>();
            bool bolReservedCharFound = txtNotes_New.Text.IndexOfAny(chrReserved) >=0;
            if (bolReservedCharFound)
            {
                string strText = txtNotes_New.Text;
                foreach (char chr in strMSOSReservedCharacters)
                    strText = strText.Replace(chr, '_');
                int intIndex = txtNotes_New.SelectionStart;
                txtNotes_New.Text = strText;
                txtNotes_New.SelectionStart = intIndex;
            }
        }

        void txtNotes_New_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        int intIndex = formWords.cProject.lstNotesInfo.IndexOf(formWords.cProject.cNote_Edit); 

                        string strHeading_Old = formWords.cProject.lstNotesInfo[intIndex].Heading;

                        formWords.cProject.lstNotesInfo[intIndex].Heading = txtNotes_New.Text;
                        lbxNotes.Items[intIndex] = txtNotes_New.Text;

                        string strHeading_New = formWords.cProject.lstNotesInfo[intIndex].Heading;

                        string strFilename_New = NotesFilename(strHeading_New);
                        rtxNotes.rtx.SaveFile(strFilename_New);
                        rtxNotes.rtx.Font = new Font("Arial", 10);

                        string strFilename_Old = NotesFilename(strHeading_Old);
                        if (System.IO.File.Exists(strFilename_Old))
                            System.IO.File.Delete(strFilename_Old);

                        e.SuppressKeyPress = true;
                        grbNotes_New.Hide();
                    }
                    break;

                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    break;

                default:
                    {
                    }
                    break;
            }
        }

        private void SpcMain_SplitterMoved1(object sender, SplitterEventArgs e)
        {   
            intSpc_distance = spcMain.SplitterDistance;
            placeObjects();
        }


        static int _intSpc_distance = 90;
        public static int intSpc_distance
        {
            get { return _intSpc_distance; }
            set { _intSpc_distance = value; }
        }
        void RtxNotes_GotFocus(object sender, EventArgs e)
        {
            if (formWords.instance.frmFindReplace == null || formWords.instance.frmFindReplace.IsDisposed)
                formWords.instance.frmFindReplace = new formFindReplace();
            formWords.instance.frmFindReplace.setTextBox(ref sender);

            try
            {
                tmrAutoSave.Enabled = true;
            }
            catch (Exception)
            {

            }

            
        }

        private void RtxNotes_LostFocus(object sender, EventArgs e)
        {
            if (NotesNeedToBeSaved)
            SaveNote();
            tmrAutoSave.Enabled = false;
            //string strFilename_Save = NotesFilename(lstNotesInfo[formWords.cProject.Index_Current].Heading);
            //rtxNotes.rtx.SaveFile(strFilename_Save);
        }

        bool bolFirstLoad = true;
        int Index_Current
        {
            get { return formWords.cProject.Index_Current; }
            set
            {
                //if (formWords.cProject.Index_Current != value)
                {
                    if (value >= 0 && value < formWords.cProject.lstNotesInfo.Count)
                    {
                        if (!bolFirstLoad && NotesNeedToBeSaved)
                        {
                            if (formWords.cProject.Index_Current < formWords.cProject.lstNotesInfo.Count)
                                SaveNote();
                        }
                        bolFirstLoad = false;
                        formWords.cProject.Index_Current
                            = lbxNotes_SelectedIndex
                            = value;

                        string strFilename_Load = NotesFilename(formWords.cProject.lstNotesInfo[formWords.cProject.Index_Current].Heading);
                        if (System.IO.File.Exists(strFilename_Load))
                        {
                            rtxNotes.PathAndFilename = strFilename_Load;
                            rtxNotes.LoadFile(rtxNotes.PathAndFilename);//.rtx.LoadFile(rtxNotes.PathAndFilename);
                            rtxNotes.rtx.Font = new Font("Arial", 10);

                            if (formWords.cProject.lstNotesInfo[formWords.cProject.Index_Current].Caret >= 0 && formWords.cProject.lstNotesInfo[formWords.cProject.Index_Current].Caret < rtxNotes.rtx.Text.Length)
                            {
                                rtxNotes.rtx.SelectionStart = formWords.cProject.lstNotesInfo[formWords.cProject.Index_Current].Caret;
                                rtxNotes.rtx.ScrollToCaret();
                            }
                            NotesNeedToBeSaved = false;
                            rtxNotes_SetEnable();

                        }
                    }
                }
            }
        }

        public void rtxNotes_SetEnable()
        {
            rtxNotes.Enabled = string.Compare(cProject.Heading_Current, cProject.lstNotesInfo[formWords.cProject.Index_Current].Heading) != 0;

            formWords.instance.Title_Draw();
        }


        void SaveNote()
        {
            string strFilename_Save = NotesFilename(formWords.cProject.lstNotesInfo[formWords.cProject.Index_Current].Heading);
            formWords.cProject.lstNotesInfo[formWords.cProject.Index_Current].Caret = rtxNotes.rtx.GetCharIndexFromPosition(new Point(0, 0));

            try
            {
                rtxNotes.rtx.SaveFile(strFilename_Save);
                formWords.Message("Autosave note : " + formWords.cProject.lstNotesInfo[formWords.cProject.Index_Current].Heading);
            }
            catch (Exception)
            {

            }
            NotesNeedToBeSaved = false;
        }

        private void TmrAutoSave_Tick(object sender, EventArgs e)
        {
            if (NotesNeedToBeSaved)
            SaveNote();
        }

        void tmrAutoSave_Reset()
        {
            tmrAutoSave.Enabled = false;
            tmrAutoSave.Enabled = true;
        }


        public static string NotesFilename(string strHeading)
        {
            return formWords.instance.Path  + formWords.instance.ProjectFileName + "_Note_" + strHeading + ".rtf";
        }


        void groupboxNotes_Disposed(object sender, EventArgs e)
        {
            if (!bolDie)
             formWords.cProject.Save();
        }

        
        private void LbxNotes_Click(object sender, EventArgs e)
        {
            if (bolQuickView)
                mnuNotes_QuickView_Click((object)mnuNotes_QuickCheck, new EventArgs());
        }

        Point ptLbx_Mouse = new Point();

        private void LbxNotes_MouseMove(object sender, MouseEventArgs e)
        {
            ptLbx_Mouse.X = e.X;
            ptLbx_Mouse.Y = e.Y;
            if (bolQuickView)
            {
                int intSelected = lbxNotes.IndexFromPoint(ptLbx_Mouse);
                if (intSelected >= 0 && intSelected < lbxNotes.Items.Count)
                    lbxNotes_SelectedIndex = intSelected;
            }
        }

        void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(groupboxNotes));
            this.SuspendLayout();
            // 
            // groupboxNotes
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            Bitmap bmp = new Bitmap(Properties.Resources.Notes);
            //Icon = Icon.FromHandle(bmp.GetHicon());
            this.Name = "groupboxNotes";
            this.ResumeLayout(false);

            this.txtNotes_New = new System.Windows.Forms.TextBox();
            this.lbxNotes = new ListBox();
            this.SuspendLayout();
            // 
            // rtxNotes
            // 
            this.rtxNotes.Location = new System.Drawing.Point(-2, 212);
            this.rtxNotes.Name = "rtxNotes";
            this.rtxNotes.Size = new System.Drawing.Size(338, 295);
            this.rtxNotes.TabIndex = 0;
            // 
            // txtNotes_New
            // 
            this.txtNotes_New.Location = new System.Drawing.Point(10, 10);
            this.txtNotes_New.Name = "txtNotes_New";
            this.txtNotes_New.Size = new System.Drawing.Size(224, 20);
            this.txtNotes_New.TabIndex = 2;

            // 
            // lbxNotes
            // 
            this.lbxNotes.FormattingEnabled = true;
            this.lbxNotes.Location = new System.Drawing.Point(0, 59);
            this.lbxNotes.Name = "lbxNotes";
            this.lbxNotes.Size = new System.Drawing.Size(336, 147);
            this.lbxNotes.TabIndex = 3;
            // 
            // groupboxNotes
            // 

            grbNotes_New = new GroupBox();
            formWords.instance.Controls.Add(grbNotes_New);
            grbNotes_New.Controls.Add(this.txtNotes_New);
            grbNotes_New.Text = "Edit Notes Heading";
            grbNotes_New.Size = new Size(225, 45);
            txtNotes_New.Dock = DockStyle.Fill;

            this.ClientSize = new System.Drawing.Size(337, 505);
            this.Controls.Add(this.lbxNotes);
            this.Controls.Add(this.rtxNotes);
            this.Name = "groupboxNotes";
            this.Text = "Notes";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void txtNotes_New_LostFocus(object sender, EventArgs e)
        {
            //txtNotes_New.Hide();
        }
        private void GrbNotes_New_VisibleChanged(object sender, EventArgs e)
        {
            grbNotes_New.Width = lbxNotes.Width;
            grbNotes_New.BackColor = Color.Blue;
            grbNotes_New.ForeColor = Color.White;
            grbNotes_New.Location = new Point(lbxNotes.Left + (lbxNotes.Width - grbNotes_New.Width) / 2,
                                              lbxNotes.Top + (lbxNotes.Height - grbNotes_New.Height) / 2);
            grbNotes_New.BringToFront();
            txtNotes_New.Focus();

            lbxNotes.Enabled = !grbNotes_New.Visible;
        }

        
        MenuItem mnuNotes_QuickCheck = null;
        //MenuItem mnuNotes_Font = null;
        void cmuNotes_Build()
        {
            MenuItem mnuNotes_Headings = new MenuItem("Headings");
            {
                MenuItem mnuEdit = new MenuItem("Edit", mnuNotes_Edit_Click);
                mnuNotes_Headings.MenuItems.Add(mnuEdit);

                MenuItem mnuDelete = new MenuItem("Delete", mnuNotes_Delete_Click);
                mnuNotes_Headings.MenuItems.Add(mnuDelete);

                MenuItem mnuAdd = new MenuItem("Add");
                {
                    MenuItem mnuHeading_Add_New = new MenuItem("New", mnuNotes_Add_New_Click);
                    mnuAdd.MenuItems.Add(mnuHeading_Add_New);
                    MenuItem mnuHeading_Add_File = new MenuItem("File", mnuNotes_Add_File_Click);
                    mnuAdd.MenuItems.Add(mnuHeading_Add_File);
                }
                mnuNotes_Headings.MenuItems.Add(mnuAdd);
            }
            cmuNotes.MenuItems.Add(mnuNotes_Headings);

            MenuItem mnuNotes_EditSelection = new MenuItem("Edit Selection", mnuNotes_EditSelection_Click);
            cmuNotes.MenuItems.Add(mnuNotes_EditSelection);

            cmuNotes.MenuItems.Add(new MenuItem("Alphabetize", mnuNotes_Alphabetize_Click));

            lbxNotes.ContextMenu = cmuNotes;
        }




        void mnuHide_Click(object sender, EventArgs e)
        {
            Hide();
        }

        void mnuNote_Font_click(object sender, EventArgs e)
        {
            FontDialog fntDialog = new FontDialog();
            if (fntDialog.ShowDialog() == DialogResult.OK)
            {
               formWords.cProject.fnt = fntDialog.Font;
            }
        }


        void mnuNotes_TopMost_Click(object sender, EventArgs e)
        {
            //TopMost = !TopMost;
            //MenuItem mnuSender = (MenuItem)sender;
            //mnuSender.Checked = TopMost;
        }

        void mnuNotes_QuickView_Click(object sender, EventArgs e)
        {
            bolQuickView = !bolQuickView;
            MenuItem mnuSender = (MenuItem)sender;
            mnuSender.Checked = bolQuickView;
        }




        void mnuNotes_Edit_Click(object sender, EventArgs e)
        {

            if (lbxNotes_SelectedIndex >= 0 && lbxNotes_SelectedIndex < lbxNotes.Items.Count)
            {
                formWords.cProject.cNote_Edit =formWords.cProject.lstNotesInfo[lbxNotes_SelectedIndex];
                
                txtNotes_New.Text = formWords.cProject.cNote_Edit.Heading;
                grbNotes_New.Show();
                
                txtNotes_New.Focus();
            }
        }

        void mnuNotes_Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("are you sure you want to delete \"" +formWords.cProject.lstNotesInfo[lbxNotes_SelectedIndex].Heading + "\"?", "delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
               formWords.cProject.lstNotesInfo.RemoveAt(lbxNotes_SelectedIndex);
                bolFirstLoad = true;
                rebuildLst();
            }
        }

        string NewNote = "New Note";
        void mnuNotes_Add_New_Click(object sender, EventArgs e)
        {
            bolIgnoreListChange = true;

            classNotesInfo cNoteRef 
                = formWords.cProject.cNote_Edit
                = new classNotesInfo(NewNote);

           formWords.cProject.lstNotesInfo.Insert(lbxNotes_SelectedIndex, cNoteRef);

            rebuildLst();

            rtxNotes.Text = "";
            rtxNotes.rtx.SelectionFont = new Font("Arial", 10);
            rtxNotes.Focus();
            NotesNeedToBeSaved = false;
            lbxNotes.Focus();
            lbxNotes_SelectedIndex =formWords.cProject.lstNotesInfo.IndexOf(cNoteRef);

            bolIgnoreListChange = false;

            txtNotes_New.Tag = (object)(lbxNotes.Items.Count - 1);
            txtNotes_New.Text = cNoteRef.Heading;
            txtNotes_New.SelectAll();
            grbNotes_New.Show();
            txtNotes_New.BringToFront();
            txtNotes_New.Focus();
        }

        static string FilenameGet(string strPath)
        {

            string strTempHeading = "";
            int intLastBackSlash = strPath.LastIndexOf("\\");
            if (intLastBackSlash > 0)
                strTempHeading = strPath.Substring(intLastBackSlash + 1);
            else
                return strPath;

            int intLastDecimal = strTempHeading.LastIndexOf(".");
            if (intLastDecimal >=0)
            strTempHeading = strTempHeading.Substring(0, intLastDecimal);

            return strTempHeading;
        }

        void mnuNotes_Add_File_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Notes - add File";
            ofd.DefaultExt = "rtf";
            ofd.Multiselect = true;
            ofd.Filter = "RichTextFile|*.rtf";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                for (int intFileCounter = 0; intFileCounter < ofd.FileNames.Length; intFileCounter++)
                {
                    string strOFD_Filename = ofd.FileNames[intFileCounter];
                    if (System.IO.File.Exists(strOFD_Filename))
                    {
                        string strTempFilenamePath = "";
                        string strHeading = FilenameGet(strOFD_Filename);
                     
                        strTempFilenamePath = strOFD_Filename;// NotesFilename( FilenameGet(strOFD_Filename));

                        string strFilenamePath = NotesFilename(strHeading);
                        if (string.Compare(strTempFilenamePath.ToLower(), strFilenamePath.ToLower()) != 0)
                        {
                            if (System.IO.File.Exists(strFilenamePath))
                            {
                                if (MessageBox.Show("the file \r\n" + strFilenamePath + "\r\nalready exists.\r\nOverwrite?", "Overwrite File?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                                    goto copyFileEnd;
                                System.IO.File.Delete(strFilenamePath);
                            }
                            System.IO.File.Copy(strTempFilenamePath, strFilenamePath);
                        }
                    copyFileEnd:

                        bolIgnoreListChange = true;

                        classNotesInfo cNote = new classNotesInfo(strHeading);

                        formWords.cProject.lstNotesInfo.Insert(lbxNotes_SelectedIndex, cNote);

                        rebuildLst();

                        rtxNotes.rtx.LoadFile(strFilenamePath);

                        lbxNotes.Focus();
                        lbxNotes_SelectedIndex = formWords.cProject.lstNotesInfo.IndexOf(cNote);
                        bolIgnoreListChange = false;
                    }
                }
            }
            
        }


        string _strFileNameAndDirectory = "";
        public string strFilePath
        {
            get
            {
                if (_strFileNameAndDirectory == "")
                    _strFileNameAndDirectory = formWords.instance.PathAndFilename.ToUpper().Replace(".RTF", ".XML");
                return _strFileNameAndDirectory;
            }
            set
            {
                _strFileNameAndDirectory = value;
            }
        }


        bool bolNotes_NeedToBeSaved = false;
        public bool NotesNeedToBeSaved
        {
            get { return bolNotes_NeedToBeSaved; }
            set { bolNotes_NeedToBeSaved = value; }
        }

        void rtxNotes_TextChanged(object sender, EventArgs e)
        {
            NotesNeedToBeSaved = true;
            if (rtxNotes.SelectionStart > 0)
            {
                if (rtxNotes.Text[rtxNotes.SelectionStart - 1] == '#')
                {
                    rtxNotes.Hide();
                    //rtxNotes.Show();
                    //rtxNotes.Refresh();
                    int intPreviousBraceValue = FindPreviousBraceValue(rtxNotes.SelectionStart);
                    int intRememberSelectionStart = rtxNotes.SelectionStart;
                    if (cursorIsInsideBrackets())
                    {
                        setThisBrace(intPreviousBraceValue + 1);
                        rtxNotes.SelectionStart = intRememberSelectionStart + (intPreviousBraceValue + 1).ToString().Length;
                    }
                    else
                    {
                        insertNewBrace(intPreviousBraceValue + 1);
                        rtxNotes.SelectionStart = intRememberSelectionStart + (intPreviousBraceValue + 1).ToString().Length + 1;
                    }
                    rtxNotes.Show();
                    rtxNotes.rtx.Focus();
                    //rtxNotes.rtx.ScrollToCaret();
                }
            }

        }

        void setThisBrace(int intNewValue)
        {
            string strText = rtxNotes.Text;
            int intIndexNextClose = rtxNotes.Text.IndexOf(conCloseBrace, rtxNotes.SelectionStart);
            int intIndexPreviousOpen = IndexOfBefore(ref strText, rtxNotes.SelectionStart, conOpenBrace);
       
            rtxNotes.rtx.SelectionStart = intIndexPreviousOpen + 1;
            int intLength = intIndexNextClose - intIndexPreviousOpen - 1;
            if (intLength >0)
            {
                rtxNotes.rtx.SelectionLength = intLength;
                rtxNotes.rtx.SelectedText = intNewValue.ToString();

                rtxNotes.rtx.SelectionStart = intIndexNextClose + 1;
                rtxNotes.rtx.SelectionLength = 0;
                setNextBrace(intNewValue + 1);
            }
            
        }

        void insertNewBrace(int intBraceValue)
        {
            int intRememberSelectionStart = rtxNotes.SelectionStart;
            
            rtxNotes.rtx.SelectionStart = ((intRememberSelectionStart > 0 && rtxNotes.rtx.Text[intRememberSelectionStart - 1] == '#') ? intRememberSelectionStart -1 : intRememberSelectionStart);
            rtxNotes.rtx.SelectionLength = 1;// intRememberSelectionStart;

            string strNew = conOpenBrace + intBraceValue.ToString() + conCloseBrace;
            rtxNotes.rtx.SelectedText = strNew;

            rtxNotes.rtx.SelectionLength = 0;
            setNextBrace(intBraceValue + 1);

            rtxNotes.SelectionStart = intRememberSelectionStart + intBraceValue.ToString().Length + 1;
        }

        void setNextBrace(int intNextValue)
        {
            int intIndexNextOpen = rtxNotes.Text.IndexOf(conOpenBrace, rtxNotes.SelectionStart);
            while (intIndexNextOpen == rtxNotes.SelectionStart && rtxNotes.SelectionStart < rtxNotes.Text.Length - 1)
                rtxNotes.SelectionStart += 1;
                          
            int intIndexNextClose = rtxNotes.Text.IndexOf(conCloseBrace, rtxNotes.SelectionStart);
            while (intIndexNextClose == rtxNotes.SelectionStart && rtxNotes.SelectionStart < rtxNotes.Text.Length - 1)
            {
                rtxNotes.SelectionStart += 1;
                intIndexNextClose = rtxNotes.Text.IndexOf(conCloseBrace, rtxNotes.SelectionStart);
            }

            if (intIndexNextClose >= 0 && intIndexNextOpen >= 0 && intIndexNextClose > intIndexNextOpen)
            {
                rtxNotes.SelectionStart = intIndexNextOpen + 1;
                setThisBrace(intNextValue);
            }
        
        }

        int FindPreviousBraceValue(int intIndex)
        {
            string strText = rtxNotes.Text;
            int intIndexPreviousClose = IndexOfBefore(ref strText, intIndex, conCloseBrace);
            if (intIndexPreviousClose > 0)
            {
                int intIndexPreviousOpen = IndexOfBefore(ref strText, intIndexPreviousClose, conOpenBrace);
                if (intIndexPreviousOpen >= 0)
                {
                    string strBraceValue = strText.Substring(intIndexPreviousOpen + 1, intIndexPreviousClose - intIndexPreviousOpen - 1);
                    try
                    {
                        int intBraceValue = Convert.ToInt16(strBraceValue);
                        return intBraceValue;
                    }
                    catch (Exception)
                    {
                        return -1;
                    }
                }
                else
                    return -1;
            }
            else
                return -1;
        }


        int IndexOfBefore(ref string strText, int intStart, char chaSearch)
        {
            int intIndexOf = strText.IndexOf(chaSearch);
            if (intIndexOf >= 0 && intIndexOf <= intStart)
            {
                int intNextIndexOf = strText.IndexOf(chaSearch, intIndexOf + 1);
                while (intNextIndexOf < intStart && intNextIndexOf > 0)
                {
                    intIndexOf = intNextIndexOf;
                    intNextIndexOf = strText.IndexOf(chaSearch, intIndexOf + 1);
                }
                return intIndexOf;
            }
            else
                return -1;
        }

        #region "numbering"

        bool cursorIsInsideBrackets()
        {
            string strText = rtxNotes.Text;
            int intIndexPreviousOpen = IndexOfBefore(ref strText, rtxNotes.SelectionStart, conOpenBrace);
            int intIndexPreviousClose = IndexOfBefore(ref strText, rtxNotes.SelectionStart, conCloseBrace);
            int intIndexNextOpen = strText.IndexOf(conOpenBrace, rtxNotes.SelectionStart);
            int intIndexNextClose = strText.IndexOf(conCloseBrace, rtxNotes.SelectionStart);

            return (intIndexPreviousOpen >= 0 && intIndexNextClose >= 0
                    && intIndexPreviousOpen > intIndexPreviousClose
                    && intIndexNextOpen > intIndexNextClose);

        }

        #endregion

        void lbxNotes_MouseWheel(object sender, MouseEventArgs e)
        {
            if (bolQuickView) return;

            //if (e.Delta < 0)
            //    moveListDown();
            //else if (e.Delta > 0)
            //    moveListUp();
        }

        void lbxNotes_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    moveListUp();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Down:
                    moveListDown();
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        void mnuNotes_EditSelection_Click(object sender, EventArgs e)
        {
            if (lbxNotes_SelectedIndex >= 0 && lbxNotes_SelectedIndex < cProject.lstNotesInfo.Count)
                cProject.Heading_Current = cProject.lstNotesInfo[lbxNotes_SelectedIndex].Heading;
            rtxNotes_SetEnable();

        }


        void mnuNotes_Alphabetize_Click(object sender, EventArgs e)
        {
            if (NotesNeedToBeSaved)
                SaveNote();

            IEnumerable<classNotesInfo> query =formWords.cProject.lstNotesInfo.OrderBy(notes => notes.Heading, new StringLibrary.classStringLibrary.Compare_Alphabetical());
           formWords.cProject.lstNotesInfo = query.ToList<classNotesInfo>();
            rebuildLst();
            
            bolIgnoreListChange = true;
            bolFirstLoad = true;
            Index_Current = 0;
            bolIgnoreListChange = false;
        }

        void moveListDown()
        {
            if (lbxNotes_SelectedIndex < lbxNotes.Items.Count - 1)
            {
                string strTemp =formWords.cProject.lstNotesInfo[lbxNotes_SelectedIndex].Heading;
               formWords.cProject.lstNotesInfo.RemoveAt(lbxNotes_SelectedIndex);
               formWords.cProject.lstNotesInfo.Insert( lbxNotes_SelectedIndex + 1,new classNotesInfo( strTemp));

                int intSelectedIndex = lbxNotes_SelectedIndex + 1;
                rebuildLst();
                bolIgnoreListChange = true;

                bolFirstLoad = true;
                formWords.cProject.Index_Current // bypass property
                    = lbxNotes_SelectedIndex
                    = intSelectedIndex;
                bolIgnoreListChange = false;
            }
        }

        void moveListUp()
        {
            if (lbxNotes_SelectedIndex > 0)
            {
                string strTemp =formWords.cProject.lstNotesInfo[lbxNotes_SelectedIndex].Heading;
               formWords.cProject.lstNotesInfo.RemoveAt(lbxNotes_SelectedIndex);
               formWords.cProject.lstNotesInfo.Insert(lbxNotes_SelectedIndex - 1,new classNotesInfo( strTemp));

                int intSelectedIndex = lbxNotes_SelectedIndex - 1;
                rebuildLst();
                bolIgnoreListChange = true;
                bolFirstLoad = true;
                formWords.cProject.Index_Current // bypass property
                    =  lbxNotes_SelectedIndex 
                    = intSelectedIndex;
                bolIgnoreListChange = false;
            }
        }

      

        void resize()
        {
            groupboxNotes_SizeChanged((object)this, new EventArgs());
        }

        void groupboxNotes_SizeChanged(object sender, EventArgs e)
        {
            placeObjects();
        }
        void placeObjects()
        { 

            lbxNotes.Top = 5;
            lbxNotes.Left = 5;
            lbxNotes.Width = Width - 5;
            lbxNotes.Height = spcMain.Panel1.Height - lbxNotes.Top;

            if (!MainScreen)
                rtxNotes.Dock = DockStyle.Fill;

            int intGap = 04;
            btnMainScreen_Toggle.Left = intGap;
            btnMainScreen_Toggle.Top = Height - btnMainScreen_Toggle.Height - intGap;
            btnMainScreen_Toggle.BringToFront();
        }


      
        void createNewNote(string strName)
        {
            
        }

        int intlbxNotes_SelectedIndex = -1;
        public int lbxNotes_SelectedIndex
        {
            get { return lbxNotes.SelectedIndex; }
            set
            {
                if (lbxNotes.SelectedIndex != value)
                    lbxNotes.SelectedIndex = value;
            }
        }


        void lbxNotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!bolIgnoreListChange)
            {
                Index_Current = lbxNotes_SelectedIndex;
            }
        }


        public void rebuildLst()
        {
            bolIgnoreListChange = true;
            lbxNotes.Items.Clear();
            for (int intNoteCounter = 0; intNoteCounter <formWords.cProject.lstNotesInfo.Count; intNoteCounter++)
                lbxNotes.Items.Add(formWords.cProject.lstNotesInfo[intNoteCounter].Heading);

            resize();
            
            bolIgnoreListChange = false;
            Index_Current = 0;
        }

        public string WorkingDirectory
        {
            get { return strWorkingDirectory; }
            set
            {
                strWorkingDirectory = value;
            }
        }




    }
}

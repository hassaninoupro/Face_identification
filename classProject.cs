using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;

namespace Words
{
    public class classNotesInfo
    {
        public string Heading = "";
        public int Caret = 0;

        public classNotesInfo() { }
        public classNotesInfo(string Heading)
        {
            this.Heading = Heading;
        }
    }

    public class classProject
    {
        public List<classNotesInfo> lstNotesInfo = new List<classNotesInfo>();

        static string strProjectFileExtension = "WordsProject";
        static public string ProjectFileExtension { get { return strProjectFileExtension; } }

        string strName = "";
        public string Name
        {
            get { return strName; }
            set { strName = value; }
        }

        Font _fnt = new Font("ms Sans-serif", 10);
        public Font fnt
        {
            get { return _fnt; }
            set
            {
                if (_fnt != value)
                {
                    _fnt = value;
                    if (grbNotes != null)
                    {
                        if (grbNotes.rtxNotes != null)
                            grbNotes.rtxNotes.Font = _fnt;
                        if (grbNotes.txtNotes_New != null)
                            grbNotes.txtNotes_New.Font = _fnt;
                        if (grbNotes.lbxNotes != null)
                            grbNotes.lbxNotes.Font = _fnt;
                    }
                }
            }
        }

        groupboxNotes grbNotes { get { return formWords.instance.grbNotes; } }


        string FontFileName
        {
            get { return System.IO.Directory.GetCurrentDirectory() + "\\fnt.txt"; }
        }

        public void Font_Load()
        {
            if (System.IO.File.Exists(FontFileName))
            {
                string strFont = System.IO.File.ReadAllText(FontFileName);
                char[] chrSplitSeparator = strFontDetail_Delimiter.ToArray<char>();

                string[] strFontDetails = strFont.Split(chrSplitSeparator);
                try
                {
                    string strTrue = true.ToString();
                    string strFamilyName = strFontDetails[(int)enuFontDetails.FamilyName];
                    float fltHeight = (float)Convert.ToDouble(strFontDetails[(int)enuFontDetails.Height]);
                    bool bolBold = string.Compare(strFontDetails[(int)enuFontDetails.Bold], strTrue) == 0;
                    bool bolItalic = string.Compare(strFontDetails[(int)enuFontDetails.Italic], strTrue) == 0;
                    bool bolStrikeOut = string.Compare(strFontDetails[(int)enuFontDetails.StrikeOut], strTrue) == 0;
                    bool bolUnderline = string.Compare(strFontDetails[(int)enuFontDetails.Underline], strTrue) == 0;

                    FontStyle fntStyle = (bolBold ? FontStyle.Bold : FontStyle.Regular)
                                        | (bolItalic ? FontStyle.Italic : FontStyle.Regular)
                                        | (bolUnderline ? FontStyle.Underline : FontStyle.Regular)
                                        | (bolStrikeOut ? FontStyle.Strikeout : FontStyle.Regular);
                    fnt = new Font(strFamilyName, fltHeight, fntStyle);
                }
                catch (Exception)
                {
                }
            }
        }

        string strFontDetail_Delimiter = "|";

        string strHeading_Current = "";
        public string Heading_Current
        {
            get { return strHeading_Current; }
            set
            {
                if (string.Compare(strHeading_Current, value) != 0)
                {
                    // Editor - save current Note
                    string strHeadingCurrent_Filename = groupboxNotes.NotesFilename(strHeading_Current);
                    if (System.IO.File.Exists(strHeadingCurrent_Filename))
                        formWords.instance.rtxCK.SaveFile(strHeadingCurrent_Filename);

                    strHeading_Current = value;

                    // Editor - load current Note
                    strHeadingCurrent_Filename = groupboxNotes.NotesFilename(strHeading_Current);
                    if (System.IO.File.Exists(strHeadingCurrent_Filename))
                        formWords.instance.rtxCK.LoadFile(strHeadingCurrent_Filename);
                }
            }
        }
        int intIndex_Current = -1;
        public int Index_Current
        {
            get { return intIndex_Current; }
            set { intIndex_Current = value; }
        }

        string _strFilePath = "";
        public string FilePath
        {
            get
            {
                if (_strFilePath == null || _strFilePath.Length == 0)
                    _strFilePath = System.IO.Directory.GetCurrentDirectory() + "\\test.rtf";
                return _strFilePath;
            }
            set { _strFilePath = value; }
        }

        string strNewNote = "";
        public string NewNote
        {
            get { return strNewNote; }
            set { strNewNote = value; }
        }

        classNotesInfo _cNote_Edit = null;
        public classNotesInfo cNote_Edit
        {
            get { return _cNote_Edit; }
            set { _cNote_Edit = value; }
        }

        public bool Load()
        {
            Index_Current = 0;
            Init();

            XmlDocument xDoc = new XmlDocument();
            if (System.IO.File.Exists(FilePath))
            {
                xDoc.Load(FilePath);
                XmlNode xRoot = xDoc.FirstChild;
                XmlNode xHeadingCurrent = xRoot.FirstChild;
                XmlNode xHeadingList = xHeadingCurrent.NextSibling;
                lstNotesInfo.Clear();
                for (int intNoteCounter = 0; intNoteCounter < xHeadingList.ChildNodes.Count; intNoteCounter++)
                {
                    XmlNode xNode = xHeadingList.ChildNodes[intNoteCounter];
                    classNotesInfo cNote = new classNotesInfo(xNode.InnerText);

                    try
                    {
                        cNote.Caret = Convert.ToInt32(xNode.Attributes[0].Value);
                    }
                    catch (Exception)
                    {
                    }
                    lstNotesInfo.Add(cNote);
                }
                Heading_Current = xHeadingCurrent.InnerText;
                return true;
            }
            else
            {
                return false;
            }

        }

        public void Init()
        {
            lstNotesInfo.Clear();
            lstNotesInfo.Add(new classNotesInfo(NewNote));
        }


        public void Save()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlElement xRoot = xDoc.CreateElement("Root");

            XmlElement xCurrentHeading = xDoc.CreateElement("CurrentHeading");
            xCurrentHeading.InnerText = Heading_Current;
            xRoot.AppendChild(xCurrentHeading);

            XmlElement xHeadingsList = xDoc.CreateElement("HeadingList");
            {
                for (int intNoteCounter = 0; intNoteCounter < lstNotesInfo.Count; intNoteCounter++)
                {
                    XmlElement xEle = xDoc.CreateElement("Heading");
                    xEle.InnerText = lstNotesInfo[intNoteCounter].Heading;
                    xEle.SetAttribute("Caret", lstNotesInfo[intNoteCounter].Caret.ToString());
                    xHeadingsList.AppendChild(xEle);
                }
            }
            xRoot.AppendChild(xHeadingsList);

            xDoc.AppendChild(xRoot);
            xDoc.Save(FilePath);

            Font_Save();
        }

        void Font_Save()
        {
            string strFont = "";
            for (int intCounter = 0; intCounter < (int)enuFontDetails._num; intCounter++)
            {
                enuFontDetails eFontDetail = (enuFontDetails)intCounter;
                switch (eFontDetail)
                {
                    case enuFontDetails.FamilyName:
                        {
                            strFont += fnt.FontFamily.Name;
                        }
                        break;

                    case enuFontDetails.Height:
                        {
                            strFont += fnt.Size.ToString();
                        }
                        break;

                    case enuFontDetails.Bold:
                        {
                            strFont += fnt.Bold.ToString();
                        }
                        break;

                    case enuFontDetails.Underline:
                        {
                            strFont += fnt.Underline.ToString();
                        }
                        break;

                    case enuFontDetails.Italic:
                        {
                            strFont += fnt.Italic.ToString();
                        }
                        break;

                    case enuFontDetails.StrikeOut:
                        {
                            strFont += fnt.Strikeout.ToString();
                        }
                        break;
                }
                strFont += strFontDetail_Delimiter;
            }
            System.IO.File.WriteAllText(FontFileName, strFont);
        }

    }

}

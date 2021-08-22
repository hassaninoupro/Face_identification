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
//using Ck_Objects;

namespace Words
{

    public class formPopUp : Form
    {
        public static formPopUp instance;
        public classDictionaryOutput_RichTextBox cDictionaryOutput_rtx = new classDictionaryOutput_RichTextBox();
        //Ck_Objects.classRTX cRTX = new Ck_Objects.classRTX();

        const string strFontFamily = "sans serif";
        Font fntHeading = new Font(strFontFamily, 14, FontStyle.Bold);
        Font fntAltHeading = new Font(strFontFamily, 12, FontStyle.Bold);
        Font fntDefinition = new Font(strFontFamily, 10, FontStyle.Regular);

        public formPopUp()
        {
            instance = this;
            Controls.Add(cDictionaryOutput_rtx.rtx);

            cDictionaryOutput_rtx.rtx.MouseLeave += Rtx_MouseLeave;
            Hide();
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(400, 300);

            TopMost = true;
            VisibleChanged += FormPopUp_VisibleChanged;
            Activated += FormPopUp_Activated;
        }

        private void Rtx_MouseLeave(object sender, EventArgs e)
        {
            Hide();
        }

        private void FormPopUp_VisibleChanged(object sender, EventArgs e)
        {

        }

        public static Point ptLocation = new Point();
        bool bolActivated = false;

        private void FormPopUp_Activated(object sender, EventArgs e)
        {
            if (bolActivated) return;
            bolActivated = true;
            Location = ptLocation;
        }

    }
    public class classDictionaryOutput_RichTextBox 
    {
        public RichTextBox rtx = new RichTextBox();

        public classDictionaryOutput_RichTextBox ()
        {
            rtx.Dock = DockStyle.Fill;
            rtx.BackColor = Color.White;
            rtx.MouseLeave += Rtx_MouseLeave;

            //rtx.KeyDown += RtxOutput_KeyDown;
            //rtx.KeyUp += RtxOutput_KeyUp;
            //rtx.KeyPress += Rtx_KeyPress;
            //rtx.MouseEnter += RtxOutput_MouseEnter;
            //rtx.MouseLeave += RtxOutput_MouseLeave;
            //rtx.MouseDoubleClick += Rtx_MouseDoubleClick;
            //rtx.MouseMove += RtxOutput_MouseMove;
            //rtx.MouseUp += RtxOutput_MouseUp;
            //rtx.MouseWheel += RtxOutput_MouseWheel;
            //rtx.VScroll += RtxOutput_VScroll;
        }
        public void LoadFile(string strFilename) { LoadFile(ref rtx, strFilename); }
        static public void LoadFile(ref RichTextBox rtx, string strFilename)
        {
            string[] strExtensions = { "txt", "rtf" };
            for (int intExtensionCounter = 0; intExtensionCounter < (int)enuFileExtensions._num; intExtensionCounter++)
            {
                enuFileExtensions eExtension = (enuFileExtensions)intExtensionCounter;
                string strTestFilename = strFilename + "." + eExtension.ToString();
                if (System.IO.File.Exists(strTestFilename))
                {
                    switch (eExtension)
                    {
                        case enuFileExtensions.rtf:
                            rtx.LoadFile(strTestFilename);
                            return;

                        case enuFileExtensions.txt:
                            {
                                classFileContent cFileContent = new classFileContent(strTestFilename);
                                rtx.Clear();
                                classStringLibrary.RTX_AppendText(ref rtx, cFileContent.Heading.Trim(), new Font("Arial", 14, FontStyle.Bold), Color.DarkBlue, 0);

                                if (cFileContent.alt_Heading != null && cFileContent.alt_Heading.Trim().Length > 0)
                                {
                                    classStringLibrary.RTX_AppendNL(ref rtx);
                                    classStringLibrary.RTX_AppendText(ref rtx, cFileContent.alt_Heading.Trim(), new Font("Arial", 12, FontStyle.Bold), Color.Blue, 10);
                                }

                                classStringLibrary.RTX_AppendNL(ref rtx);
                                classStringLibrary.RTX_AppendText(ref rtx, cFileContent.Definition.Trim(), new Font("Arial", 12, FontStyle.Bold), Color.Black , 8);

                                return;
                            }
                    }
                }
            }
        }



        private void Rtx_MouseLeave(object sender, EventArgs e)
        {
            if (formDictionaryOutput.rtxCalling != null)
                formDictionaryOutput.rtxCalling.Focus();
            else
                formWords.instance.rtxCK.rtx.Focus();
        }

       

    }
}

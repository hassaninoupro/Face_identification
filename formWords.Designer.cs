namespace Words
{
    partial class formWords
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formWords));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.mnuProject = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProject_New = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProject_Load = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProject_Save = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProject_SaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProject_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDictionarySelection = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuProject,
            this.mnuDictionarySelection});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(674, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // mnuProject
            // 
            this.mnuProject.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuProject_New,
            this.mnuProject_Load,
            this.mnuProject_Save,
            this.mnuProject_SaveAs,
            this.mnuProject_Exit});
            this.mnuProject.Name = "mnuProject";
            this.mnuProject.Size = new System.Drawing.Size(56, 20);
            this.mnuProject.Text = "&Project";
            // 
            // mnuProject_New
            // 
            this.mnuProject_New.Name = "mnuProject_New";
            this.mnuProject_New.Size = new System.Drawing.Size(138, 22);
            this.mnuProject_New.Text = "&New";
            this.mnuProject_New.Click += new System.EventHandler(this.mnuProject_New_Click);
            // 
            // mnuProject_Load
            // 
            this.mnuProject_Load.Name = "mnuProject_Load";
            this.mnuProject_Load.Size = new System.Drawing.Size(138, 22);
            this.mnuProject_Load.Text = "&Load";
            this.mnuProject_Load.Click += new System.EventHandler(this.mnuProject_Load_Click);
            // 
            // mnuProject_Save
            // 
            this.mnuProject_Save.Name = "mnuProject_Save";
            this.mnuProject_Save.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuProject_Save.Size = new System.Drawing.Size(138, 22);
            this.mnuProject_Save.Text = "&Save";
            this.mnuProject_Save.Click += new System.EventHandler(this.mnuProject_Save_Click);
            // 
            // mnuProject_SaveAs
            // 
            this.mnuProject_SaveAs.Name = "mnuProject_SaveAs";
            this.mnuProject_SaveAs.Size = new System.Drawing.Size(138, 22);
            this.mnuProject_SaveAs.Text = "Save &As";
            this.mnuProject_SaveAs.Click += new System.EventHandler(this.mnuProject_SaveAs_Click);
            // 
            // mnuProject_Exit
            // 
            this.mnuProject_Exit.Name = "mnuProject_Exit";
            this.mnuProject_Exit.Size = new System.Drawing.Size(138, 22);
            this.mnuProject_Exit.Text = "E&xit";
            this.mnuProject_Exit.Click += new System.EventHandler(this.mnuProject_Exit_Click);
            // 
            // mnuDictionarySelection
            // 
            this.mnuDictionarySelection.Name = "mnuDictionarySelection";
            this.mnuDictionarySelection.Size = new System.Drawing.Size(124, 20);
            this.mnuDictionarySelection.Text = "Dictionary Selection";
            this.mnuDictionarySelection.Click += new System.EventHandler(this.mnuOptions_DictionarySelection_Click);
            // 
            // formWords
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 478);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "formWords";
            this.Text = "In My Words";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    

        public System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuProject;
        private System.Windows.Forms.ToolStripMenuItem mnuProject_Load;
        private System.Windows.Forms.ToolStripMenuItem mnuProject_SaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuProject_Exit;
        private System.Windows.Forms.ToolStripMenuItem mnuProject_New;
        private System.Windows.Forms.ToolStripMenuItem mnuProject_Save;


        #endregion

        private System.Windows.Forms.ToolStripMenuItem mnuDictionarySelection;
    }
}


using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyFlowLayout
{

    /// <summary>
    /// Represents the form on which the majority of the system is displayed
    /// Allows window flow panel to be displayed and panels to be added and removed
    /// Contains a custom header and title.
    /// </summary>
    public class MainForm : Form, IComparable
    {
        string title;
        MainFormIOController IO;
        MainFormCollection collection;

        FormWindowState LastWindowState;
        public Panel header;
        Label headerTitle;
        Button minimise, maximise, close;

        int formIndex;

        public delegate void MyKeyPressEventHandler(Keys key);
        public event MyKeyPressEventHandler MyKeyPress;

        public MainForm(MainFormIOController IOController, MainFormCollection Collection)
        {
            this.IOController = IOController;
            this.collection = Collection;

            InitializeComponent();
            SetupHeader(this.IOController.FormTitle);

            this.IOController.SetAssociatedForm(this);

            IOController.SetupControls();
            
            Resize += CustomOnResize;
            ResizeEnd += CustomEndResize;
            FormClosed += MainForm_FormClosed;
            IOController.MainPanel.Click += MainPanel_Click;
        }

        /// <summary>
        /// For testing purposes.
        /// </summary>
        public MainForm() { }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (MyKeyPress != null)
                MyKeyPress(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void MainPanel_Click(object sender, EventArgs e)
        {
            if (PanelDroppedOnForm != null)
                PanelDroppedOnForm(this);
        }

        public delegate void FormClosedEventHandler(int FormIndex);
        public event FormClosedEventHandler MainFormClosed;

        public void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainFormClosed(FormIndex);
        }

        void CustomOnResize(object sender, EventArgs e)
        {
            if (WindowState != LastWindowState)
            {
                LastWindowState = WindowState;
                IOController.FlowLayoutEvents.OnMainPanelLayoutChanged();
            }

            LocateComponents();
        }

        void CustomEndResize(object sender, EventArgs e)
        {
            IOController.FlowLayoutEvents.OnMainPanelLayoutChanged();
        }

        void LocateComponents()
        {
            header.Size = new Size(this.ClientRectangle.Right, 17);

            int rightBorder = header.ClientRectangle.Right - 3;
            minimise.Location = new Point(rightBorder - 66, 0);
            maximise.Location = new Point(rightBorder - 49, 0);
            close.Location = new Point(rightBorder - 32, 0);
        }

        public delegate void PanelDroppedEventHandler(MainForm DroppedForm);
        public event PanelDroppedEventHandler PanelDroppedOnForm;

        /// <summary>
        /// Sets up the header of the form with minimise, maximise, and close buttons.
        /// </summary>
        /// <param name="Title">The title to be displayed in the header bar of the system</param>
        void SetupHeader(string Title)
        {
            header = new Panel();
            header.BackgroundImage = Properties.Resources.Form_Header;
            header.Location = new Point(0, 0);

            headerTitle = new Label();
            headerTitle.Text = Title;
            headerTitle.Location = new Point(0, 0);
            headerTitle.Size = new Size(100, 17);
            headerTitle.BackColor = Color.FromArgb(52, 43, 236);
            headerTitle.ForeColor = Color.White;
            header.Controls.Add(headerTitle);

            minimise = new Button();
            minimise.Size = new Size(17, 17);
            minimise.FlatStyle = FlatStyle.Flat;
            minimise.BackgroundImage = Properties.Resources.Hide_Display;
            minimise.MouseLeave += (s, e) => minimise.BackgroundImage = Properties.Resources.Hide_Display;
            minimise.MouseHover += (s, e) => minimise.BackgroundImage = Properties.Resources.Hide_Hover;
            minimise.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            header.Controls.Add(minimise);

            maximise = new Button();
            maximise.Size = new Size(17, 17);
            maximise.BackgroundImage = Properties.Resources.FullScreen_Display;
            maximise.MouseLeave += (s, e) => maximise.BackgroundImage = Properties.Resources.FullScreen_Display;
            maximise.MouseHover += (s, e) => maximise.BackgroundImage = Properties.Resources.FullScreen_Hover;
            maximise.FlatStyle = FlatStyle.Flat;
            maximise.Click += SetButtonImages;
            header.Controls.Add(maximise);

            close = new Button();
            close.Size = new Size(32,17);
            close.FlatStyle = FlatStyle.Flat;
            close.BackgroundImage = Properties.Resources.Close_Display;
            close.MouseHover += (s, e) => close.BackgroundImage = Properties.Resources.Close_Hover;
            close.MouseLeave += (s, e) => close.BackgroundImage = Properties.Resources.Close_Display;
            close.Click += (s, e) => this.Close();
            header.Controls.Add(close);

            LocateComponents();

            this.Controls.Add(header);
        }

        /// <summary>
        /// <para>Alters the form window state after the 'restore' button is clicked.</para>
        /// <para>Changes the buttom image to reflect the current window state.</para>
        /// </summary>
        void SetButtonImages(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                maximise.BackgroundImage = Properties.Resources.RestoreDown_Display;
                maximise.MouseHover += (a, b) => maximise.BackgroundImage = Properties.Resources.RestoreDown_Hover;
                maximise.MouseLeave += (a, b) => maximise.BackgroundImage = Properties.Resources.RestoreDown_Display;
                maximise.MouseHover -= (a, b) => maximise.BackgroundImage = Properties.Resources.FullScreen_Hover;
                maximise.MouseLeave -= (a, b) => maximise.BackgroundImage = Properties.Resources.FullScreen_Display;

            }
            else
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    maximise.BackgroundImage = Properties.Resources.FullScreen_Display;
                    maximise.MouseHover += (a, b) => maximise.BackgroundImage = Properties.Resources.FullScreen_Hover;
                    maximise.MouseLeave += (a, b) => maximise.BackgroundImage = Properties.Resources.FullScreen_Display;
                    maximise.MouseHover -= (a, b) => maximise.BackgroundImage = Properties.Resources.RestoreDown_Hover;
                    maximise.MouseLeave -= (a, b) => maximise.BackgroundImage = Properties.Resources.RestoreDown_Display;
                }
            }
        }

        void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.SuspendLayout();

            this.Name = "MainForm";
            //Change the below two lines to change the window state.
            this.ControlBox = true;
            this.Text = "StratSim";
            this.Icon = (Icon)resources.GetObject("$this.Icon");
            this.Height = Screen.GetWorkingArea(this).Height;
            this.Width = Screen.GetWorkingArea(this).Width;
            this.DoubleBuffered = true;
            this.DesktopLocation = new Point(0, 0);
            this.BackColor = Color.White;

            this.ResumeLayout(true);

        }

        public MainFormCollection Collection
        { get { return collection; } }

        /// <summary>
        /// Gets or sets the index of the form in the list of forms currently active in the system
        /// </summary>
        public int FormIndex
        {
            get { return formIndex; }
            set { formIndex = value; }
        }

        public int CompareTo(object obj)
        {
            return this.FormIndex - ((MainForm)obj).FormIndex;
        }

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                headerTitle.Text = title;
            }
        }
        public virtual MainFormIOController IOController
        {
            get { return IO; }
            set { IO = value; }
        }

    }
}

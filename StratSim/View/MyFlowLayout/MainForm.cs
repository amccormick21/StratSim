using System;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.MyFlowLayout
{
    /// <summary>
    /// Represents the form on which the majority of the system is displayed
    /// Allows window flow panel to be displayed and panels to be added and removed
    /// Contains a custom header and title.
    /// </summary>
    public class MainForm : Form
    {
        MainFormIOController IO;
        FormWindowState LastWindowState;
        public Panel header;
        Button minimise, maximise, close;

        int _formIndex;

        public delegate void MyKeyPressEventHandler(Keys key);
        public event MyKeyPressEventHandler MyKeyPress;

        public MainForm(int FormIndex)
        {
            InitializeComponent();
            SetupHeader();
            IO = new MainFormIOController(this);

            if (FormIndex == 0)
            {
                IO.SetupControls();
            }

            _formIndex = FormIndex;

            Resize += CustomOnResize;
            ResizeEnd += CustomEndResize;
            FormClosed += MainForm_FormClosed;
            IO.MainPanel.Click += MainPanel_Click;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            MyKeyPress(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void MainPanel_Click(object sender, EventArgs e)
        {
            if (PanelDroppedOnForm != null)
                PanelDroppedOnForm(this);
        }

        public void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.FormClosed(_formIndex);
        }

        void CustomOnResize(object sender, EventArgs e)
        {
            if (WindowState != LastWindowState)
            {
                LastWindowState = WindowState;
                FlowLayoutEvents.OnMainPanelLayoutChanged();
            }

            LocateComponents();
        }

        void CustomEndResize(object sender, EventArgs e)
        {
            FlowLayoutEvents.OnMainPanelLayoutChanged();
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

        public void OnPanelAdded(int panelIndex)
        {
            PanelAdded(panelIndex);
        }

        public delegate void PanelAddedEventHandler(int panelIndex);
        public event PanelAddedEventHandler PanelAdded;

        /// <summary>
        /// Sets up the header of the form with minimise, maximise, and close buttons.
        /// </summary>
        void SetupHeader()
        {
            header = new Panel();
            header.BackgroundImage = StratSim.Properties.Resources.Form_Header;
            header.Location = new Point(0, 0);

            Label HeaderTitle = new Label();
            HeaderTitle.Text = "Strat Sim";
            HeaderTitle.Location = new Point(0, 0);
            HeaderTitle.Size = new Size(100, 17);
            HeaderTitle.BackColor = Color.FromArgb(52, 43, 236);
            HeaderTitle.ForeColor = Color.White;
            header.Controls.Add(HeaderTitle);

            minimise = new Button();
            minimise.Size = new Size(17, 17);
            minimise.FlatStyle = FlatStyle.Flat;
            minimise.BackgroundImage = StratSim.Properties.Resources.Hide_Display;
            minimise.MouseLeave += (s, e) => minimise.BackgroundImage = StratSim.Properties.Resources.Hide_Display;
            minimise.MouseHover += (s, e) => minimise.BackgroundImage = StratSim.Properties.Resources.Hide_Hover;
            minimise.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            header.Controls.Add(minimise);

            maximise = new Button();
            maximise.Size = new Size(17, 17);
            maximise.BackgroundImage = StratSim.Properties.Resources.FullScreen_Display;
            maximise.MouseLeave += (s, e) => maximise.BackgroundImage = StratSim.Properties.Resources.FullScreen_Display;
            maximise.MouseHover += (s, e) => maximise.BackgroundImage = StratSim.Properties.Resources.FullScreen_Hover;
            maximise.FlatStyle = FlatStyle.Flat;
            maximise.Click += SetButtonImages;
            header.Controls.Add(maximise);

            close = new Button();
            close.Size = new Size(32,17);
            close.FlatStyle = FlatStyle.Flat;
            close.BackgroundImage = StratSim.Properties.Resources.Close_Display;
            close.MouseHover += (s, e) => close.BackgroundImage = StratSim.Properties.Resources.Close_Hover;
            close.MouseLeave += (s, e) => close.BackgroundImage = StratSim.Properties.Resources.Close_Display;
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
                maximise.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Display;
                maximise.MouseHover += (a, b) => maximise.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Hover;
                maximise.MouseLeave += (a, b) => maximise.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Display;
                maximise.MouseHover -= (a, b) => maximise.BackgroundImage = StratSim.Properties.Resources.FullScreen_Hover;
                maximise.MouseLeave -= (a, b) => maximise.BackgroundImage = StratSim.Properties.Resources.FullScreen_Display;

            }
            else
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    maximise.BackgroundImage = StratSim.Properties.Resources.FullScreen_Display;
                    maximise.MouseHover += (a, b) => maximise.BackgroundImage = StratSim.Properties.Resources.FullScreen_Hover;
                    maximise.MouseLeave += (a, b) => maximise.BackgroundImage = StratSim.Properties.Resources.FullScreen_Display;
                    maximise.MouseHover -= (a, b) => maximise.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Hover;
                    maximise.MouseLeave -= (a, b) => maximise.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Display;
                }
            }
        }

        void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.SuspendLayout();

            this.Name = "MainForm";
            this.ControlBox = false;
            this.Text = String.Empty;
            this.Icon = (Icon)resources.GetObject("$this.Icon");
            this.Height = Screen.GetWorkingArea(this).Height;
            this.Width = Screen.GetWorkingArea(this).Width;
            this.DoubleBuffered = true;
            this.DesktopLocation = new Point(0, 0);
            this.BackColor = Color.White;

            this.ResumeLayout(true);

        }

        /// <summary>
        /// The index of the form in the list of forms currently active in the system
        /// </summary>
        public int FormIndex
        {
            get { return _formIndex; }
        }

        public MainFormIOController IOController
        {
            get{ return IO;}
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyFlowLayout
{
    /// <summary>
    /// <para>Controller providing methods to hide and show panels on the main form.</para>
    /// <para>Contains methods for adding and removing controls,
    /// and for starting the functions used on the form.</para>
    /// </summary>
    public class MainFormIOController : IDisposable, IIOController
    {
        MainForm associatedForm;
        MyToolbar toolbar;
        WindowFlowPanel mainPanel;
        ContentTabControl contentTabControl;
        PanelControlEvents panelEvents;
        FlowLayoutEvents flowEvents;
        string formTitle;

        /// <summary>
        /// Creates a new instance of a MainFormIOController class.
        /// </summary>
        /// <param name="Form">The form to be associated with this controller.</param>
        /// <param name="Collection">The colleciton of forms in which the form is located.</param>
        public MainFormIOController(WindowFlowPanel MainPanel, MyToolbar Toolbar, PanelControlEvents Events, string Title)
        {
            this.mainPanel = MainPanel;
            this.toolbar = Toolbar;
            this.flowEvents = new FlowLayoutEvents();
            this.formTitle = Title;

            SetEvents(Events);
            SubscribeToEvents();
        }

        public virtual MainFormIOController GetNew()
        {
            WindowFlowPanel MainPanel = new WindowFlowPanel();
            PanelControlEvents PanelEvents = new PanelControlEvents();
            MyToolbar Toolbar = new MyToolbar(MainPanel, PanelEvents);
            return new MainFormIOController(MainPanel, Toolbar, PanelEvents, "");
        }

        /// <summary>
        /// Sets the form associated with this IO controller to the specified form.
        /// </summary>
        /// <param name="Form">The new form to link to the IO Controller.</param>
        public virtual void SetAssociatedForm(MainForm Form)
        {
            AssociatedForm = Form;
            AssociatedForm.KeyPreview = true;
            AssociatedForm.FormClosed += form_FormClosed;
            this.MainPanel.LinkToForm(AssociatedForm);
            this.Toolbar.SetForm(AssociatedForm);
            AssociatedForm.Controls.Add(MainPanel);
            AddPanel(Toolbar);
        }

        void form_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Sets up the initial set of controls on the form when it is initialised.
        /// </summary>
        public virtual void SetupControls()
        {
            ContentTabControl = new ContentTabControl(associatedForm);
            FinishedAdding();
        }

        public virtual void SetEvents(PanelControlEvents events)
        {
            this.panelEvents = events;
        }
        public virtual void UnsubscribeFromEvents()
        {
            PanelControlEvents.ShowContentTabControl -= PanelControlEvents_ShowContentTabControl;
        }
        public virtual void SubscribeToEvents()
        {
            PanelControlEvents.ShowContentTabControl += PanelControlEvents_ShowContentTabControl;
        }

        void PanelControlEvents_ShowContentTabControl(MainForm callingForm)
        {
            if (callingForm == AssociatedForm)
            {
                AddPanel(ContentTabControl);
                FinishedAdding();
            }
        }

        /// <summary>
        /// Adds a control to the form.
        /// </summary>
        /// <param name="ControlToAdd">The IDockableControl to add to the form</param>
        public void AddPanel(IDockableControl ControlToAdd)
        {
            if (ControlToAdd != null && ControlToAdd.Removed)
            {
                MainPanel.AddControl(ControlToAdd, true);
                if (ControlToAdd.Type == typeof(MyPanel))
                {
                    Toolbar.AddPanel(((MyPanel)ControlToAdd).PanelIndex);
                    ((MyPanel)ControlToAdd).OnPanelOpened(AssociatedForm);
                }
                if (ControlToAdd.Type == typeof(MyToolbar))
                    ControlToAdd.Removed = false;
            }
            FlowLayoutEvents.OnMainPanelLayoutChanged();
        }
        /// <summary>
        /// <para>Removes a control from the form.</para>
        /// <para>If the control is not on the form, the form it is on is located and the panel is removed from this form.</para>
        /// </summary>
        /// <param name="ControlToRemove">The IDockableControl to remove from the form</param>
        public void RemovePanel(IDockableControl ControlToRemove)
        {
            if (((MyPanel)ControlToRemove).ParentForm.FormIndex == AssociatedForm.FormIndex)
            {
                if (ControlToRemove.Type == typeof(MyPanel))
                {
                    int panelIndexToRemove = ((MyPanel)ControlToRemove).PanelIndex;

                    if (!ControlToRemove.Removed)
                    {
                        if (!((MyPanel)ControlToRemove).InContentPanel)
                        {
                            Toolbar.RemovePanel(panelIndexToRemove);
                            MainPanel.RemoveControl(ControlToRemove);
                            foreach (MyPanel p in MainPanel.VisiblePanels)
                            {
                                if (p.PanelIndex > panelIndexToRemove)
                                {
                                    p.PanelIndex--;
                                }
                            }
                        }
                    }
                    ((MyPanel)ControlToRemove).OnPanelClosed(AssociatedForm);
                    FlowLayoutEvents.OnMainPanelLayoutChanged();
                }
                else
                {
                    FormCollection[((MyPanel)ControlToRemove).ParentForm.FormIndex].IOController.RemovePanel(ControlToRemove);
                }

            }
        }

        /// <summary>
        /// Adds a panel to the content tab control
        /// </summary>
        /// <param name="ControlToAdd">The panel to add within the main tab control</param>
        public void AddContentPanel(MyPanel ControlToAdd)
        {
            ContentTabControl.AddPanelToTabControl(ControlToAdd);
            if (ControlToAdd.Removed)
            {
                ControlToAdd.OnPanelOpened(AssociatedForm);
                ControlToAdd.Removed = false;
            }
        }
        /// <summary>
        /// Removes a panel from the content tab control
        /// </summary>
        /// <param name="ControlToRemove">The panel to remove from the main tab control</param>
        public void RemoveContentPanel(MyPanel ControlToRemove)
        {
            if (!ControlToRemove.Removed)
            {
                ControlToRemove.Removed = true;
                ControlToRemove.OnPanelClosed(AssociatedForm);
            }
            ContentTabControl.RemovePanelFromControl(ControlToRemove);
        }

        public void FinishedAdding()
        {
            MainPanel.FinishedAddingPanels();
        }

        public delegate void StartLayoutEventHandler(MyPanel SizingPanel, List<MyPanel> VisiblePanels, System.Drawing.Point StartLocation, Dictionary<DockTypes, System.Drawing.Point> DockPoints);
        public static event StartLayoutEventHandler StartLayout;

        public void StartReLayout(MyPanel SizingPanel, System.Drawing.Point StartLocation)
        {
            if (StartLayout != null)
            {
                AssociatedForm.Controls.Remove(MainPanel);
                StartLayout(SizingPanel, MainPanel.VisiblePanels, StartLocation, MainPanel.DockPoints);
            }
        }
        public void EndReLayout()
        {
            AssociatedForm.Controls.Add(MainPanel);
        }

        public void ShowDynamicLayoutPanel(Panel LayoutPanel)
        {
            LayoutPanel.Size = associatedForm.ClientSize;
            LayoutPanel.Location = new System.Drawing.Point(0, 0);
            AssociatedForm.Controls.Add(LayoutPanel);
            LayoutPanel.Show();
        }
        public void HideDynamicLayoutPanel(Panel LayoutPanel)
        {
            AssociatedForm.Controls.Remove(LayoutPanel);
            EndReLayout();
        }

        public void OpenPanelInCurrentForm(MyPanel PanelToAddToWindow, MainForm FormToAddOn)
        {
            RemovePanel(PanelToAddToWindow);
            FormToAddOn.IOController.AddPanel(PanelToAddToWindow);
            PanelToAddToWindow.OnOpenedInDifferentForm(FormToAddOn);
            FlowLayoutEvents.OnMainPanelLayoutChanged();
            FormToAddOn.IOController.FlowLayoutEvents.OnMainPanelLayoutChanged();
        }

        public void Dispose()
        {
            MainPanel.Dispose();
            Toolbar.Dispose();
        }

        public MainForm AssociatedForm
        {
            get { return associatedForm; }
            set { associatedForm = value; }
        }
        public virtual ContentTabControl ContentTabControl
        {
            get { return contentTabControl; }
            set { contentTabControl = value; }
        }
        public virtual MyToolbar Toolbar
        {
            get { return toolbar; }
            set { toolbar = value; }
        }
        public virtual WindowFlowPanel MainPanel
        {
            get { return mainPanel; }
            set { mainPanel = value; }
        }
        public virtual PanelControlEvents PanelControlEvents
        {
            get { return panelEvents; }
            set { panelEvents = value; }
        }
        public FlowLayoutEvents FlowLayoutEvents
        {
            get { return flowEvents; }
            set { flowEvents = value; }
        }
        public MainFormCollection FormCollection
        { get { return AssociatedForm.Collection; } }

        public virtual string FormTitle
        {
            get { return formTitle; }
            set { formTitle = value; }
        }
    }
}

using System.Collections.Generic;
using System.Windows.Forms;

namespace MyFlowLayout
{
    /// <summary>
    /// <para>Represents a collection of forms derived from the MainForm class.</para>
    /// <para>Allows the forms to be used for flow layout events</para>
    /// </summary>
    public class MainFormCollection : List<MainForm>
    {
        /// <summary>
        /// Creates a new instance of the list of forms.
        /// </summary>
        public MainFormCollection()
            : base()
        {
        }

        /// <summary>
        /// Removes a form from the list of forms when it is closed.
        /// </summary>
        /// <param name="formIndex">The index of the form that is closed.</param>
        public void FormClosed(int formIndex)
        {
            this.RemoveAt(formIndex);

            foreach (MainForm Form in this)
            {
                if (Form.FormIndex > formIndex)
                {
                    Form.FormIndex--;
                }
            }

            if (this.Count == 0)
            {
                Application.Exit();
            }
        }

        public void AddEventToForms(MainForm.PanelDroppedEventHandler panelDroppedEventHandler)
        {
            foreach (MainForm f in this)
            {
                f.PanelDroppedOnForm += panelDroppedEventHandler;
            }
        }

        public void RemoveEventFromForms(MainForm.PanelDroppedEventHandler panelDroppedEventHandler)
        {
            foreach (MainForm f in this)
            {
                f.PanelDroppedOnForm -= panelDroppedEventHandler;
            }
        }

        /// <summary>
        /// Adds a form to the collection, and assigns it an automatically generated index.
        /// </summary>
        public void AddForm(MainFormIOController IOController)
        {
            MainForm FormToAdd = new MainForm(IOController, this);
            FormToAdd.FormIndex = this.Count;
            FormToAdd.MainFormClosed += FormClosed;
            this.Add(FormToAdd);
        }

        /// <summary>
        /// Opens a panel in a new window
        /// </summary>
        /// <param name="PanelToAddToNewWindow">The panel to open in a new window</param>
        /// <param name="OldForm">The original form on which it was displayed</param>
        public void OpenInNewWindow(MyPanel PanelToAddToNewWindow, MainForm OldForm)
        {
            //Add a new form with a new instance of the IO controller specified by the panel being removed.
            //This means that the new form matches the type of the panel.
            this.AddForm(PanelToAddToNewWindow.ParentForm.IOController.GetNew());
            int indexOfLastForm = this.Count - 1;

            //Remove the panel from the old form and add the panel to the new one.
            OldForm.IOController.RemovePanel(PanelToAddToNewWindow);
            this[indexOfLastForm].IOController.AddPanel(PanelToAddToNewWindow);

            //Fire the opened in new form event for the panel.
            PanelToAddToNewWindow.OnOpenedInDifferentForm(this[indexOfLastForm]);

            //Show the new form.
            this[indexOfLastForm].Show();
        }
    }
}

using System;
using System.Windows.Forms;

namespace MyFlowLayout
{
    /// <summary>
    /// Represents a button used in the context menu, used for changing the properties of the panel
    /// </summary>
    public class MyToolStripButton : ToolStripButton
    {
        int thisButtonIndex;
        Type type;

        //Constructors for buttons based on different properties passed to them
        public MyToolStripButton(int buttonIndex)
        {
            thisButtonIndex = buttonIndex;
            type = typeof(int);
            CheckOnClick = true;
            Click += GenerateClickEvent;
            this.Width = 100;
            this.DisplayStyle = ToolStripItemDisplayStyle.Text;
        }

        public MyToolStripButton(DockTypes dockType)
        {
            thisButtonIndex = (int)dockType;
            type = typeof(DockTypes);
            CheckOnClick = true;
            Click += GenerateClickEvent;
            this.Width = 100;
            this.DisplayStyle = ToolStripItemDisplayStyle.Text;
        }

        public MyToolStripButton(AutosizeTypes autosizeType)
        {
            thisButtonIndex = (int)autosizeType;
            type = typeof(AutosizeTypes);
            CheckOnClick = true;
            Click += GenerateClickEvent;
            this.Width = 100;
            this.DisplayStyle = ToolStripItemDisplayStyle.Text;
        }

        public MyToolStripButton(FillStyles fillStyle)
        {
            thisButtonIndex = (int)fillStyle;
            type = typeof(FillStyles);
            CheckOnClick = true;
            Click += GenerateClickEvent;
            this.Width = 100;
            this.DisplayStyle = ToolStripItemDisplayStyle.Text;
        }

        public virtual void GenerateClickEvent(object sender, EventArgs e)
        {
            ButtonClicked(this.ButtonIndex, this.Type);
        }

        public delegate void OnCheckedEventHandler(int ButtonIndex, Type ButtonType);
        public event OnCheckedEventHandler ButtonClicked;

        public int ButtonIndex
        {
            get { return thisButtonIndex; }
            set { thisButtonIndex = value; }
        }
        public Type Type
        { get { return type; } }
    }

    /// <summary>
    /// Represents a custom context menu, displayed when the header of a panel is right-clicked
    /// Provides options for changing the layout of the panel
    /// </summary>
    class MyContextMenu : ContextMenuStrip
    {
        ToolStripDropDownButton DockTypeDropDown, AutoSizeDropDown, FillStyleDropDown;

        MyPanel _parent;

        public MyContextMenu(MyPanel parent)
        {
            _parent = parent;
            SetupComponents();
        }

        void SetupComponents()
        {
            MyToolStripButton TempButton;

            //Dock Type
            DockTypeDropDown = new ToolStripDropDownButton();
            DockTypeDropDown.Text = "Item Dock Location";
            DockTypeDropDown.ToolTipText = "Select where this item is docked on your screen";

            foreach (var DockType in (DockTypes[])Enum.GetValues(typeof(DockTypes)))
            {
                TempButton = new MyToolStripButton(DockType);
                TempButton.Text = Convert.ToString(DockType);
                TempButton.CheckOnClick = true;
                if (_parent.DockType == DockType) { TempButton.Checked = true; }
                TempButton.ButtonClicked += ContextMenuClick;
                DockTypeDropDown.DropDownItems.Add(TempButton);
            }

            //AutoSize
            AutoSizeDropDown = new ToolStripDropDownButton();
            AutoSizeDropDown.Text = "Item Autosize Properties";
            AutoSizeDropDown.ToolTipText = "Select how this item can be stretched to fill the screen";

            foreach (var AutoSize in (AutosizeTypes[])Enum.GetValues(typeof(AutosizeTypes)))
            {
                TempButton = new MyToolStripButton(AutoSize);
                TempButton.Text = Convert.ToString(AutoSize);
                TempButton.CheckOnClick = true;
                if (_parent.AutoSizeType == AutoSize) { TempButton.Checked = true; }
                TempButton.ButtonClicked += ContextMenuClick;
                AutoSizeDropDown.DropDownItems.Add(TempButton);
            }

            AutoSizeDropDown.Width = 100;

            //Fill Style
            FillStyleDropDown = new ToolStripDropDownButton();
            FillStyleDropDown.Text = "Item Fill Properties";
            FillStyleDropDown.ToolTipText = "Select whether this item fills the screen or is finite-size";

            foreach (var FillStyle in (FillStyles[])Enum.GetValues(typeof(FillStyles)))
            {
                TempButton = new MyToolStripButton(FillStyle);
                TempButton.Text = Convert.ToString(FillStyle);
                TempButton.CheckOnClick = true;
                if (_parent.FillStyle == FillStyle) { TempButton.Checked = true; }
                TempButton.ButtonClicked += ContextMenuClick;
                FillStyleDropDown.DropDownItems.Add(TempButton);
            }
            FillStyleDropDown.Width = 100;

            this.Items.Add(DockTypeDropDown);
            this.Items.Add(AutoSizeDropDown);
            this.Items.Add(FillStyleDropDown);
        }

        /// <summary>
        /// Selects the correct buttons in the context menu based on panel properties
        /// </summary>
        public void SetCheckButtons()
        {
            foreach (var DockType in (DockTypes[])Enum.GetValues(typeof(DockTypes)))
            {
                ((MyToolStripButton)DockTypeDropDown.DropDownItems[(int)DockType]).Checked = (_parent.DockType == DockType);
            }
            foreach (var AutoSize in (AutosizeTypes[])Enum.GetValues(typeof(AutosizeTypes)))
            {
                ((MyToolStripButton)AutoSizeDropDown.DropDownItems[(int)AutoSize]).Checked = (_parent.AutoSizeType == AutoSize);
            }
            foreach (var FillStyle in (FillStyles[])Enum.GetValues(typeof(FillStyles)))
            {
                ((MyToolStripButton)FillStyleDropDown.DropDownItems[(int)FillStyle]).Checked = (_parent.FillStyle == FillStyle);
            }
        }

        /// <summary>
        /// Handles a click event on a context menu button
        /// </summary>
        void ContextMenuClick(int buttonIndex, Type buttonType)
        {
            int buttonIndexIncrement = 0;
            if (buttonType == typeof(DockTypes))
            {
                foreach (MyToolStripButton ts in DockTypeDropDown.DropDownItems)
                {
                    ts.Checked = (buttonIndexIncrement++ == buttonIndex);
                }
                (_parent).DockType = (DockTypes)buttonIndex;
            }
            if (buttonType == typeof(AutosizeTypes))
            {
                foreach (MyToolStripButton ts in AutoSizeDropDown.DropDownItems)
                {
                    ts.Checked = (buttonIndexIncrement++ == buttonIndex);
                }
                (_parent).AutoSizeType = (AutosizeTypes)buttonIndex;
            }
            if (buttonType == typeof(FillStyles))
            {
                foreach (MyToolStripButton ts in FillStyleDropDown.DropDownItems)
                {
                    ts.Checked = (buttonIndexIncrement++ == buttonIndex);
                }
                (_parent).FillStyle = (FillStyles)buttonIndex;
            }
        }
    }

}

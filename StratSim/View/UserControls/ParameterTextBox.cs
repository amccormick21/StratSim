using DataSources;
using StratSim.Model;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.UserControls
{
    /// <summary>
    /// Represents a text box that displays a pace parameter datum.
    /// Contains methods for validating and formatting the text box based on the contents.
    /// </summary>
    class ParameterTextBox : TextBox
    {
        /// <summary>
        /// Enum containing options for the way the control will be validated
        /// </summary>
        internal enum ValidationMethod { LessThan, GreaterThan, Between };

        string parameterValue = "";

        public int DriverIndex, ParameterIndex;
        public float originalValue;
        public bool textChanged = false;

        ValidationMethod validationMethod;

        float upperBound, lowerBound;

        /// <summary>
        /// Creates a new instance of a ParameterTextBox which is validated either greater than or less than a specified boundary.
        /// </summary>
        /// <param name="initialValue">The initial value to display in the text box</param>
        /// <param name="bound">The limiting value for validation of the input</param>
        /// <param name="boundIsUpperBound">True if the control must be less than the specified bound; otherwise false</param>
        public ParameterTextBox(int driverIndex, int parameterIndex, float initialValue, float bound, bool boundIsUpperBound)
        {
            if (boundIsUpperBound)
            { upperBound = bound; validationMethod = ValidationMethod.LessThan; }
            else
            { lowerBound = bound; validationMethod = ValidationMethod.GreaterThan; }

            InitialiseComponent(driverIndex, parameterIndex, initialValue);
        }
        /// <summary>
        /// Creates a new instance of a ParameterText box which is validated between two specified values
        /// </summary>
        public ParameterTextBox(int driverIndex, int parameterIndex, float initialValue, float _lowerBound, float _upperBound)
        {
            validationMethod = ValidationMethod.Between;
            lowerBound = _lowerBound;
            upperBound = _upperBound;

            InitialiseComponent(driverIndex, parameterIndex, initialValue);
        }

        void InitialiseComponent(int driverIndex, int parameterIndex, float initialValue)
        {
            DriverIndex = driverIndex;
            ParameterIndex = parameterIndex;
            originalValue = initialValue;
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;

            Leave += ParameterTextBox_Leave;
            TextChanged += ParameterTextBox_TextChanged;
        }

        /// <summary>
        /// Converts a value to be displayed into a string that can be set as the text for the control.
        /// </summary>
        public string GetParameterText(float valueToDisplay)
        {
            return Convert.ToString(Math.Round(valueToDisplay, 3));
        }

        /// <summary>
        /// Sets the text box properties including color and text
        /// </summary>
        /// <param name="value">Parameter value to display</param>
        /// <param name="IsDefault">Value representing whether the value is equal to the default value</param>
        /// <param name="IsAnomaly">Value representing whether the value is an anomaly</param>
        public void SetProperties(float value, bool IsDefault, bool IsAnomaly)
        {
            if (IsDefault)
            {
                BackColor = Color.Yellow;
            }
            else
            {
                BackColor = Color.Cyan;
                if (!textChanged) { BackColor = Color.White; }
            }

            if (IsAnomaly)
            {
                ForeColor = Color.Red;
            }
            else
            {
                ForeColor = DefaultForeColor;
            }

            parameterValue = Convert.ToString(Math.Round(value, 3));
            Text = parameterValue;
            textChanged = false;
        }

        void ParameterTextBox_TextChanged(object sender, EventArgs e)
        {
            textChanged = true;
        }

        void ParameterTextBox_Leave(object sender, EventArgs e)
        {
            ParameterValue = Text;
        }

        /// <summary>
        /// Validates the value that is intended for the cell
        /// </summary>
        /// <param name="value">The intended cell contents</param>
        /// <returns>True if the value can be accepted</returns>
        bool Validate(float value)
        {
            bool incorrect = false;

            switch (validationMethod)
            {
                case ValidationMethod.LessThan: Functions.ValidateLessThan(value, upperBound, "", ref incorrect, true); break;
                case ValidationMethod.GreaterThan: Functions.ValidateGreaterThan(value, lowerBound, "", ref incorrect, true); break;
                case ValidationMethod.Between: Functions.ValidateBetweenExclusive(value, lowerBound, upperBound, "", ref incorrect, true); break;
            }

            return !incorrect;
        }

        public float UpperBound
        {
            get { return upperBound; }
            set { upperBound = value; }
        }

        public float LowerBound
        {
            get { return lowerBound; }
            set { lowerBound = value; }
        }
        public string ParameterValue
        {
            get { return parameterValue; }
            set
            {
                float parameterNumber;

                bool valid = float.TryParse(value, out parameterNumber);

                if (valid)
                {
                    if (Validate(parameterNumber))
                    {
                        parameterValue = Convert.ToString(Math.Round(parameterNumber, 3));
                        var parameterChangeEventArgs = new TextChangedEventArgs(this);
                        OnParameterChanged(parameterChangeEventArgs);
                    }
                    else
                    {
                        Text = parameterValue;
                    }
                }
                else
                {
                    Text = parameterValue;
                }
            }
        }

        public delegate void textChangedEventHandler(TextChangedEventArgs e);
        public event textChangedEventHandler OnParameterChanged;
    }

    /// <summary>
    /// Event arguments containing data about the text box when the text is changed.
    /// </summary>
    class TextChangedEventArgs : EventArgs
    {
        string value;
        int driverIndex;
        int parameterIndex;
        ParameterTextBox textBox;

        public string Value
        { get { return value; } }
        public int DriverIndex
        { get { return driverIndex; } }
        public int ParameterIndex
        { get { return parameterIndex; } }
        public ParameterTextBox TextBox
        { get { return textBox; } }

        /// <summary>
        /// Creates new TextChangedEventArgs
        /// </summary>
        /// <param name="t">The text box that fired the original event</param>
        public TextChangedEventArgs(ParameterTextBox t)
        {
            value = t.Text;
            driverIndex = t.DriverIndex;
            parameterIndex = t.ParameterIndex;
            textBox = t;
        }
    }

}

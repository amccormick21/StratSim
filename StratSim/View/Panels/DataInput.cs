using DataSources;
using StratSim.Model;
using StratSim.Model.Files;
using StratSim.View.MyFlowLayout;
using StratSim.ViewModel;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MyFlowLayout;
using DataSources.DataConnections;
using System.Collections.Generic;
using System.Net;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Control to be displayed in the content tab control allowing
    /// data to be loaded from PDF files
    /// </summary>
    public class DataInput : MyPanel
    {
        //properties
        const int leftBorder = 10;
        const int topBorder = 10;
        const int defaultHeight = 15;
        const int defaultWidth = 200;

        Size cbxDefault = new Size(defaultWidth, defaultHeight);
        Size btnDefault = new Size(100, 25);

        DataController DataController = new DataController();

        //objects
        ComboBox SelectRace = new ComboBox();
        ComboBox SelectSession = new ComboBox();
        Label EnterText = new Label();
        Button SelectText = new Button();
        Button AnalyseData = new Button();
        Button ShowGridWindow;

        /// <summary>
        /// Starts the data input panel with nothing selected
        /// </summary>
        public DataInput(MainForm FormToAdd)
            : base(450, 240, "Load Data", FormToAdd, Properties.Resources.Input)
        {
            InitialiseObjects();
            LoadTrackList();
        }

        /// <summary> 
        /// Sets the combo boxes to display the race and session indices
        /// </summary>
        public void ResetPanel(int roundIndex, int sessionIndex)
        {
            if (SelectRace.SelectedIndex != roundIndex)
                SelectRace.SelectedIndex = roundIndex;
            if (SelectSession.SelectedIndex != sessionIndex)
                SelectSession.SelectedIndex = sessionIndex;
        }

        void cbxSelectRace_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectSession.Items.Count == 0)
                LoadSessionList();
            Data.RaceIndex = SelectRace.SelectedIndex;
        }

        void cbxSelectSession_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get text from the PDF and process this text.
            var timingFileText = GetTextFromSelectedPDFFile((Session)SelectSession.SelectedIndex, SelectRace.SelectedIndex, SelectRace.Text);
            if (timingFileText != "")
                ProcessTimingData(timingFileText);
        }

        void btnSelectText_Click(object sender, EventArgs e)
        {
            var dataFromClipboard = Clipboard.GetText();
            ProcessTimingData(dataFromClipboard);
        }

        private void ProcessTimingData(string timingFileContent)
        {
            Session session = (Session)SelectSession.SelectedIndex;

            if (session == Session.Grid)
            {
                //Start the grid loading window to finish grid setup
                //The data controller will process data when the grid window is confirmed.
                StartGridWindow(timingFileContent);
                ShowGridWindow.Visible = true;
            }
            else
            {
                //Update the data immediately
                DataController.ProcessData(timingFileContent, session, SelectRace.SelectedIndex);
            }

            int nextSessionIndex = GetNextSessionIndex(session);
            if (nextSessionIndex != -1)
            {
                ResetPanel(SelectRace.SelectedIndex, nextSessionIndex);
            }
        }

        void StartGridWindow(string timingFileContent)
        {
            int[] gridOrder = GridData.GetGridOrderFromFileText(timingFileContent, Properties.Settings.Default.CurrentYear);
            ((StratSimPanelControlEvents)ParentForm.IOController.PanelControlEvents).OnStartGridPanel(ParentForm, gridOrder);
            Program.GridPanel.PositionsConfirmed += GridPanel_PositionsConfirmed;
        }

        void GridPanel_PositionsConfirmed(object sender, string e)
        {
            DataController.ProcessData(e, Session.Grid, Data.RaceIndex);
        }

        void btnAnalyseData_Click(object sender, EventArgs e)
        {
            StartParameterCalculations();
        }

        public void InitialiseObjects()
        {
            SelectRace.Location = new Point(leftBorder, topBorder + 25);
            SelectRace.Size = cbxDefault;
            SelectRace.Text = "Select race";
            SelectRace.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            SelectRace.SelectedIndexChanged += cbxSelectRace_SelectedIndexChanged;
            this.Controls.Add(SelectRace);

            SelectSession.Location = new Point(leftBorder, topBorder * 2 + defaultHeight + 25);
            SelectSession.Size = cbxDefault;
            SelectSession.Text = "Select session";
            SelectSession.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            SelectSession.SelectedIndexChanged += cbxSelectSession_SelectedIndexChanged;
            this.Controls.Add(SelectSession);

            SelectText.Location = new Point(leftBorder * 2 + 200, topBorder + 25);
            SelectText.Size = btnDefault;
            SelectText.Text = "Confirm data entry";
            SelectText.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            SelectText.Click += btnSelectText_Click;
            SelectText.Visible = true;
            this.Controls.Add(SelectText);

            AnalyseData.Location = new Point(leftBorder * 2 + 200, topBorder * 2 + btnDefault.Height + 25);
            AnalyseData.Size = btnDefault;
            AnalyseData.Text = "Analyse Data";
            AnalyseData.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            AnalyseData.Click += btnAnalyseData_Click;
            this.Controls.Add(AnalyseData);

            ShowGridWindow = new Button();
            ShowGridWindow.Location = new Point(leftBorder * 3 + 300, topBorder + 25);
            ShowGridWindow.Size = btnDefault;
            ShowGridWindow.Text = "Show Grid Window";
            ShowGridWindow.Visible = false;
            ShowGridWindow.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            ShowGridWindow.Click += ShowGridWindow_Click;
            this.Controls.Add(ShowGridWindow);
        }

        void ShowGridWindow_Click(object sender, EventArgs e)
        {
            ((StratSimPanelControlEvents)ParentForm.IOController.PanelControlEvents).OnShowGridPanel(ParentForm);
        }

        void LoadTrackList()
        {
            foreach (Track t in Data.Tracks)
            {
                SelectRace.Items.Add(t.name);
            }
        }
        void LoadSessionList()
        {
            foreach (Session session in (Session[])Enum.GetValues(typeof(Session)))
            {
                SelectSession.Items.Add(session);
            }
        }

        /// <summary>
        /// Gets the next session to set the data input panel to display.
        /// </summary>
        /// <param name="currentSession">The session currently displayed</param>
        /// <param name="dataType">The data type currently displayed</param>
        /// <returns>An integer representing the session index to display</returns>
        int GetNextSessionIndex(Session currentSession)
        {
            if (currentSession == Session.Race)
            {
                return -1;
            }
            else
            {
                int current = (int)currentSession;
                return current + 1;
            }
        }

        void StartParameterCalculations()
        {
            bool allSessionsLoaded = true;
            string sessionsNotLoaded = "";
            int session = 0;

            //set up error message for sessions not yet loaded
            foreach (KeyValuePair<Session, bool> b in DataController.sessionDataLoaded)
            {
                if (b.Value == false)
                {
                    allSessionsLoaded = false;
                    sessionsNotLoaded += '\n' + " " +
                        (session == 4 ? "Speeds"
                        : (session == 5 ? "Grid" : b.Key.GetSessionName())
                        );
                }
                session++;
            }

            if (allSessionsLoaded)
            {
                ((StratSimPanelControlEvents)PanelControlEvents).OnLoadPaceParametersFromRace();
                ((StratSimPanelControlEvents)PanelControlEvents).OnShowPaceParameters(ParentForm);
            }
            else
            {
                //start a dialog
                string message = "Data from the following sessions has not been loaded. This may cause incorrect results. Continue?";
                message += sessionsNotLoaded;
                string caption = "Data Not Fully Loaded";

                if (Functions.StartDialog(message, caption))
                {
                    ((StratSimPanelControlEvents)PanelControlEvents).OnLoadPaceParametersFromRace();
                    ((StratSimPanelControlEvents)PanelControlEvents).OnShowPaceParameters(ParentForm);
                }
            }

        }

        public string GetTextFromSelectedPDFFile(Session session, int raceIndex, string raceName)
        {
            string fileText = "";

            if (session == Session.Grid && Properties.Settings.Default.CurrentYear != 2015)
            {
                fileText = TryGetGridData(Properties.Settings.Default.CurrentYear, raceIndex);
            }
            else
            {
                string fileName = "";
                fileName += session.GetSessionName();
                fileName += " ";
                fileName += session.GetPDFFileName();
                fileName += ".pdf";

                string path = Path.Combine(Data.Settings.TimingDataBaseFolder, Convert.ToString(Properties.Settings.Default.CurrentYear), raceName);

                //Check folder exists and create folder if it doesn't.
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileLocation = Path.Combine(path, fileName);
                if (!File.Exists(fileLocation))
                {
                    try
                    {
                        //Try downloading from the old location
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(session.GetPDFUrl(raceIndex, Data.Tracks[raceIndex].abbreviation, Properties.Settings.Default.CurrentYear), fileLocation);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        try
                        {
                            string source;
                            WebRequest req;
                            try
                            {
                                req = WebRequest.Create(GetDataURL(raceIndex, Properties.Settings.Default.CurrentYear, true));
                                req.Method = "GET";
                                using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream()))
                                {
                                    source = reader.ReadToEnd();
                                }
                            }
                            catch (WebException)
                            {
                                req = WebRequest.Create(GetDataURL(raceIndex, Properties.Settings.Default.CurrentYear, false));
                                req.Method = "GET";
                                using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream()))
                                {
                                    source = reader.ReadToEnd();
                                }
                            }
                            string url = GetPdfUrlFromHtmlParse(source, session);
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(url, fileLocation);
                            }
                        }
                        catch (InvalidOperationException)
                        {

                        }
                    }
                }

                if (File.Exists(fileLocation))
                {
                    //Parse the file and return the contents
                    fileText = GetPDFText(fileLocation);
                }
            }
            return fileText;
        }

        private string GetPdfUrlFromHtmlParse(string source, Session session)
        {
            //Find the start of the links to the timing data
            int timingDataStartIndex = source.IndexOf(@">TIMING INFORMATION<");
            int timingDataEndIndex = source.IndexOf(@">TECHNICAL REPORTS<", timingDataStartIndex);
            source = source.Substring(timingDataStartIndex, timingDataEndIndex - timingDataStartIndex);
            string sessionName = session.GetSessionHeading();
            int sessionDataStartIndex = source.IndexOf(sessionName);
            string fileName = session.GetTimingDataType().GetPDFFileName(session);
            string linkText = source.Substring(sessionDataStartIndex, source.Length - sessionDataStartIndex);
            int startOfLinkIdentifier = linkText.IndexOf(fileName);
            linkText = linkText.Substring(0, startOfLinkIdentifier);
            int startOfLink = linkText.LastIndexOf(" href=\"") + 8;
            int endOfLink = linkText.IndexOf('"', startOfLink);
            string link = linkText.Substring(startOfLink, endOfLink-startOfLink);
            link = "http://www.fia.com/" + link;
            return link;
        }

        private string GetDataURL(int raceIndex, int currentYear, bool digitOne)
        {
            if (currentYear == 2014)
                throw new InvalidOperationException("Cannot retrieve grid data for 2014");

            //Default URL:
            string url = "http://www.fia.com/events/fia-formula-" + (digitOne ? "1" : "one") + "-world-championship/season-";
            url += currentYear.ToString();
            url += @"/event-timing-information";
            if (currentYear == 2015)
            {
                //If race index is 0, this is correct. Spain (4) is also weird for 2015
                if (raceIndex != 0 && raceIndex != 4)
                {
                    //Otherwise, add the index
                    url += "-" + (raceIndex - 1).ToString();
                }
            }
            return url;
        }

        private string TryGetGridData(int currentYear, int roundIndex)
        {
            if (currentYear == 2014)
                throw new InvalidOperationException("Cannot retrieve grid data for 2014");

            string webAddress = "http://www.fia.com/events/fia-formula-1-world-championship/season-";
            webAddress += currentYear.ToString();
            webAddress += @"/qualifying-classification";
            if (roundIndex != 0)
                webAddress += "-" + (roundIndex - 1).ToString();

            string html;
            using (var htmlClient = new WebClient())
            {
                html = htmlClient.DownloadString(webAddress);
                int qualiStartIndex = html.IndexOf("QUALIFYING - CLASSIFICATION");
                html = html.Substring(qualiStartIndex);
                qualiStartIndex = html.IndexOf("<tbody>");
                var qualiEndIndex = html.IndexOf("</tbody>");
                html = html.Substring(qualiStartIndex, qualiEndIndex - qualiStartIndex);
            }
            return html;
        }

        /// <summary>
        /// Returns the text content of the specified PDF
        /// </summary>
        /// <param name="pdfFilePath">The file path of the pdf</param>
        public static string GetPDFText(string pdfFilePath)
        {
            PDDocument doc = null;
            try
            {
                doc = PDDocument.load(pdfFilePath);
                PDFTextStripper stripper = new PDFTextStripper();
                return stripper.getText(doc);
            }
            finally
            {
                if (doc != null)
                {
                    doc.close();
                }
            }
        }
    }
}

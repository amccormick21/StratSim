using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using StratSim;
using StratSim.Model;
using System.IO;
using StratSim.Model.Files;
using StratSim.View.Panels;
using DataSources.DataConnections;

namespace StratSimTest
{
    [TestClass]
    public class GenericTesting
    {
        [TestMethod]
        public void GetControlColor()
        {

        }

        [TestMethod]
        public void ModifyQualifyingRecordData()
        {
            //Reads in a file of driver indices and replaces with the race numbers of the drivers
            //Used for converting qualifying data to driver race numbers
            //NOW DEPRECATED AND WILL CORRUPT DATA. DO NOT USE.
            /*
            int[] driverNumbers2014 = { 1, 3, 14, 7, 22, 20, 8, 13, 6, 44, 99, 21, 27, 11, 19, 77, 25, 26, 9, 10, 17, 4 };
            int numberOfTracks = 0;
            var tracks = Track.InitialiseTracks(out numberOfTracks);

            int driverIndex;
            int positionIndex;
            int[] driverNumberQualifyingOrder;
            string filePath = "";

            foreach (var track in tracks)
            {
                positionIndex = 0;
                driverNumberQualifyingOrder = new int[driverNumbers2014.Length];
                filePath = @"C:\Users\Alex\SkyDrive\Documents\Projects\StratSim\3.1.9\Strategy Simulation V2\RaceData\" + track.name + "\\" + track.name + " Qualifying Grid.txt";
                using (var sr = new StreamReader(filePath))
                {
                    while (!sr.EndOfStream)
                    {
                        driverIndex = Convert.ToInt32(sr.ReadLine());
                        driverNumberQualifyingOrder[positionIndex] = driverNumbers2014[driverIndex];
                        positionIndex++;
                    }
                }

                using (var sw = new StreamWriter(filePath))
                {
                    foreach (var driverNumber in driverNumberQualifyingOrder)
                    {
                        if (driverNumber != 0)
                        {
                            sw.WriteLine(driverNumber);
                        }
                    }
                }
            }
            */
        }

        [TestMethod]
        public void TestGetTeamIndexFromDriverIndex()
        {
            int numberOfDriversInSeason = -1;
            Driver[] drivers = Driver.InitialiseDrivers(out numberOfDriversInSeason, 2014);
            Team[] teams = Team.InitialiseTeams(2014);

            Assert.AreNotEqual(-1, numberOfDriversInSeason);

            int teamIndex;
            for (int driverIndex = 0; driverIndex < numberOfDriversInSeason; driverIndex++)
            {
                teamIndex = Driver.GetTeamIndex(driverIndex, drivers, teams);
                Assert.AreNotEqual(-1, teamIndex, string.Format("Driver Index: {0}", driverIndex));
                Assert.AreEqual(teams[teamIndex].TeamName, drivers[driverIndex].Team);
            }
        }

        [TestMethod]
        public void TestGetCalendarID()
        {
            int calendarId = DataSources.DataConnections.RaceCalendarConnection.GetRaceCalendarID(1, 2014);
            Assert.AreEqual(1, calendarId);
        }

        [TestMethod]
        public void TestGetRoundNumber()
        {
            int roundNumber = DataSources.DataConnections.RaceCalendarConnection.GetRoundNumber(8);
            Assert.AreEqual(8, roundNumber);
        }

        [TestMethod]
        public void TestGetTracks()
        {
            int numberOfTracks;
            var tracks = Track.InitialiseTracks(out numberOfTracks, 2015);

            Assert.AreEqual(19, numberOfTracks);
            Assert.AreEqual("BAHRAIN", tracks[3].name);
            Assert.AreEqual(21f, tracks[0].pitStopLoss, 0.2);
        }

        [TestMethod]
        public void TestGetTextFromPDF()
        {
            var pdfPath = @"C:\Users\Alex\SkyDrive\Documents\Projects\F1\Race Reports\2014\Australia\Qualifying Preliminary Classification.pdf";
            var text = DataInput.GetPDFText(pdfPath);
            var intended = "Qualifying Session Preliminary Classification";
            var firstLine = text.Substring(0, intended.Length);

            Assert.AreEqual(intended, firstLine);
        }

        [TestMethod]
        public void TestGetPdfUrl()
        {
            Session session = Session.FP1;

            //Test 2015
            var pdfUrl = session.GetPDFUrl(0, "aus", 2015);
            Assert.AreEqual("http://www.fia.com/sites/default/files/2015_01_aus_f1_p1_timing_firstpracticesessionlaptimes_v01.pdf".ToLower(), pdfUrl.ToLower());

            //Test 2014
            pdfUrl = session.GetPDFUrl(0, "aus", 2014);
            Assert.AreEqual("http://www.fia.com/sites/default/files/championship/event_report/documents/2014_01_aus_f1_p1_timing_firstpracticesessionlaptimes_v01.pdf".ToLower(), pdfUrl.ToLower());
        }
    }
}

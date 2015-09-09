using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSources.DataConnections
{
    public enum Session
    {
        FP1,
        FP2,
        FP3,
        Qualifying,
        SpeedTrap,
        Grid,
        Race
    }

    public static class SessionExtensionMethods
    {
        public const int LapDataSessions = 5;

        public static int GetSessionIndex(this Session session)
        {
            switch (session)
            {
                case Session.FP1:
                    return 0;
                case Session.FP2:
                    return 1;
                case Session.FP3:
                    return 2;
                case Session.Qualifying:
                case Session.SpeedTrap:
                case Session.Grid:
                    return 3;
                case Session.Race:
                    return 4;
                default:
                    return -1; //Should never be reached
            }
        }

        public static TimingDataType GetTimingDataType(this Session session)
        {
            switch (session)
            {
                case Session.FP1:
                    return TimingDataType.LapTimeData;
                case Session.FP2:
                    return TimingDataType.LapTimeData;
                case Session.FP3:
                    return TimingDataType.LapTimeData;
                case Session.Qualifying:
                    return TimingDataType.LapTimeData;
                case Session.SpeedTrap:
                    return TimingDataType.SpeedData;
                case Session.Grid:
                    return TimingDataType.GridData;
                case Session.Race:
                    return TimingDataType.LapTimeData;
                default:
                    return 0; //Should never be reached
            }
        }

        public static string GetPDFFileName(this Session session)
        {
            TimingDataType dataType = GetTimingDataType(session);
            return dataType.GetPDFFileName(session);
        }

        public static string GetSessionAbbreviation(this Session session)
        {
            switch (session)
            {
                case Session.FP1:
                    return "P1";
                case Session.FP2:
                    return "P2";
                case Session.FP3:
                    return "P3";
                case Session.Qualifying:
                    return "Q0";
                case Session.SpeedTrap:
                    return "Q0";
                case Session.Grid:
                    return "Q0";
                case Session.Race:
                    return "R0";
                default:
                    return ""; //Should never be reached
            }
        }

        public static string GetPDFUrl(this Session session, int raceIndex, string trackAbbreviation, int currentYear)
        {
            string url = "";
            //Does not work for grid - this needs to be done manually.
            if ((session == Session.Grid) && (currentYear == 2014))
                throw new InvalidOperationException("Cannot automatically retrieve data for the grid");

            //Needs to be modified according to the folders available each year.
            string location = "";
            switch (currentYear)
            {
                case 2014: location = "http://www.fia.com/sites/default/files/championship/event_report/documents/"; break;
                case 2015: location = "http://www.fia.com/sites/default/files/"; break;
            }

            string fileName = currentYear.ToString() + "_" + (raceIndex + 1).ToString().PadLeft(2, '0') +"_" + trackAbbreviation + "_F1_" + session.GetSessionAbbreviation();
            fileName += "_Timing_" + session.GetSessionFullName() + session.GetPDFFileName().Replace(" ", "") + "_V01.pdf";
            url = (location + fileName).ToLower();
            return url;
        }

        public static string GetTextFileName(this Session session)
        {
            TimingDataType dataType = GetTimingDataType(session);
            return dataType.GetTextFileName();
        }

        public static string GetSessionFullName(this Session session)
        {
            int sessionIndex = GetSessionIndex(session);

            switch (sessionIndex)
            {
                case 0:
                    return "FirstPracticeSession";
                case 1:
                    return "SecondPracticeSession";
                case 2:
                    return "ThirdPracticeSession";
                case 3:
                    return "QualifyingSession";
                case 4:
                    return "Race";
                default:
                    return "";
            }
        }

        public static string GetSessionName(this Session session)
        {
            int sessionIndex = GetSessionIndex(session);

            switch (sessionIndex)
            {
                case 0:
                    return "First Practice Session";
                case 1:
                    return "Second Practice Session";
                case 2:
                    return "Third Practice Session";
                case 3:
                    return "Qualifying";
                case 4:
                    return "Race";
                default:
                    return "";
            }
        }

        public static string GetSessionHeading(this Session session)
        {
            switch (session)
            {
                case Session.FP1:
                    return "First Practice";
                case Session.FP2:
                    return "Second Practice";
                case Session.FP3:
                    return "Third Practice";
                case Session.Qualifying:
                    return "Qualifying Session";
                case Session.SpeedTrap:
                    return "Qualifying Session";
                case Session.Grid:
                    return "Race";
                case Session.Race:
                    return "Race";
                default:
                    return ""; //Should never be reached
            }
        }

        public static Session GetSessionFromIndex(int sessionIndex)
        {
            switch (sessionIndex)
            {
                case 0:
                    return Session.FP1;
                case 1:
                    return Session.FP2;
                case 2:
                    return Session.FP3;
                case 3:
                    return Session.Qualifying;
                case 4:
                    return Session.Race;
                default:
                    return 0;
            }
        }

        
        public static Session GetFromFileName(string file)
        {
            //It is known that this is lap data. Hence simply checking the first letter is sufficient.
            switch (file[0])
            {
                case 'F': //First practice
                    return Session.FP1;
                case 'S': //Second practice
                    return Session.FP2;
                case 'T':
                    return Session.FP3;
                case 'Q':
                    return Session.Qualifying;
                case 'R':
                    return Session.Race;
            }

            return 0;
        }
    }
}

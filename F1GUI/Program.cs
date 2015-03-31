using MyFlowLayout;
using StratSim.View.MyFlowLayout;
using System;
using System.Windows.Forms;
using TeamStats;
using TeamStats.MyFlowLayout;

namespace F1GUI
{
    static class Program
    {
        static MainFormCollection MainForms;
        static DragDropController DragDropController;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForms = new MainFormCollection();
            DragDropController = new DragDropController(MainForms);

            //StratSim:
            MainForms.AddForm(StratSim.Program.StartProject());
            ((StratSimFormIOController)MainForms[0].IOController).ShowStartPanel();
            MainForms[0].Show();

            //TeamStats:
            //MainForms.AddForm(TeamStats.Program.StartProject());
            //((TeamStatsFormIOController)MainForms[1].IOController).ShowChampionshipsPanel();
            //MainForms[1].Show();

            Application.Run();
        }
    }
}

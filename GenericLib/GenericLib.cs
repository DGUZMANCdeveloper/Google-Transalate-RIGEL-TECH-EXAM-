using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using MiTools;
using RPABaseAPI;
using Cartes;
using System.Threading.Tasks;
using Serilog;
using System.Xml;

namespace GLib
{
    public class GenericLib: MyCartesAPIBase
    {
       
        private static bool loaded = false;
        public int CicloFrase;
        private string FraseOutput;

        // Variables de Cartes
        private RPAWin32Component Chrome = null;
        private RPAWin32Component FlechaInit = null;
        private RPAWin32Component FlechaOutput = null;
        private RPAWin32Component BuscarInit = null;
        private RPAWin32Component BuscarOutput = null;
        private RPAWin32Component panelFraseInit = null;
        private RPAWin32Component panelFraseoutput = null;

        public GenericLib(MyCartesProcess owner) : base (owner)
        {
        }

        protected virtual string getNeededRPASuiteVersion() // It returns a string with the version of RPA Suite needed by this library
        {
            return "3.4.2.1";
        }
        protected override void MergeLibrariesAndLoadVariables()
        {
            if (!loaded || (Execute("isVariable(\"$Chrome\");") != "1"))
            {
                Chrome = null;
                loaded = cartes.merge(CurrentPath + "\\Cartes\\Traductor.cartes.rpa") == 1;
            }
            if (FlechaInit == null)
            {
                FlechaInit = GetComponent<RPAWin32Component>("$FlechaInit");
                FlechaOutput = GetComponent<RPAWin32Component>("$FlechaOut");
                BuscarInit = GetComponent<RPAWin32Component>("$BuscarInit");
                BuscarOutput = GetComponent<RPAWin32Component>("$BuscarOut");
                panelFraseInit = GetComponent<RPAWin32Component>("$Panelinit");
                panelFraseoutput = GetComponent<RPAWin32Component>("$Panelout");
            }
        }

        public void AbrirTraductor(string  url, int timeout)
        {
            string[] kill = { "chrome" };
            restartProcess("Reiniciando Edge", kill, "chrome.exe" + url);
        }

        public string Traducir(string Inputlanguage, string Outputlanguage, string FraseInit, int timeout)
        {
            void secuenciaTraducir()
            {
                if(CicloFrase == 0)
                {
                    FlechaInit.click();
                    BuscarInit.TypeFromClipboard(Inputlanguage);
                    BuscarInit.Press(13, 1);
                    FlechaOutput.click();
                    BuscarOutput.TypeFromClipboard(Outputlanguage);
                    BuscarInit.Press(13, 1);
                }
                panelFraseInit.TypeFromClipboard(FraseInit);
                reset(panelFraseInit);
                Thread.Sleep(500);
                panelFraseoutput.focus();
                FraseOutput = panelFraseoutput.name();

            }
            sequence(secuenciaTraducir, panelFraseInit, timeout, "Iniciando traducción", "No fue posible traducir");
            return FraseOutput;
        }

        private void sequence(Action seq, RPAWin32Component checkComponent,int tout, string mensajeinicio, string mensajeError)
        {
            bool exit;
            DateTime timeout;
            timeout = DateTime.Now.AddSeconds(tout);
            exit = false;
            do
            {
                CheckAbort();
                cartes.reset(checkComponent.api());
                if (timeout < DateTime.Now) throw new Exception("timeout");
                else
                {
                    if (checkComponent.componentexist(10) == 1)
                    {
                        cartes.balloon(mensajeinicio);
                        Log.Information(mensajeinicio);
                        seq();
                        exit = true;
                    }
                    else
                    {
                        cartes.balloon(mensajeError);
                        Log.Error(mensajeError);
                        throw new Exception(mensajeError);
                    }
                }
            } while (!exit);
        }

        private void restartProcess(string LogMessage, string[] ProccessNames, string proccessPath)
        {
            foreach(var Application in ProccessNames)
            {
                try
                {
                    Log.Information(LogMessage);
                    Process[] processInstances = Process.GetProcessesByName(Application);
                    foreach (Process p in processInstances)
                               p.Kill();
                }
                catch
                {
                    //nothing
                }
                
            }
            if(proccessPath != null)
            {
                cartes.run(proccessPath);
            }
        }

        public override void Close()
        {
            void seqClose()
            {
                try
                {
                    throw new Exception("KILLING CHROME");
                }
                catch
                {
                    //nothing
                }
            }
            sequence(seqClose, Chrome, 10, "Cerrando ciclo", "Error cerrando ciclo");
        }
    }
}

using GLib;
using RPABaseAPI;
using System;
using System.Collections.Generic;
using System.Xml;
using MiTools;
using Cartes;
using Serilog;


namespace Traductor
{
    class Logica : MyCartesProcessBase
    {
        private GenericLib fGeneric;
        bool errorShare = false;
        int maxretry;
        int timeout;
        string toReport;
        DateTime timestart = DateTime.Now;
        DateTime timefinish;
        string source_language;
        string final_language;
        string url;
        List<string> phrases = new List<string>();
        List<string> finalPhrases = new List<string>();
        XmlDocument XmlCfgB2B = new XmlDocument();

        public Logica() : base ()
        {
            fGeneric = null;
        }
        protected virtual string getNeededRPASuiteVersion() // It returns a string with the version of RPA Suite needed by this library
        {
            return "3.4.2.1";
        }
        protected override string getRPAMainFile()
        {
            return @"Cartes\Google-translator.cartes.rpa";
        }
        protected override void LoadConfiguration(XmlDocument XmlCfg)
        {
            
        }

        protected override void DoExecute(ref DateTime start)
        {
            try
            {

                //Cargamos la configuración
                void loadConfigurations()
                {
                    try
                    {
                        XmlCfgB2B.Load(CurrentPath + "\\settings.xml");
                        source_language = XmlCfgB2B.GetElementsByTagName("source")[0].InnerText;
                        final_language = XmlCfgB2B.GetElementsByTagName("destination")[0].InnerText;
                        maxretry = Int32.Parse(XmlCfgB2B.GetElementsByTagName("maxretry")[0].InnerText);
                        timeout = Int32.Parse(XmlCfgB2B.GetElementsByTagName("timeout")[0].InnerText);
                        timestart = DateTime.Now;
                    }
                    catch
                    {
                        throw new Exception("Error cargando configuraciones");
                    }

                }
                errorShare = retryLoop(loadConfigurations, "CARGANDO CONFIGUACIONES", "ESPERE","CARGANDO...","ERROR1", timeout, 1, false, loadConfigurations);
                //Datos de transacción
                void getTransactionData()
                {
                    //Leer las palabras que se traducen
                    try
                    {
                        //obtenemos 
                        for (int i = 0; i < XmlCfgB2B.GetElementsByTagName("input").Count; i++)
                        {
                            phrases.Add(XmlCfgB2B.GetElementsByTagName("input")[i].InnerText);
                        }
                    }
                    catch
                    {
                        Log.Information("Error obteniendo frases a traducir");
                        throw new Exception("Error obteniendo frases a traducir");
                    }
                }
                errorShare = retryLoop(getTransactionData, "Obteniendo datos","ESPERE","UN MINUTO MAS","ERROR2", timeout, 1, false, getTransactionData);
                void InitProccess()
                {
                    //Comenzamos
                    try
                    {
                        url = " https://translate.google.com/?h1=es";
                        Traductor.AbrirTraductor(url, timeout);
                    }
                    catch
                    {
                        Log.Information("Error abriendo Google");
                        throw new Exception("Error abriendo Google Chrome");
                    }
                }
                errorShare = retryLoop(InitProccess, "Inicializando", "Espere por favor", "Un minuto mas", "ERROR3",60, 1, false, InitProccess);
                //Proceso de transaccion
                void proccessTransactionData()
                {
                    Traductor.CicloFrase = 0;
                    foreach (string fraseAtraducir in phrases)
                    {
                        try
                        {
                            
                            string FraseTraducida = Traductor.Traducir(source_language, final_language, fraseAtraducir, 60);
                            finalPhrases.Add(FraseTraducida);
                            Traductor.CicloFrase++;

                        }
                        catch
                        {
                            
                            throw new Exception("Error en la traduccion");
                        }
                    }
                    for (int i = 0; i < XmlCfgB2B.GetElementsByTagName("input").Count; i++)
                    {
                        XmlCfgB2B.GetElementsByTagName("output")[i].InnerText = finalPhrases[i];
                    }
                    XmlCfgB2B.Save(CurrentPath + "\\settings.xml");
                }
                errorShare = retryLoop(proccessTransactionData, "transaccion", "Continuamos", "un minuto mas", "ERROR4", timeout, 1, false, proccessTransactionData);
            }
            finally
            {
                Traductor.Close();
            }
        }


        private bool retryLoop(Action sequence, string logstart, string logSuccess, string LogRetry, string logerror, int timeout, int maxretry, bool errorShare, Action restore)
        {
            bool error = false;
            int retry = 0;
            bool exit = false;
            do
            {
                try
                {
                    if (errorShare)
                    {
                        retry = maxretry + 1;
                        throw new Exception(logerror);
                    }
                    else
                    {
                        Log.Information(logstart);

                        sequence();

                        exit = true;

                        Log.Information(logSuccess);
                    }
                }
                catch (Exception e)
                {
                    if (retry <= maxretry)
                    {
                        Console.WriteLine(e);
                        retry++;
                        Traductor.CicloFrase = 0;
                        Log.Warning("Reintento numero {retry}" + LogRetry, retry);
                        cartes.balloon("reintento ahora " + retry.ToString() + " " + LogRetry);
                    }
                    else
                    {
                        Console.WriteLine(e);
                        cartes.balloon(logerror);
                        Log.Error(logerror);
                        exit = true;
                        error = true;
                    }
                }
            } while (!exit);
            return error;
        }


        public  GenericLib Traductor
        {
            get
            {
                if (fGeneric == null)
                    fGeneric = new GenericLib(this);
                return fGeneric;
            }
            
        }
    }
    
}

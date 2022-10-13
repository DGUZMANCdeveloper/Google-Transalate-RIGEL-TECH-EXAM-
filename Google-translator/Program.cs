using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GLib;
using Traductor;

namespace Google_translator
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logica DanRot = null;
            DanRot = new Logica();
            DanRot.Execute();
        }
    }
}

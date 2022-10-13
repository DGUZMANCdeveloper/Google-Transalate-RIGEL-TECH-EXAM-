using Cartes;
using MiTools;
using RPABaseAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSEDGE
{
    public class MSEDGELIB : MyCartesAPIBase
    {
        protected RPAWin32Component edgeclose = null;
        public  MSEDGELIB(MyCartesProcess owner) : base(owner)
        {

        }
        protected override void MergeLibrariesAndLoadVariables()
        {
            if (!isVariable("$Edgeexit"))
            {
                cartes.merge(CurrentPath + "\\Cartes\\MSEDGE.cartes.rpa");
            }
            if (edgeclose == null)
            {
                edgeclose = (RPAWin32Component)cartes.component("$Edgeexit");
            }

        }
        public override void Close()
        {
            bool exit;
            DateTime timeout;

            timeout = DateTime.Now.AddSeconds(60);
            exit = false;
            do
            {
                reset(edgeclose);
                Thread.Sleep(200);
                CheckAbort();
                if (timeout < DateTime.Now) throw new MyException(EXIT_ERROR
, "Timeout.");
                else if (edgeclose.ComponentExist()) edgeclose.click();
                else exit = true;

            } while (!exit);
        }

        public void Open()
        {
            bool exit;
            DateTime timeout;
            try
            {
                timeout = DateTime.Now.AddSeconds(60);
                exit = false;
                do
                {
                    reset(edgeclose);
                    Thread.Sleep(200);
                    CheckAbort();
                    if (timeout < DateTime.Now) throw new MyException(EXIT_ERROR, "timeout.");
                    else if (edgeclose.ComponentExist()) exit = true;
                    else
                    {
                        cartes.run("microsoft-edge:");
                        ComponentsExist(30, edgeclose);
                    }
                } while (!exit);
            }
            catch (Exception e)
            {
                forensic("MSEDGE.open", e);
                throw;
            }
        }
    }
}

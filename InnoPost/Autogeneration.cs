using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InnoPost
{
    public static class Autogeneration
    {
        public static string GenerarConsulta(string tablaNombre, string PrefijoConsulta, string tablaAlias, List<Columna> ListaDeColumnas)
        {
            string Consulta = "CREATE OR REPLACE FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "()\r\nRETURNS TABLE (";

            foreach (Columna c in ListaDeColumnas)
            {
                Consulta += c.Nombre + " " + c.Tipo + ", ";
            }

            Consulta = Consulta.Remove(Consulta.Length - 2, 2); // Quitamos el espacio y la última coma

            Consulta += ")\r\nLANGUAGE 'plpgsql'\r\nCOST 100\r\nVOLATILE SECURITY DEFINER\r\nROWS 1000\r\nSET search_path='" + tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];

            Consulta += "'\r\nAS $BODY$\r\n\r\nBEGIN\r\nreturn query SELECT ";

            foreach (Columna c in ListaDeColumnas)
            {
                Consulta += tablaAlias + "." + c.Nombre + ", ";
            }

            Consulta = Consulta.Remove(Consulta.Length - 2, 2); // Quitamos el espacio y la última coma

            Consulta += " FROM " + tablaNombre + " " + tablaAlias + ";";
            Consulta += "\r\nEND;\r\n$BODY$;\r\n\r\nALTER FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() OWNER TO postgres;";
            Consulta += "\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() TO postgres;";
            Consulta += "\r\nREVOKE ALL ON FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() FROM PUBLIC;";
            return Consulta;
        }


        public static string GenerarEliminacion(string tablaNombre, string PrefijoEliminacion, List<Columna> ListaDeColumnas)
        {
            string Eliminacion = "CREATE OR REPLACE FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(_" + ListaDeColumnas[0].Nombre + " " + ListaDeColumnas[0].Tipo + ")\r\nRETURNS integer " + "\r\nLANGUAGE 'plpgsql'\r\n\r\nCOST 100\r\nVOLATILE SECURITY DEFINER\r\nSET search_path='" + tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
            Eliminacion += "'\r\nAS $BODY$\r\n\r\nBEGIN\r\n\r\ndelete from " + tablaNombre + " where " + ListaDeColumnas[0].Nombre + "=_" + ListaDeColumnas[0].Nombre;
            Eliminacion += ";\r\n\r\nRETURN 1; -- Éxito\r\n\r\nEND;\r\n$BODY$;\r\nALTER FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") OWNER TO postgres;";
            Eliminacion += "\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") TO postgres;";
            Eliminacion += "\r\nREVOKE ALL ON FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") FROM PUBLIC;";
            return Eliminacion;
        }

        public class Columna
        {
            public string Nombre { get; set; }
            public string Tipo { get; set; }

            public Columna(string nombre, string tipo)
            {
                Nombre = nombre;
                Tipo = tipo;
            }
        }
    }
}

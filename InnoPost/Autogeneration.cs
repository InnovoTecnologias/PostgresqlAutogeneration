using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InnoPost
{
    public static class Autogeneration
    {
        public static string ProcesarNombre(string tablaDef)
        {
            return tablaDef.Substring(tablaDef.IndexOf("CREATE TABLE ") + 13, tablaDef.IndexOf("(") - tablaDef.IndexOf("CREATE TABLE ") - 13).Replace("\t", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        }

        public static string GenerarConsulta(string tablaNombre, string PrefijoConsulta, string tablaAlias, List<Columna> ListaDeColumnas, string usuarioExtra)
        {
            string Retorno = "CREATE OR REPLACE FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "()\r\nRETURNS TABLE (";

            foreach (Columna c in ListaDeColumnas)
            {
                Retorno += c.Nombre + " " + (c.Tipo.Contains("character varying") ? "varchar" : c.Tipo) + ", ";
            }

            Retorno = Retorno.Remove(Retorno.Length - 2, 2); // Quitamos el espacio y la última coma

            Retorno += ")\r\nLANGUAGE 'plpgsql'\r\nCOST 100\r\nVOLATILE SECURITY DEFINER\r\nROWS 1000\r\nSET search_path='" + tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];

            Retorno += "'\r\nAS $BODY$\r\n\r\nBEGIN\r\nreturn query SELECT ";

            foreach (Columna c in ListaDeColumnas)
            {
                Retorno += tablaAlias + "." + c.Nombre + ", ";
            }

            Retorno = Retorno.Remove(Retorno.Length - 2, 2); // Quitamos el espacio y la última coma

            Retorno += " FROM " + tablaNombre + " " + tablaAlias + ";";
            Retorno += "\r\nEND;\r\n$BODY$;\r\n\r\nALTER FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() OWNER TO postgres;";
            Retorno += "\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() TO postgres;";
            if (usuarioExtra!="") Retorno += "\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() TO " + usuarioExtra +";";
            Retorno += "\r\nREVOKE ALL ON FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() FROM PUBLIC;";
            return Retorno;
        }

        public static string GenerarInsercion(string tablaNombre, string PrefijoInsercion, string tablaAlias, List<Columna> ListaDeColumnas, string usuarioExtra)
        {
            string Retorno = "CREATE OR REPLACE FUNCTION " + tablaNombre + "_" + PrefijoInsercion + "(\r\n";

            for (int i=1; i <ListaDeColumnas.Count;i++)
            {
                Retorno += "_" + ListaDeColumnas[i].Nombre + " " + (ListaDeColumnas[i].Tipo.Contains("character varying") ? "varchar" : ListaDeColumnas[i].Tipo) + ",\r\n";
            }

            Retorno = Retorno.Remove(Retorno.Length - 3, 3);

            Retorno += ")\r\nRETURNS " + ListaDeColumnas[0].Tipo + "\r\nLANGUAGE 'plpgsql'\r\nCOST 100\r\nVOLATILE SECURITY DEFINER\r\nSET search_path = '" + tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
            Retorno += "'\r\nAS $BODY$\r\n\r\nBEGIN\r\n\r\nINSERT INTO " + tablaNombre + "(";

            for (int i = 1; i < ListaDeColumnas.Count; i++)
            {
                Retorno += ListaDeColumnas[i].Nombre + ", ";
            }

            Retorno = Retorno.Remove(Retorno.Length - 2, 2); // Quitamos el espacio y la última coma
            Retorno += ") VALUES (";
            
            for (int i = 1; i < ListaDeColumnas.Count; i++)
            {
                Retorno += "_" + ListaDeColumnas[i].Nombre + ", ";
            }

            Retorno = Retorno.Remove(Retorno.Length - 2, 2); // Quitamos el espacio y la última coma
            Retorno += "); \r\n\r\nreturn (SELECT currval('" + tablaNombre + "_" + ListaDeColumnas[0].Nombre + "_seq')); -- OK\r\n\r\nEND;\r\n$BODY$;\r\n\r\nALTER FUNCTION ";
            Retorno += tablaNombre + "_" + PrefijoInsercion + "(";
            
            for (int i = 1; i < ListaDeColumnas.Count; i++)
            {
                if (ListaDeColumnas[i].Tipo.Contains("character varying")) Retorno += "varchar, ";
                else Retorno += ListaDeColumnas[i].Tipo + ", ";
            }

            Retorno = Retorno.Remove(Retorno.Length - 2, 2); // Quitamos el espacio y la última coma

            Retorno += ") OWNER TO postgres;\r\n\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoInsercion + "(";

            for (int i = 1; i < ListaDeColumnas.Count; i++)
            {
                if (ListaDeColumnas[i].Tipo.Contains("character varying")) Retorno += "varchar, ";
                else Retorno += ListaDeColumnas[i].Tipo + ", ";
            }

            Retorno = Retorno.Remove(Retorno.Length - 2, 2); // Quitamos el espacio y la última coma

            Retorno += ") TO postgres;";

            if (usuarioExtra != "")
            {
                Retorno += "\r\n\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoInsercion + "(";

                for (int i = 1; i < ListaDeColumnas.Count; i++)
                {
                    if (ListaDeColumnas[i].Tipo.Contains("character varying")) Retorno += "varchar, ";
                    else Retorno += ListaDeColumnas[i].Tipo + ", ";
                }

                Retorno = Retorno.Remove(Retorno.Length - 2, 2);
                Retorno += ") TO " + usuarioExtra + ";";
            }

            Retorno += "\r\n\r\nREVOKE ALL ON FUNCTION " + tablaNombre + "_" + PrefijoInsercion + "(";

            for (int i = 1; i < ListaDeColumnas.Count; i++)
            {
                if (ListaDeColumnas[i].Tipo.Contains("character varying")) Retorno += "varchar, ";
                else Retorno += ListaDeColumnas[i].Tipo + ", ";
            }
            Retorno = Retorno.Remove(Retorno.Length - 2, 2);

            Retorno += ") FROM PUBLIC;";

            return Retorno;
        }

        public static string GenerarModificacion(string tablaNombre, string PrefijoModificacion, string tablaAlias, List<Columna> ListaDeColumnas, string usuarioExtra)
        {
            string Retorno = "CREATE OR REPLACE FUNCTION " + tablaNombre + "_" + PrefijoModificacion + "(\r\n";

            foreach (Columna c in ListaDeColumnas)
            {
                Retorno += "_" + c.Nombre + " " + (c.Tipo.Contains("character varying") ? "varchar" : c.Tipo) + ",\r\n";
            }

            Retorno = Retorno.Remove(Retorno.Length - 3, 3);
            Retorno += ")\r\nRETURNS integer\r\nLANGUAGE 'plpgsql'\r\n\r\nCOST 100\r\nVOLATILE SECURITY DEFINER\r\nSET search_path = '" + tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0]; ;
            Retorno += "'\r\nAS $BODY$\r\n\r\nBEGIN\r\n\r\nUPDATE " + tablaNombre + " set ";

            for (int i = 1; i < ListaDeColumnas.Count; i++)
            {
                Retorno += ListaDeColumnas[i].Nombre + "=_" + ListaDeColumnas[i].Nombre + ", ";
            }
            Retorno = Retorno.Remove(Retorno.Length - 2, 2);

            Retorno += " WHERE " + ListaDeColumnas[0].Nombre + "=_" + ListaDeColumnas[0].Nombre + ";\r\n\r\nreturn 1; --OK\r\n\r\nEND;\r\n$BODY$;\r\n\r\nALTER FUNCTION ";

            Retorno += tablaNombre + "_" + PrefijoModificacion + "(";

            foreach (Columna c in ListaDeColumnas)
            {
                if (c.Tipo.Contains("character varying")) Retorno += "varchar, ";
                else Retorno += c.Tipo + ", ";
            }
            Retorno = Retorno.Remove(Retorno.Length - 2, 2);

            Retorno += ") OWNER TO postgres;\r\n\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoModificacion + "(";

            foreach (Columna c in ListaDeColumnas)
            {
                if (c.Tipo.Contains("character varying")) Retorno += "varchar, ";
                else Retorno += c.Tipo + ", ";
            }
            Retorno = Retorno.Remove(Retorno.Length - 2, 2);

            Retorno += ") TO postgres;";

            if (usuarioExtra != "")
            {
                Retorno += "\r\n\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoModificacion + "(";

                foreach (Columna c in ListaDeColumnas)
                {
                    if (c.Tipo.Contains("character varying")) Retorno += "varchar, ";
                    else Retorno += c.Tipo + ", ";
                }

                Retorno = Retorno.Remove(Retorno.Length - 2, 2);
                Retorno += ") TO " + usuarioExtra + ";";
            }
            
            Retorno += "\r\n\r\nREVOKE ALL ON FUNCTION " + tablaNombre + "_" + PrefijoModificacion + "(";

            foreach (Columna c in ListaDeColumnas)
            {
                if (c.Tipo.Contains("character varying")) Retorno += "varchar, ";
                else Retorno += c.Tipo + ", ";
            }
            Retorno = Retorno.Remove(Retorno.Length - 2, 2);

            Retorno += ") FROM PUBLIC;";

            return Retorno;
        }

        public static string GenerarEliminacion(string tablaNombre, string PrefijoEliminacion, List<Columna> ListaDeColumnas, string usuarioExtra)
        {
            string Retorno = "CREATE OR REPLACE FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(_" + ListaDeColumnas[0].Nombre + " " + ListaDeColumnas[0].Tipo + ")\r\nRETURNS integer " + "\r\nLANGUAGE 'plpgsql'\r\n\r\nCOST 100\r\nVOLATILE SECURITY DEFINER\r\nSET search_path='" + tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
            Retorno += "'\r\nAS $BODY$\r\n\r\nBEGIN\r\n\r\ndelete from " + tablaNombre + " where " + ListaDeColumnas[0].Nombre + "=_" + ListaDeColumnas[0].Nombre;
            Retorno += ";\r\n\r\nRETURN 1; -- Éxito\r\n\r\nEND;\r\n$BODY$;\r\nALTER FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") OWNER TO postgres;";
            Retorno += "\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") TO postgres;";
            if (usuarioExtra != "") Retorno += "\r\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") TO " + usuarioExtra + ";";
            Retorno += "\r\nREVOKE ALL ON FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") FROM PUBLIC;";
            return Retorno;
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

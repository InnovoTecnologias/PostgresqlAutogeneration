using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PostgresqlAutogeneration
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = @"-- Table: proyectos.equipo
		                    -- DROP TABLE proyectos.equipo;

                            CREATE TABLE proyectos.equipo
                            (
                                id integer NOT NULL DEFAULT nextval('proyectos.equipo_id_seq'::regclass),
                                idcliente integer NOT NULL,
                                idtipo integer NOT NULL,
                                nombre character varying(250) COLLATE pg_catalog.""default"" NOT NULL,
                                numero integer NOT NULL,
                                usuario character varying(250) COLLATE pg_catalog.""default"" NOT NULL,
                                cpu character varying(250) COLLATE pg_catalog.""default"" NOT NULL,
                                ram integer NOT NULL,
                                fuentes character varying(250) COLLATE pg_catalog.""default"",
                                opticos character varying(250) COLLATE pg_catalog.""default"",
                                ip character varying(50) COLLATE pg_catalog.""default"",
                                mascara character varying(50) COLLATE pg_catalog.""default"",
                                gateway character varying(50) COLLATE pg_catalog.""default"",
                                dhcp boolean,
                                so character varying(50) COLLATE pg_catalog.""default"",
                                codigo character varying(50) COLLATE pg_catalog.""default"",
                                CONSTRAINT equipo_pkey PRIMARY KEY (id),
                                CONSTRAINT fk_equipo_idcliente FOREIGN KEY (idcliente)
                                    REFERENCES ventas.cliente (id) MATCH SIMPLE
                                    ON UPDATE NO ACTION
                                    ON DELETE NO ACTION,
                                CONSTRAINT fk_equipo_idtipo FOREIGN KEY (idtipo)
                                    REFERENCES proyectos.equipo_tipo (id) MATCH SIMPLE
                                    ON UPDATE NO ACTION
                                    ON DELETE NO ACTION
                            )
                            WITH (
                                OIDS = FALSE
                            )
                            TABLESPACE pg_default;

                            ALTER TABLE proyectos.equipo
                                OWNER to postgres;";
        }



        private void button1_Click(object sender, EventArgs e)
        {
            string tablaDef = textBox1.Text;
            string PrefijoConsulta = "obtener", PrefijoInsercion = "agregar", PrefijoModificacion = "modificar", PrefijoEliminacion = "eliminar";

            string tablaNombre = tablaDef.Substring(tablaDef.IndexOf("CREATE TABLE ") + 13, tablaDef.IndexOf("(") - tablaDef.IndexOf("CREATE TABLE ") - 14);
            string tablaAlias = (tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))[1].Substring(0, 1);

            bool HayUnoQueAbre = false;
            int FinDeDefinicion = 0;

            for (int i = tablaDef.IndexOf("(") + 1; i < tablaDef.Length; i++)
            {
                if (tablaDef[i] == '(') HayUnoQueAbre = true;
                else
                {
                    if (tablaDef[i] == ')')
                    {
                        if (HayUnoQueAbre) HayUnoQueAbre = false;
                        else
                        {
                            FinDeDefinicion = i;
                            break;
                        }
                    }
                }
            }

            string[] ColumnasYRestricciones = tablaDef.Substring(tablaDef.IndexOf("(") + 1, FinDeDefinicion - tablaDef.IndexOf("(") - 1).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            List<Columna> ListaDeColumnas = new List<Columna>();

            foreach (string unElemento in ColumnasYRestricciones)
            {
                if (!(unElemento.TrimStart().StartsWith("CONSTRAINT ")))
                {
                    string[] unaColumna = unElemento.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string tipoColumna = "";

                    if (unaColumna[1] == "character") tipoColumna = unaColumna[1] + " " + unaColumna[2];
                    else tipoColumna = unaColumna[1];

                    ListaDeColumnas.Add(new Columna(unaColumna[0], tipoColumna));
                }
            }


            // Generamos la función de Consulta
            string Consulta = "CREATE OR REPLACE FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "()\nRETURNS TABLE(";

            foreach (Columna c in ListaDeColumnas)
            {
                Consulta += c.Nombre + " " + c.Tipo + ", ";
            }

            Consulta = Consulta.Remove(Consulta.Length - 2, 2); // Quitamos el espacio y la última coma

            Consulta += ")\nLANGUAGE 'plpgsql'\nCOST 100\nVOLATILE SECURITY DEFINER\nROWS 1000\nSET search_path='" + tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];

            Consulta += "'\nAS $BODY$\n\nBEGIN\nreturn query SELECT ";

            ;

            foreach (Columna c in ListaDeColumnas)
            {
                Consulta += tablaAlias + "." + c.Nombre + ", ";
            }

            Consulta = Consulta.Remove(Consulta.Length - 2, 2); // Quitamos el espacio y la última coma

            Consulta += " FROM " + tablaNombre + " " + tablaAlias + ";";
            Consulta += "\nEND;\n$BODY$;\n\nALTER FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() OWNER TO postgres;";
            Consulta += "\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() TO postgres;";
            Consulta += "\nREVOKE ALL ON FUNCTION " + tablaNombre + "_" + PrefijoConsulta + "() FROM PUBLIC;";

            //Console.WriteLine(Consulta);

            // Generamos la función de Eliminación
            string Eliminacion = "CREATE OR REPLACE FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(_" + ListaDeColumnas[0].Nombre + " " + ListaDeColumnas[0].Tipo + ")\nRETURNS integer " + "\nLANGUAGE 'plpgsql'\n\nCOST 100\nVOLATILE SECURITY DEFINER\nSET search_path='" + tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
            Eliminacion += "'\nAS $BODY$\n\nBEGIN\n\ndelete from " + tablaNombre + " where " + ListaDeColumnas[0].Nombre + "=_" + ListaDeColumnas[0].Nombre;
            Eliminacion += ";\n\nRETURN 1; -- Éxito\n\nEND;\n$BODY$;\nALTER FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") OWNER TO postgres;";
            Eliminacion += "\nGRANT EXECUTE ON FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") TO postgres;";
            Eliminacion += "\nREVOKE ALL ON FUNCTION " + tablaNombre + "_" + PrefijoEliminacion + "(" + ListaDeColumnas[0].Tipo + ") FROM PUBLIC;";

            textBox2.Text = Consulta + "\r\n\r\n" + Eliminacion;
        }
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

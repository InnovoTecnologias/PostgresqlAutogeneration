using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InnoPost;

namespace PostgresqlAutogeneration
{
    public partial class Form1 : Form
    {
        private string tablaNombre;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txbTableDef.Text = @"-- Table: proyectos.equipo
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
            bool HayUnoQueAbre = false;
            int FinDeDefinicion = 0;

            for (int i = txbTableDef.Text.IndexOf("(") + 1; i < txbTableDef.Text.Length; i++)
            {
                if (txbTableDef.Text[i] == '(') HayUnoQueAbre = true;
                else
                {
                    if (txbTableDef.Text[i] == ')')
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

            string[] ColumnasYRestricciones = txbTableDef.Text.Substring(txbTableDef.Text.IndexOf("(") + 1, FinDeDefinicion - txbTableDef.Text.IndexOf("(") - 1).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            List<Autogeneration.Columna> ListaDeColumnas = new List<Autogeneration.Columna>();

            foreach (string unElemento in ColumnasYRestricciones)
            {
                if (!(unElemento.TrimStart().StartsWith("CONSTRAINT ")))
                {
                    string[] unaColumna = unElemento.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string tipoColumna = "";

                    if (unaColumna[1] == "character") tipoColumna = unaColumna[1] + " " + unaColumna[2];
                    else tipoColumna = unaColumna[1];

                    ListaDeColumnas.Add(new Autogeneration.Columna(unaColumna[0], tipoColumna));
                }
            }
            
            textBox2.Text = Autogeneration.GenerarConsulta(tablaNombre, txbGetPrefix.Text, "e", ListaDeColumnas) + "\r\r\n\r\r\n" + Autogeneration.GenerarEliminacion(tablaNombre, txbDeletePrefix.Text, ListaDeColumnas);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (txbTableDef.Text=="") txbTableDef.Text = Clipboard.GetText();
            tablaNombre = txbTableDef.Text.Substring(txbTableDef.Text.IndexOf("CREATE TABLE ") + 13, txbTableDef.Text.IndexOf("(") - txbTableDef.Text.IndexOf("CREATE TABLE ") - 43);
            txbTableAlias.Text = (tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))[1].Substring(0, 1);
        }
    }
}

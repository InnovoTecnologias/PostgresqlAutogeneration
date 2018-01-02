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
        
        private void button1_Click(object sender, EventArgs e)
        {
            ProcesarNombreYAlias();
            
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
            
            textBox2.Text = Autogeneration.GenerarConsulta(tablaNombre, txbGetPrefix.Text, txbTableAlias.Text, ListaDeColumnas, txbExtraUser.Text) + "\r\n\r\n" + Autogeneration.GenerarInsercion(tablaNombre, txbAddPrefix.Text, txbTableAlias.Text, ListaDeColumnas, txbExtraUser.Text) + "\r\n\r\n" + Autogeneration.GenerarModificacion(tablaNombre, txbUpdatePrefix.Text, txbTableAlias.Text, ListaDeColumnas, txbExtraUser.Text) + "\r\n\r\n" + Autogeneration.GenerarEliminacion(tablaNombre, txbDeletePrefix.Text, ListaDeColumnas, txbExtraUser.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            txbTableDef.Text = Clipboard.GetText();
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }

        private void ProcesarNombreYAlias()
        {
            tablaNombre = Autogeneration.ProcesarNombre(txbTableDef.Text);
            if (txbTableAlias.Text=="") txbTableAlias.Text = (tablaNombre.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))[1].Substring(0, 1);
        }
    }
}

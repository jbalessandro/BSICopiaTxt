using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BSICopiaTxt
{
    public partial class frmBSICopiaTxt : Form
    {
        public frmBSICopiaTxt()
        {
            InitializeComponent();

            txtOrigem.Text = @"C:\BSI\Origem";
            txtDestino.Text = @"C:\BSI\Destino";
            txtBackup.Text = @"C:\BSI\Backup";

            try
            {
                Copiar();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao copiar: " + ex.Message, "Erro", MessageBoxButtons.OK);
            }
        }

        private void btnCopiar_Click(object sender, EventArgs e)
        {
            Copiar();
        }

        private void Copiar()
        {
            var copier = new Copier(new Location
            {
                Backup = txtBackup.Text,
                Destino = txtDestino.Text,
                Origem = txtOrigem.Text
            });

            copier.MakeCopy();
        }
    }

    public class Copier
    {
        private Location _location;

        public Copier(Location location)
        {
            _location = location;

            validarLocation();
        }

        private void validarLocation()
        {
            if (string.IsNullOrEmpty(_location.Backup) ||
                string.IsNullOrEmpty(_location.Origem) ||
                string.IsNullOrEmpty(_location.Destino))
            {
                throw new ArgumentNullException("Favor informar a localização dos três diretórios");
            }

            if (!Directory.Exists(_location.Origem))
            {
                throw new ArgumentException("Diretório origem inexistente");
            }

            if (!Directory.Exists(_location.Destino))
            {
                throw new ArgumentException("Diretório destino inexistente");
            }

            if (!Directory.Exists(_location.Backup))
            {
                throw new ArgumentException("Diretório de backup inexistente");
            }
        }

        public void MakeCopy()
        {
            var arquivos = new List<string>();

            var arquivosACopiar = Directory.GetFiles(_location.Origem).Where(c => c.EndsWith(".txt") || c.EndsWith(".TXT"));

            foreach (var item in arquivosACopiar)
            {
                FileInfo arquivoOrigem = new FileInfo(item);
                if (arquivoOrigem.Exists)
                {
                    if (arquivoOrigem.Length > 0)
                    {
                        // destino
                        string arquivoDestino = Path.Combine(_location.Destino, arquivoOrigem.Name);
                        arquivoOrigem.CopyTo(arquivoDestino, true);

                        // backup
                        string arquivoBackup = GetNameArquivoBackup(arquivoDestino);
                        arquivoOrigem.CopyTo(arquivoBackup, true);

                        // lista
                        arquivos.Add(arquivoOrigem.FullName);

                        // log
                        log(arquivoOrigem.FullName);
                    }
                }
            }
        }

        private void log(string arquivo)
        {
            using (StreamWriter writer = File.AppendText(Path.Combine(_location.Backup, "Log.txt")))
            {
                writer.WriteLine("Arquivo " + arquivo + " copiado em " + DateTime.Now.ToString());
            }
        }

        private string GetNameArquivoBackup(string arquivoDestino)
        {
            int i = 0;
            string line;

            using (StreamReader reader = new StreamReader(arquivoDestino))
            {
                while ((line = reader.ReadLine()) != null && i < 2)
                {
                    if (i == 1)
                    {
                        string diretorioBackup = Path.Combine(_location.Backup, line.Substring(line.Length - 8, 8));
                        if (!Directory.Exists(diretorioBackup))
                        {
                            Directory.CreateDirectory(diretorioBackup);
                        }

                        FileInfo f = new FileInfo(arquivoDestino);
                        return Path.Combine(diretorioBackup, f.Name);
                    }
                    i++;
                }
            }

            return string.Empty;
        }
    }

    public class Location
    {
        public string Origem { get; set; }
        public string Destino { get; set; }
        public string Backup { get; set; }
    }
}
